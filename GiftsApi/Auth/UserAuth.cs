using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace GiftsApi.Auth;


public class UserAuthorizationCrudHandler :
    AuthorizationHandler<OperationAuthorizationRequirement, User>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   OperationAuthorizationRequirement requirement,
                                                   User resource)
    {
        var userId = context.User.GetUserId();
        if (userId == null)
        {
            context.Fail(new AuthorizationFailureReason(this, "User not found"));
            return Task.CompletedTask;
        }

        if (requirement == CrudRequirements.Create)
        {
            context.Succeed(requirement);
        }
        else if (requirement == CrudRequirements.Read)
        {
            context.Succeed(requirement);
        }
        else if (requirement == CrudRequirements.Update)
        {
            if (resource.Id == userId.Value)
            {
                context.Succeed(requirement);
            }
        }
        else if (requirement == CrudRequirements.Delete)
        {
            if (resource.Id == userId.Value)
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }
}