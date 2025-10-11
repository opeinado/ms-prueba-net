using System;
using System.Threading;
using System.Threading.Tasks;
using CreditPro.Application.DTOs;

namespace CreditPro.Application.Services;

public interface ICreditApplicationService
{
    Task<CreditApplicationDto> CreateAsync(CreateCreditApplicationRequest request, CancellationToken cancellationToken = default);
    Task<CreditApplicationDto> UpdateStatusAsync(Guid applicationId, UpdateCreditApplicationStatusRequest request, CancellationToken cancellationToken = default);
    Task<CreditApplicationWithHistoryDto> GetByIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
}
