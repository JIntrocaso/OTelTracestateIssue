using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureOpenTelemetryExtensions
{
    public static OpenTelemetryBuilder ConfigureOpenTelemetry(this IServiceCollection services, IHostEnvironment hostEnvironment)
    {
        return services.AddOpenTelemetry()
            .ConfigureResource(r =>
            {
                r.AddDetector(new EnvironmentDetector(hostEnvironment));
                r.AddEnvironmentVariableDetector();
            })
            .WithTracing(builder =>
            {
                builder
                    // Ensure the TracerProvider subscribes to any custom ActivitySources.
                    .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                    .AddHttpClientInstrumentation(o => o.RecordException = true)
                    .AddOtlpExporter();

            })
            .WithMetrics(builder =>
            {
                builder
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    //.AddConsoleExporter()
                    .AddOtlpExporter();
            });
    }

    internal class EnvironmentDetector : IResourceDetector
    {
        public const string AttributeServiceName = "service.name";
        public const string AttributeServiceInstance = "service.instance.id";
        public const string AttributeServiceVersion = "service.version";

        private readonly IHostEnvironment _env;

        public EnvironmentDetector(IHostEnvironment env)
        {
            _env = env;
        }

        public Resource Detect()
        {
            var attributes = new Dictionary<string, object>
            {
                { AttributeServiceName, _env.ApplicationName },
                { AttributeServiceInstance, Environment.MachineName },
                { "application", _env.ApplicationName },
                { "deployment.environment", _env.EnvironmentName },
                { AttributeServiceVersion, Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown" }
            };

            return new Resource(attributes);
        }
    }
}
