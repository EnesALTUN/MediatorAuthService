using MediatorAuthService.Application.Cqrs.Commands.AuthCommands;
using MediatorAuthService.Application.Cqrs.Commands.RoleCommands;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Validators.AuthValidators;
using MediatorAuthService.Application.Validators.RoleValidators;
using MediatorAuthService.Application.Validators.UserValidators;

namespace MediatorAuthService.UnitTests;

public class ValidatorTests
{
    [Theory]
    [InlineData("weak", false)]
    [InlineData("alllowercase1!", false)]
    [InlineData("Passw0rd!", true)]
    public void CreateUser_EnforcesThePasswordPolicy(string password, bool isValid)
    {
        CreateUserCommand command = new() { Name = "John", Surname = "Doe", Email = "john@example.com", Password = password };

        Assert.Equal(isValid, new CreateUserValidator().Validate(command).IsValid);
    }

    [Fact]
    public void ChangePassword_EnforcesThePolicy_AndRejectsTheSamePassword()
    {
        ChangePasswordUserValidator validator = new();

        Assert.False(validator.Validate(new ChangePasswordUserCommand("Passw0rd!", "weak")).IsValid);
        Assert.False(validator.Validate(new ChangePasswordUserCommand("Passw0rd!", "Passw0rd!")).IsValid);
        Assert.True(validator.Validate(new ChangePasswordUserCommand("Passw0rd!", "N3wPassw0rd!")).IsValid);
    }

    [Fact]
    public void Login_DoesNotDiscloseThePasswordPolicy()
    {
        Assert.True(new LoginValidator().Validate(new LoginCommand { Email = "john@example.com", Password = "weak" }).IsValid);
    }

    [Fact]
    public void UpdateUser_ValidatesTheEmailFormat()
    {
        UpdateUserCommand command = new() { Name = "John", Surname = "Doe", Email = "not-an-email" };

        Assert.False(new UpdateUserValidation().Validate(command).IsValid);
    }

    [Theory]
    [InlineData(1, 100, null, true)]
    [InlineData(1, 101, null, false)]
    [InlineData(0, 10, null, false)]
    [InlineData(1, 10, "Descending", true)]
    [InlineData(1, 10, "random", false)]
    public void GetUserAll_LimitsPaging(int pageId, int pageSize, string? orderType, bool isValid)
    {
        GetUserAllQuery query = new() { PageId = pageId, PageSize = pageSize, OrderType = orderType };

        Assert.Equal(isValid, new GetUserAllValidator().Validate(query).IsValid);
    }

    [Fact]
    public void CreateRole_RejectsUnknownPermissions()
    {
        CreateRoleValidator validator = new();

        Assert.False(validator.Validate(new CreateRoleCommand { Name = "Role", Permissions = ["Users.Everything"] }).IsValid);
        Assert.True(validator.Validate(new CreateRoleCommand { Name = "Role", Permissions = ["Users.Read"] }).IsValid);
    }
}
