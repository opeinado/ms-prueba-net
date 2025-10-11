namespace CreditPro.Application.DTOs;

public class UpdateCreditApplicationStatusRequest
{
    public string NewStatus { get; set; } = null!;
    public string? Notes { get; set; }
}
