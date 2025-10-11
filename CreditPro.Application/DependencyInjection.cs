using CreditPro.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CreditPro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreditApplicationService, CreditApplicationService>();
        return services;
    }
}
