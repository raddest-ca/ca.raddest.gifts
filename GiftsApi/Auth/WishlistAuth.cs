using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace GiftsApi.Auth;


// https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-7.0
public class WishlistAuthorizationCrudHandler :
    AuthorizationHandler<GroupScopedOperationAuthorizationRequirement, Wishlist>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   GroupScopedOperationAuthorizationRequirement requirement,
                                                   Wishlist resource)
    {
        var userId = context.User.GetUserId();
        if (userId == null)
        {
            context.Fail(new AuthorizationFailureReason(this, "user not found"));
            return Task.CompletedTask;
        }

        var isInGroup = requirement.Group.Members.Contains(userId.Value);
        if (!isInGroup)
        {
            context.Fail(new AuthorizationFailureReason(this, "user is not a member of the group"));
            return Task.CompletedTask;
        }

        if (requirement.Requirement == CrudRequirements.Update || requirement.Requirement == CrudRequirements.Delete)
        {
            if (!resource.Owners.Contains(userId.Value))
            {
                context.Fail(new AuthorizationFailureReason(this, "user is not an owner of the wishlist"));
                return Task.CompletedTask;
            }
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}