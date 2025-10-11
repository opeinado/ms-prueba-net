using System;
using System.Threading;
using System.Threading.Tasks;
using CreditPro.Domain.Entities;

namespace CreditPro.Application.Interfaces;

public interface ICreditApplicationRepository
{
    Task CreateAsync(CreditApplication application, CancellationToken cancellationToken = default);
    Task<CreditApplication?> GetByIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid applicationId, string status, CancellationToken cancellationToken = default);
}
