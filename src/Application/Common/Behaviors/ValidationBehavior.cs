using System.Reflection;
using Domain.Common;
using FluentValidation;
using MediatR;

namespace Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        // Map FluentValidation failures → your Error model
        var errors = failures
            .Select(f => Error.Validation(
                string.IsNullOrWhiteSpace(f.ErrorCode) ? "Validation.Failed" : f.ErrorCode,
                f.ErrorMessage))
            .ToList();

        // Return Result / Result<T> failure instead of throwing
        return CreateValidationResult(errors);
    }

    private static TResponse CreateValidationResult(List<Error> errors)
    {
        // Result (non-generic)
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(errors);

        // Result<T>
        if (typeof(TResponse).IsGenericType
            && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(TResponse);
            var failureMethod = resultType.GetMethod(
                nameof(Result.Failure),
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(IReadOnlyList<Error>)],
                modifiers: null);

            if (failureMethod is null)
                throw new InvalidOperationException(
                    $"Could not find Failure(IReadOnlyList<Error>) on {resultType.Name}.");

            return (TResponse)failureMethod.Invoke(null, [errors])!;
        }

        // Handlers that don't return Result still throw (or change this if you want)
        throw new ValidationException(
            errors.Select(e => new FluentValidation.Results.ValidationFailure
            {
                ErrorCode = e.Code,
                ErrorMessage = e.Message
            }));
    }
}