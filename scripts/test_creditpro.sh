#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
COMPOSE_FILE="${REPO_ROOT}/docker-compose.yml"
API_URL="http://localhost:5180"
API_PID=""
TMP_DIR="${REPO_ROOT}/.tmp_test_creditpro"
LOG_FILE="${TMP_DIR}/api.log"

cleanup() {
  local exit_code=$?

  if [[ -n "${API_PID}" ]] && kill -0 "${API_PID}" >/dev/null 2>&1; then
    kill "${API_PID}" >/dev/null 2>&1 || true
    wait "${API_PID}" >/dev/null 2>&1 || true
  fi

  if command -v docker >/dev/null 2>&1; then
    docker compose -f "${COMPOSE_FILE}" down >/dev/null 2>&1 || true
  fi

  rm -rf "${TMP_DIR}" >/dev/null 2>&1 || true
  exit "${exit_code}"
}
trap cleanup EXIT

require_command() {
  local cmd="$1"
  if ! command -v "${cmd}" >/dev/null 2>&1; then
    echo "Error: se requiere el comando '${cmd}' para ejecutar este script." >&2
    exit 1
  fi
}

wait_for_http() {
  local url="$1"
  local retries="${2:-30}"
  local delay="${3:-2}"

  for ((i = 0; i < retries; i++)); do
    if curl -sSf "${url}" >/dev/null 2>&1; then
      return 0
    fi
    sleep "${delay}"
  done

  echo "Error: el endpoint ${url} no respondió tras $(("${retries}" * "${delay}")) segundos." >&2
  echo "Últimas líneas del log de la API:"
  tail -n 50 "${LOG_FILE}" || true
  return 1
}

require_command dotnet
require_command docker
require_command curl
require_command python3

cd "${REPO_ROOT}"

echo "==> Compilando la solución"
dotnet build CreditPro.sln >/dev/null

echo "==> Ejecutando pruebas unitarias"
dotnet test CreditPro.sln >/dev/null

echo "==> Iniciando infraestructura con Docker Compose"
docker compose -f "${COMPOSE_FILE}" up -d postgres dynamodb dynamodb-init >/dev/null

echo "==> Publicando e iniciando la API en segundo plano"
ASPNETCORE_ENVIRONMENT=Development \
ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=creditpro;Username=postgres;Password=postgres" \
DynamoDb__TableName="creditpro-audit-events" \
DynamoDb__Region="us-east-1" \
DynamoDb__ServiceUrl="http://localhost:8000" \
dotnet run --project CreditPro.Api/CreditPro.Api.csproj --urls "${API_URL}" >"${LOG_FILE}" 2>&1 &
API_PID=$!

echo "==> Esperando a que la API responda"
wait_for_http "${API_URL}/swagger/index.html" 60 2

invalid_payload='{"customerId":"cust-invalid","creditAmount":500,"applicationDate":"2025-01-10T10:30:00Z"}'
invalid_code=$(curl -sS -o "${TMP_DIR}/invalid.json" -w "%{http_code}" \
  -H "Content-Type: application/json" \
  -d "${invalid_payload}" \
  "${API_URL}/api/credit-applications")

if [[ "${invalid_code}" -ne 400 ]]; then
  echo "Error: se esperaba HTTP 400 para monto inválido, se obtuvo ${invalid_code}" >&2
  exit 1
fi
echo "==> Validación correcta para montos fuera de rango (HTTP 400)"

create_payload='{"customerId":"cust-001","creditAmount":12000,"applicationDate":"2025-01-10T10:30:00Z","collateralDescription":"Vehículo Subaru 2023"}'
create_code=$(curl -sS -o "${TMP_DIR}/create.json" -w "%{http_code}" \
  -H "Content-Type: application/json" \
  -d "${create_payload}" \
  "${API_URL}/api/credit-applications")

if [[ "${create_code}" -ne 201 ]]; then
  echo "Error: se esperaba HTTP 201 al crear la solicitud, se obtuvo ${create_code}" >&2
  exit 1
fi

application_id=$(python3 - <<'PY'
import json, sys
data=json.load(open(sys.argv[1]))
print(data["applicationId"])
PY
"${TMP_DIR}/create.json")

status_payload='{"newStatus":"En Análisis","notes":"Se requiere documentación adicional"}'
status_code=$(curl -sS -o "${TMP_DIR}/status.json" -w "%{http_code}" \
  -H "Content-Type: application/json" \
  -d "${status_payload}" \
  "${API_URL}/api/credit-applications/${application_id}/status")

if [[ "${status_code}" -ne 200 ]]; then
  echo "Error: se esperaba HTTP 200 al actualizar estado, se obtuvo ${status_code}" >&2
  exit 1
fi

echo "==> Solicitud creada y estado actualizado correctamente (applicationId=${application_id})"

get_code=$(curl -sS -o "${TMP_DIR}/get.json" -w "%{http_code}" \
  "${API_URL}/api/credit-applications/${application_id}")

if [[ "${get_code}" -ne 200 ]]; then
  echo "Error: se esperaba HTTP 200 al consultar la solicitud, se obtuvo ${get_code}" >&2
  exit 1
fi

python3 - <<'PY'
import json, sys

with open(sys.argv[1]) as f:
    data = json.load(f)

status = data["application"]["status"]
history = data["history"]

if status != "En Análisis":
    raise SystemExit(f"Estado actual inesperado: {status}")

if len(history) < 2:
    raise SystemExit(f"Se esperaban al menos 2 eventos en el historial, se obtuvo {len(history)}")

types = {evt["eventType"] for evt in history}
expected = {"Creación", "Actualización de Estado"}

if not expected.issubset(types):
    raise SystemExit(f"El historial no contiene todos los eventos esperados. Tipos encontrados: {types}")
PY
"${TMP_DIR}/get.json"

echo "==> Historial de eventos verificado correctamente"
echo "==> Pruebas completas: todos los requisitos funcionales validados"
mkdir -p "${TMP_DIR}"
