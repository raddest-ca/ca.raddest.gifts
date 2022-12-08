using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiftsApi.Extensions;

public static class AuthorizationResultExtensions
{
    public static IActionResult ToActionResult(this AuthorizationResult result)
    {
        if (result.Succeeded) return new OkResult();
        return new BadRequestObjectResult(new ProblemDetails{
            Title = "Authorization failed",
            Status = StatusCodes.Status403Forbidden,
            Detail = string.Join(", ", result.Failure?.FailureReasons.Select(x => x.Message) ?? new string[0])
        });
    }
}