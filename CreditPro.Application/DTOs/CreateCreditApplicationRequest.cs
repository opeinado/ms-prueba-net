using System;

namespace CreditPro.Application.DTOs;

public class CreateCreditApplicationRequest
{
    public string CustomerId { get; set; } = null!;
    public decimal CreditAmount { get; set; }
    public DateTime ApplicationDate { get; set; }
    public string? CollateralDescription { get; set; }
}
