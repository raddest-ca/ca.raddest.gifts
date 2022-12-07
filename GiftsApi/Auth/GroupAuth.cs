using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace GiftsApi.Auth;


public class JoinGroupAuthorizationHandler : AuthorizationHandler<JoinGroupAuthorizationRequirement, Group>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        JoinGroupAuthorizationRequirement requirement,
        Group group
    )
    {
        var userId = context.User.GetUserId();
        if (userId == null) 
        {
            context.Fail(new AuthorizationFailureReason(this, "User not found"));
            return Task.CompletedTask;
        }
        if (group.Members.Contains(userId!.Value)) {
            context.Fail(new AuthorizationFailureReason(this, "Already a member"));
            return Task.CompletedTask;
        }
        if (BC.Verify(requirement.UserProvidedPassword, group.Password))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

// https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-7.0
public class GroupAuthorizationCrudHandler :
    AuthorizationHandler<OperationAuthorizationRequirement, Group>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   OperationAuthorizationRequirement requirement,
                                                   Group resource)
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
            if (resource.Members.Contains(userId.Value))
            {
                context.Succeed(requirement);
            }
        }
        else if (requirement == CrudRequirements.Update)
        {
            if (resource.Members.Contains(userId.Value))
            {
                context.Succeed(requirement);
            }
        }
        else if (requirement == CrudRequirements.Delete)
        {
            if (resource.Members.Contains(userId.Value))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}