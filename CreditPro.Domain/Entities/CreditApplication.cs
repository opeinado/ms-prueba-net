using System;

namespace CreditPro.Domain.Entities;

public class CreditApplication
{
    private CreditApplication()
    {
    }

    public CreditApplication(Guid applicationId, string customerId, decimal creditAmount, DateTime applicationDate, string status, string? collateralDescription = null)
    {
        ApplicationId = applicationId;
        CustomerId = customerId;
        CreditAmount = creditAmount;
        ApplicationDate = applicationDate;
        Status = status;
        CollateralDescription = collateralDescription;
    }

    public Guid ApplicationId { get; private set; }
    public string CustomerId { get; private set; } = null!;
    public decimal CreditAmount { get; private set; }
    public DateTime ApplicationDate { get; private set; }
    public string Status { get; private set; } = null!;
    public string? CollateralDescription { get; private set; }
    public string DescripcionFinal { get; private set; } = null!;

    public void UpdateStatus(string newStatus)
    {
        Status = newStatus;
    }
}
