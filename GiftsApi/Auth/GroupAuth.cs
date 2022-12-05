using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace GiftsApi.Auth;

public class PasswordRequirement: IAuthorizationRequirement {
    public string Name = nameof(PasswordRequirement);
    public string UserProvidedPassword { get; set; }
}

public static class CrudRequirements
{
    public static OperationAuthorizationRequirement Create =
        new OperationAuthorizationRequirement { Name = nameof(Create) };
    public static OperationAuthorizationRequirement Read =
        new OperationAuthorizationRequirement { Name = nameof(Read) };
    public static OperationAuthorizationRequirement Update =
        new OperationAuthorizationRequirement { Name = nameof(Update) };
    public static OperationAuthorizationRequirement Delete =
        new OperationAuthorizationRequirement { Name = nameof(Delete) };
}

public class GroupAuthorizationPasswordHandler : AuthorizationHandler<PasswordRequirement, Group>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PasswordRequirement requirement,
        Group resource
    ) {
        var userId = context.User.GetUserId();
        if (userId == null) return Task.CompletedTask;
        if (BC.Verify(requirement.UserProvidedPassword, resource.Password)) {
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
        if (userId == null) return Task.CompletedTask;
        if (requirement == CrudRequirements.Create) {
            context.Succeed(requirement);
        } else if (requirement == CrudRequirements.Read) {
            if (resource.Members.Contains(userId.Value)) {
                context.Succeed(requirement);
            }
        } else if (requirement == CrudRequirements.Update) {
            if (resource.Members.Contains(userId.Value)) {
                context.Succeed(requirement);
            }
        } else if (requirement == CrudRequirements.Delete) {
            if (resource.Members.Contains(userId.Value)) {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}