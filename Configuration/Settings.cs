namespace testproj.Configuration;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

public static class SettingsFactory
{
    public static IConfiguration Create(IConfiguration? configuration = null)
    {
        var conf = configuration ?? new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile("appsettings.json", optional: false)
                                        .AddJsonFile("appsettings.development.json", optional: true)
                                        .AddEnvironmentVariables()
                                        .Build();

        return conf;
    }
}

public abstract class Settings
{
    public static T Load<T>(string key, IConfiguration configuration = null)
    {
        var settings = (T)Activator.CreateInstance(typeof(T));

        SettingsFactory.Create(configuration).GetSection(key).Bind(settings, (x) => { x.BindNonPublicProperties = true; });

        return settings;
    }
}

public class ApiSpecialSettings
{
    public string HelloMessage { get; private set; }
}

public static class Bootstrapper
{
    public static IServiceCollection AddMainSettings(this IServiceCollection services, IConfiguration configuration = null)
    {
        var settings = Settings.Load<MainSettings>("Main", configuration);
        services.AddSingleton(settings);

        return services;
    }

    public static IServiceCollection AddSwaggerSettings(this IServiceCollection services, IConfiguration configuration = null)
    {
        var settings = Settings.Load<SwaggerSettings>("Swagger", configuration);
        services.AddSingleton(settings);

        return services;
    }

    public static IServiceCollection AddApiSpecialSettings(this IServiceCollection services, IConfiguration configuration = null)
    {
        var settings = Settings.Load<ApiSpecialSettings>("ApiSpecial", configuration);
        services.AddSingleton(settings);

        return services;
    }
    public static IServiceCollection RegisterAppServices(this IServiceCollection services)
    {
        services
            .AddMainSettings()
            .AddSwaggerSettings()
            .AddApiSpecialSettings()
            ;

        return services;
    }
}
public static class ControllerAndViewsConfiguration
{
    public static IServiceCollection AddAppControllerAndViews(this IServiceCollection services)
    {
        services
            .AddRazorPages();

        services
            .AddControllers()
            .AddNewtonsoftJson();

        return services;
    }

    public static IEndpointRouteBuilder UseAppControllerAndViews(this IEndpointRouteBuilder app)
    {
        app.MapRazorPages();
        app.MapControllers();

        return app;
    }
}
public static class VersioningConfiguration
{
    public static IServiceCollection AddAppVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;

            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
