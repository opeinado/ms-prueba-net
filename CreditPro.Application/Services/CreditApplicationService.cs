using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using CreditPro.Application.DTOs;
using CreditPro.Application.Exceptions;
using CreditPro.Application.Interfaces;
using CreditPro.Domain.Entities;
using CreditPro.Domain.ValueObjects;

namespace CreditPro.Application.Services;

public class CreditApplicationService : ICreditApplicationService
{
    private const decimal MinimumAmount = 1000m;
    private const decimal MaximumAmount = 150000m;

    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly IAuditEventRepository _auditEventRepository;

    public CreditApplicationService(
        ICreditApplicationRepository creditApplicationRepository,
        IAuditEventRepository auditEventRepository)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _auditEventRepository = auditEventRepository;
    }

    public async Task<CreditApplicationDto> CreateAsync(CreateCreditApplicationRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreditAmount(request.CreditAmount);

        var applicationId = Guid.NewGuid();
        var status = CreditApplicationStatus.Received;

        var application = new CreditApplication(
            applicationId,
            request.CustomerId,
            request.CreditAmount,
            request.ApplicationDate,
            status,
            request.CollateralDescription);

        await _creditApplicationRepository.CreateAsync(application, cancellationToken).ConfigureAwait(false);

        await _auditEventRepository.AddAsync(
            new AuditEvent(
                applicationId,
                DateTime.UtcNow,
                eventType: "Creación",
                newState: status,
                details: new JsonObject
                {
                    ["creditAmount"] = request.CreditAmount,
                    ["applicationDate"] = request.ApplicationDate,
                    ["collateralDescription"] = request.CollateralDescription
                }),
            cancellationToken).ConfigureAwait(false);

        return MapToDto(application);
    }

    public async Task<CreditApplicationDto> UpdateStatusAsync(Guid applicationId, UpdateCreditApplicationStatusRequest request, CancellationToken cancellationToken = default)
    {
        ValidateStatus(request.NewStatus);

        var application = await _creditApplicationRepository.GetByIdAsync(applicationId, cancellationToken)
            .ConfigureAwait(false);

        if (application is null)
        {
            throw new NotFoundException($"No se encontró la solicitud con ID {applicationId}.");
        }

        application.UpdateStatus(request.NewStatus);
        await _creditApplicationRepository.UpdateStatusAsync(applicationId, request.NewStatus, cancellationToken)
            .ConfigureAwait(false);

        await _auditEventRepository.AddAsync(
            new AuditEvent(
                applicationId,
                DateTime.UtcNow,
                eventType: "Actualización de Estado",
                newState: request.NewStatus,
                details: new JsonObject
                {
                    ["notes"] = request.Notes
                }),
            cancellationToken).ConfigureAwait(false);

        return MapToDto(application);
    }

    public async Task<CreditApplicationWithHistoryDto> GetByIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var application = await _creditApplicationRepository.GetByIdAsync(applicationId, cancellationToken)
            .ConfigureAwait(false);

        if (application is null)
        {
            throw new NotFoundException($"No se encontró la solicitud con ID {applicationId}.");
        }

        var events = await _auditEventRepository.GetByApplicationIdAsync(applicationId, cancellationToken)
            .ConfigureAwait(false);

        return new CreditApplicationWithHistoryDto
        {
            Application = MapToDto(application),
            History = events
                .OrderBy(e => e.Timestamp)
                .Select(MapToDto)
                .ToList()
        };
    }

    private static CreditApplicationDto MapToDto(CreditApplication application) =>
        new()
        {
            ApplicationId = application.ApplicationId,
            CustomerId = application.CustomerId,
            CreditAmount = application.CreditAmount,
            ApplicationDate = application.ApplicationDate,
            Status = application.Status,
            CollateralDescription = application.CollateralDescription,
            DescripcionFinal = application.DescripcionFinal
        };

    private static AuditEventDto MapToDto(AuditEvent auditEvent) =>
        new()
        {
            Timestamp = auditEvent.Timestamp,
            EventType = auditEvent.EventType,
            NewState = auditEvent.NewState,
            Details = auditEvent.Details
        };

    private static void ValidateCreditAmount(decimal amount)
    {
        if (amount <= MinimumAmount || amount >= MaximumAmount)
        {
            throw new ValidationException($"El monto solicitado debe ser mayor a {MinimumAmount:N0} y menor a {MaximumAmount:N0}.");
        }
    }

    private static void ValidateStatus(string newStatus)
    {
        if (!CreditApplicationStatus.ValidStatuses.Contains(newStatus))
        {
            throw new ValidationException($"El estado '{newStatus}' no es válido.");
        }
    }
}
