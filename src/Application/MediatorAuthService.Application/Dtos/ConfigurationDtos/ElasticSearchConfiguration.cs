namespace MediatorAuthService.Application.Dtos.ConfigurationDtos;

public class ElasticSearchConfiguration
{
    public string Uri { get; set; } = default!;
    public string IndexFormat { get; set; } = default!;
    public string? Username { get; set; }
    public string? Password { get; set; }
}