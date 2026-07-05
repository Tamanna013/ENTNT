using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FleetMind.Api.Middleware;

/// <summary>
/// Global action filter that automatically runs FluentValidation validators
/// for every action parameter that has a registered IValidator&lt;T&gt; in DI.
/// Short-circuits with a 400 Bad Request containing per-field error messages
/// if validation fails — individual controllers never need to manually
/// call validators or check ModelState.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            var validator = _serviceProvider.GetService(validatorType);

            if (validator is null)
                continue;

            var validateMethod = validatorType.GetMethod("ValidateAsync",
                new[] { argumentType, typeof(CancellationToken) });

            if (validateMethod is null)
                continue;

            var validationResult = await (Task<FluentValidation.Results.ValidationResult>)
                validateMethod.Invoke(validator, new[] { argument, CancellationToken.None })!;

            if (!validationResult.IsValid)
            {
                var fieldErrors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                foreach (var kvp in fieldErrors)
                {
                    errors[kvp.Key] = kvp.Value;
                }
            }
        }

        if (errors.Count > 0)
        {
            context.Result = new BadRequestObjectResult(new
            {
                traceId = context.HttpContext.TraceIdentifier,
                statusCode = 400,
                message = "One or more validation errors occurred.",
                errors
            });
            return;
        }

        await next();
    }
}
