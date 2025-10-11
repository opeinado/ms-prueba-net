using System.Collections.Generic;

namespace CreditPro.Application.DTOs;

public class CreditApplicationWithHistoryDto
{
    public CreditApplicationDto Application { get; set; } = null!;
    public IReadOnlyCollection<AuditEventDto> History { get; set; } = new List<AuditEventDto>();
}
