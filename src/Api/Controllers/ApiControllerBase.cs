using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Unwraps a Result&lt;T&gt;: 200 OK with the value on success,
    /// or the mapped ProblemDetails response on failure.
    /// </summary>
    protected ActionResult<T> HandleResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : HandleFailure(result);

    /// <summary>
    /// Unwraps a Result with no value: 204 No Content on success,
    /// or the mapped ProblemDetails response on failure.
    /// </summary>
    protected ActionResult HandleResult(Result result) =>
        result.IsSuccess ? NoContent() : HandleFailure(result);

    /// <summary>
    /// Maps a failed Result to the matching ProblemDetails + HTTP status.
    /// Only called internally by HandleResult once IsFailure is confirmed.
    /// </summary>
    protected ActionResult HandleFailure(Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("HandleFailure was called on a successful Result.");

        var error = result.Errors[0];

        var problem = new ProblemDetails
        {
            Title = error.Code,
            Detail = error.Message
        };

        if (result.Errors.Count > 1)
            problem.Extensions["errors"] = result.Errors.Select(e => new { e.Code, e.Message });

        return error.Type switch
        {
            ErrorType.NotFound => NotFound(problem),
            ErrorType.Conflict => Conflict(problem),
            ErrorType.Unauthorized => Unauthorized(problem),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, problem),
            _ => BadRequest(problem) // Validation and Failure both surface as 400
        };
    }
}
