namespace MediatorAuthService.Application.Common.Authorization;

/// <summary>
/// Catalog of every permission the API checks. Roles are composed from these values at runtime.
/// </summary>
public static class Permissions
{
    public static class Users
    {
        public const string Read = "Users.Read";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
        public const string AssignRoles = "Users.AssignRoles";
    }

    public static class Roles
    {
        public const string Read = "Roles.Read";
        public const string Create = "Roles.Create";
        public const string Update = "Roles.Update";
        public const string Delete = "Roles.Delete";
    }

    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new(Users.Read, "Users", "View users and their roles."),
        new(Users.Create, "Users", "Create users."),
        new(Users.Update, "Users", "Update other users and activate or deactivate them."),
        new(Users.Delete, "Users", "Delete users."),
        new(Users.AssignRoles, "Users", "Assign roles to users."),
        new(Roles.Read, "Roles", "View roles and the permission catalog."),
        new(Roles.Create, "Roles", "Create roles."),
        new(Roles.Update, "Roles", "Update roles and their permissions."),
        new(Roles.Delete, "Roles", "Delete roles."),
    ];

    private static readonly HashSet<string> Names = [.. All.Select(permission => permission.Name)];

    public static IReadOnlySet<string> AllNames => Names;

    public static bool Exists(string permission) => Names.Contains(permission);
}

public sealed record PermissionDefinition(string Name, string Group, string Description);
