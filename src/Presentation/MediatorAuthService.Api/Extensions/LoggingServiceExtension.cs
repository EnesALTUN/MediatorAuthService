using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using MediatorAuthService.Api.Configuration;
using Serilog;

namespace MediatorAuthService.Api.Extensions;

/// <summary>
/// Contains extension methods that configure Serilog-based Elasticsearch logging infrastructure.
/// </summary>
public static class LoggingServiceExtension
{
    /// <summary>
    /// Adds Serilog with Elasticsearch logging integration to the application's host configuration.
    /// Configures Console and Elasticsearch sinks, and enriches log records with
    /// machine name, environment name, and application name metadata.
    /// The Elasticsearch sink is skipped when ElasticSearch:Uri is empty; basic authentication is used
    /// when ElasticSearch:Username and ElasticSearch:Password are set.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> instance to which the logging configuration will be applied.</param>
    public static void AddElasticSearchLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, services, loggerConfig) =>
        {
            ElasticSearchConfiguration esConfig = context.Configuration
                .GetSection("ElasticSearch")
                .Get<ElasticSearchConfiguration>() ?? new ElasticSearchConfiguration();

            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", "MediatorAuthService")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

            if (string.IsNullOrWhiteSpace(esConfig.Uri))
                return;

            loggerConfig.WriteTo.Async(a => a.Elasticsearch(
                nodes: [new Uri(esConfig.Uri)],
                opts =>
                {
                    opts.DataStream = new DataStreamName("logs", "mediator-auth-service");
                    // An unreachable Elasticsearch must not prevent the service from starting.
                    opts.BootstrapMethod = BootstrapMethod.Silent;
                },
                transport =>
                {
                    if (!string.IsNullOrWhiteSpace(esConfig.Username) && !string.IsNullOrWhiteSpace(esConfig.Password))
                        transport.Authentication(new BasicAuthentication(esConfig.Username, esConfig.Password));
                }
            ), bufferSize: 1000);
        });
    }
}
