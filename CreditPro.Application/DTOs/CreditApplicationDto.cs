using System;

namespace CreditPro.Application.DTOs;

public class CreditApplicationDto
{
    public Guid ApplicationId { get; set; }
    public string CustomerId { get; set; } = null!;
    public decimal CreditAmount { get; set; }
    public DateTime ApplicationDate { get; set; }
    public string Status { get; set; } = null!;
    public string? CollateralDescription { get; set; }
}
