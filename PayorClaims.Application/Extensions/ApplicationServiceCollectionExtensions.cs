using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PayorClaims.Application.Dtos.Claims;
using PayorClaims.Application.Validation;

namespace PayorClaims.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ClaimSubmissionInputValidator>();
        return services;
    }
}
