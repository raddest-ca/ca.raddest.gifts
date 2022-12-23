using System.Linq;
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
        // we don't use bcrypt since group passwords are stored plaintext
        if (requirement.UserProvidedPassword == group.Password)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}


public class ModifyGroupUserAuthorizationHandler : AuthorizationHandler<ModifyGroupUserAuthorizationRequirement, Group>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ModifyGroupUserAuthorizationRequirement requirement,
        Group group
    )
    {
        var userId = context.User.GetUserId();
        if (userId == null) 
        {
            context.Fail(new AuthorizationFailureReason(this, "User not found"));
            return Task.CompletedTask;
        }
        if (requirement.Requirement == CrudRequirements.Delete && requirement.UserId == userId.Value)
        {
            // members can leave
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        
        if (group.Owners.Contains(userId.Value))
        {
            // owners can do anything
            context.Succeed(requirement);
            return Task.CompletedTask;
        } else {
            context.Fail(new AuthorizationFailureReason(this, "User not an owner of the group"));
            return Task.CompletedTask;
        }
    }
}

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