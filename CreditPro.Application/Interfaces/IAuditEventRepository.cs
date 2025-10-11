using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CreditPro.Domain.Entities;

namespace CreditPro.Application.Interfaces;

public interface IAuditEventRepository
{
    Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AuditEvent>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
}
