namespace testproj.Configuration;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

public class SwaggerSettings
{
    public bool Enabled { get; private set; }

    public string OAuthClientId { get; private set; }
    public string OAuthClientSecret { get; private set; }

    public SwaggerSettings()
    {
        Enabled = false;
    }
}

public class MainSettings
{
    public string MainUrl { get; private set; }
}

public static class SwaggerConfiguration
{
    private static string AppTitle = "Api";
    public static IServiceCollection AddAppSwagger(this IServiceCollection services, MainSettings mainSettings, SwaggerSettings swaggerSettings)
    {
        if (!swaggerSettings.Enabled)
            return services;

        services
            .AddOptions<SwaggerGenOptions>()
            .Configure<IApiVersionDescriptionProvider>((options, provider) =>
            {
                foreach (var avd in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(avd.GroupName, new OpenApiInfo
                    {
                        Version = avd.GroupName,
                        Title = $"{AppTitle}"
                    });
                }
            });

        services.AddSwaggerGen(options =>
        {
            options.SupportNonNullableReferenceTypes();

            options.UseInlineDefinitionsForEnums();

            options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

            options.DescribeAllParametersInCamelCase();

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Name = "Bearer",
                Type = SecuritySchemeType.OAuth2,
                Scheme = "oauth2",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Flows = new OpenApiOAuthFlows
                {
                    Password = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri($"{mainSettings.MainUrl}/connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            {"api", "Full API access"},
                        }
                    }
                }
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        new List<string>()
                    }
                });

            options.UseOneOfForPolymorphism();
            options.EnableAnnotations(enableAnnotationsForInheritance: true, enableAnnotationsForPolymorphism: true);

            options.ExampleFilters();
        });

        services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

        services.AddSwaggerGenNewtonsoftSupport();

        return services;
    }


    public static void UseAppSwagger(this WebApplication app)
    {
        var swaggerSettings = app.Services.GetService<SwaggerSettings>();

        if (!swaggerSettings?.Enabled ?? false)
            return;

        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api/{documentname}/api.yaml";
        });

        app.UseSwaggerUI(
            options =>
            {
                options.RoutePrefix = "api";
                provider.ApiVersionDescriptions.ToList().ForEach(
                    description => options.SwaggerEndpoint($"/api/{description.GroupName}/api.yaml", description.GroupName.ToUpperInvariant())
                );

                options.DocExpansion(DocExpansion.List);
                options.DefaultModelsExpandDepth(-1);
                options.OAuthAppName(AppTitle);

            }
        );
    }
}
