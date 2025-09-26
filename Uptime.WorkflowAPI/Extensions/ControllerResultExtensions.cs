using Microsoft.AspNetCore.Mvc;
using Uptime.Workflows.Core.Common;

namespace Uptime.Workflows.Api.Extensions;

public static class ControllerResultExtensions
{
    /// <summary>
    /// Maps a possibly‐null query result to either 404 or 200.
    /// </summary>
    public static ActionResult<TDst> ToActionResult<TDst>(this ControllerBase controller, TDst? value) where TDst : class
    {
        if (value is null)
        {
            return controller.NotFound();
        }

        return controller.Ok(value);
    }

    public static ActionResult ToActionResult<TSource, TDst>(this ControllerBase c, Result<TSource> result, Func<TSource, TDst> map)
    {
        return result.Succeeded 
            ? c.Ok(map(result.Value!)) 
            : c.ToActionResult(result); // delegate back to your single‐value handler
    }

    public static ActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.Succeeded)
        {
            // For a Unit result we return 204 No Content
            if (typeof(T) == typeof(Unit))
                return controller.NoContent();

            // Otherwise 200 OK with the value
            return controller.Ok(result.Value);
        }
        
        // Failure path: switch on error code
        switch (result.Code)
        {
            case ErrorCode.NotFound:
                return controller.NotFound(new { result.Details });
            case ErrorCode.Conflict:
                return controller.Conflict(new { result.Details });
            case ErrorCode.Forbidden:
                return controller.Forbid();
            case ErrorCode.Validation:
                return controller.BadRequest(new { result.Details });
            case ErrorCode.Cancelled:
                return controller.StatusCode(499, new { result.Details }); // or 400
            default:
                // unexpected → 500
                return controller.Problem(detail: result.Details);
        }
    }
}