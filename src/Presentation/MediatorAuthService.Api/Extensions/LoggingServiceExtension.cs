using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using MediatorAuthService.Application.Dtos.ConfigurationDtos;
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
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> instance to which the logging configuration will be applied.</param>
    /// <param name="configuration">The application configuration containing Elasticsearch connection settings.</param>
    public static void AddElasticSearchLogging(this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        var esConfig = configuration
            .GetSection("ElasticSearch")
            .Get<ElasticSearchConfiguration>()!;

        hostBuilder.UseSerilog((context, services, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", "MediatorAuthService")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.Async(a => a.Elasticsearch(
                    nodes: [new Uri(esConfig.Uri)],
                    opts =>
                    {
                        opts.DataStream = new DataStreamName("logs", "mediator-auth-service");
                        opts.BootstrapMethod = BootstrapMethod.Failure;
                    }
                ), bufferSize: 1000);
        });
    }
}