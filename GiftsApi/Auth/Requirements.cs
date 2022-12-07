using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace GiftsApi.Auth;

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

public class JoinGroupAuthorizationRequirement : IAuthorizationRequirement
{
    public string Name = nameof(JoinGroupAuthorizationRequirement);
    public string UserProvidedPassword { get; set; }
}

public class GroupScopedOperationAuthorizationRequirement : IAuthorizationRequirement
{
    public Group Group { get; set; }
    public OperationAuthorizationRequirement Requirement { get; set; }
}