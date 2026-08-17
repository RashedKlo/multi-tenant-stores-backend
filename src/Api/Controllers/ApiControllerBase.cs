using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;
// Namespace/folder is a guess based on your DiscoveryController — rename to match
// your actual API project if it differs.
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Maps a failed Result to the matching ProblemDetails + HTTP status.
    /// Call only when result.IsFailure — e.g.
    /// <c>return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);</c>
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
            _ => BadRequest(problem) // Validation and Failure both surface as 400
        };
    }
}