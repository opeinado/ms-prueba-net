namespace CreditPro.Domain.ValueObjects;

public static class CreditApplicationStatus
{
    public const string Received = "Recibida";
    public const string Approved = "Aprobada";
    public const string Rejected = "Rechazada";
    public const string InReview = "En Análisis";

    public static readonly string[] ValidStatuses = { Approved, Rejected, InReview };
}
