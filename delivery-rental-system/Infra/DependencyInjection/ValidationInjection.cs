using delivery_rental_system.Application.Handlers.Validators;
using delivery_rental_system.Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;

namespace delivery_rental_system.Infra.DependencyInjection;

public static class ValidationInjection
{
    public static IServiceCollection AddValidationLayer(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation(o =>
        {
            o.DisableDataAnnotationsValidation = true;
        });

        services.AddValidatorsFromAssemblyContaining<CreateMotorcycleCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<DeleteMotorcycleCommandValidator>();

        services.AddValidatorsFromAssemblyContaining<CreateDeliveryManCommandValidator>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}