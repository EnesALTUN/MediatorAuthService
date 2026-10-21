using MediatorAuthService.Application.Common.Security;
using MediatorAuthService.Application.Cqrs.Commands.AuthCommands;

namespace MediatorAuthService.UnitTests;

public class LogSanitizerTests
{
    private sealed class SampleRequest
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid Id { get; set; } = Guid.NewGuid();

        public List<string> Tags { get; set; } = ["a", "b"];

        public Dictionary<string, int> Counts { get; set; } = new() { ["x"] = 1 };

        public SampleRequest? Child { get; set; }
    }

    [Fact]
    public void Sanitize_RedactsSensitiveData_AndMasksPersonalData()
    {
        var result = Assert.IsType<Dictionary<string, object?>>(
            LogSanitizer.Sanitize(new LoginCommand { Email = "john@example.com", Password = "Secret1!" }));

        Assert.Equal(LogSanitizer.Redacted, result["Password"]);
        Assert.Equal("j***@example.com", result["Email"]);
    }

    [Fact]
    public void Sanitize_HandlesDates_Collections_AndSelfReferences()
    {
        SampleRequest request = new();
        request.Child = request;

        var result = Assert.IsType<Dictionary<string, object?>>(LogSanitizer.Sanitize(request));

        Assert.Equal(request.CreatedAt, result["CreatedAt"]);
        Assert.Equal(request.Id, result["Id"]);
        Assert.Equal(["a", "b"], Assert.IsType<List<object?>>(result["Tags"]));
        Assert.NotNull(result["Counts"]);
        Assert.NotNull(result["Child"]);
    }

    [Theory]
    [InlineData("john@example.com", "j***@example.com")]
    [InlineData("john", "j***")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void Mask_KeepsOnlyTheFirstCharacterAndDomain(string? value, string? expected)
    {
        Assert.Equal(expected, LogSanitizer.Mask(value));
    }
}
