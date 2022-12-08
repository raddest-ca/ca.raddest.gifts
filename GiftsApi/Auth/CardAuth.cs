using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace GiftsApi.Auth;


// https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-7.0
public class CardAuthorizationCrudHandler :
    AuthorizationHandler<WishlistScopedOperationAuthorizationRequirement, Card>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   WishlistScopedOperationAuthorizationRequirement requirement,
                                                   Card resource)
    {
        var userId = context.User.GetUserId();
        if (userId == null)
        {
            context.Fail(new AuthorizationFailureReason(this, "user not found"));
            return Task.CompletedTask;
        }

        // anyone in the group can modify cards on any wishlist within the group
        // unless the user is an owner of the wishlist and the card is marked as hidden from the list owners

        var isInGroup = requirement.Group.Members.Contains(userId.Value);
        if (!isInGroup)
        {
            context.Fail(new AuthorizationFailureReason(this, "user is not a member of the group"));
            return Task.CompletedTask;
        }

        var isWishlistOwner = requirement.Wishlist.Owners.Contains(userId.Value);
        if (!resource.VisibleToListOwner && isWishlistOwner)
        {
            context.Fail(new AuthorizationFailureReason(this, "user is not allowed to view this card"));
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}