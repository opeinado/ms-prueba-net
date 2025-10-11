using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CreditPro.Application.Interfaces;
using CreditPro.Domain.Entities;
using CreditPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CreditPro.Infrastructure.Repositories;

public class CreditApplicationRepository : ICreditApplicationRepository
{
    private readonly CreditProDbContext _dbContext;

    public CreditApplicationRepository(CreditProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(CreditApplication application, CancellationToken cancellationToken = default)
    {
        await _dbContext.CreditApplications.AddAsync(application, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<CreditApplication?> GetByIdAsync(Guid applicationId, CancellationToken cancellationToken = default) =>
        _dbContext.CreditApplications.AsNoTracking().FirstOrDefaultAsync(x => x.ApplicationId == applicationId, cancellationToken);

    public async Task UpdateStatusAsync(Guid applicationId, string status, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _dbContext.CreditApplications
            .Where(x => x.ApplicationId == applicationId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Status, status), cancellationToken)
            .ConfigureAwait(false);

        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"No se pudo actualizar la solicitud {applicationId}.");
        }
    }
}
