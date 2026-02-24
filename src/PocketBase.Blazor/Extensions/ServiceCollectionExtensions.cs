using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPocketBase(this IServiceCollection services, Action<PocketBaseOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        PocketBaseOptions options = new PocketBaseOptions();
        configure(options);

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            throw new ArgumentException("BaseUrl must be provided in PocketBaseOptions", nameof(options.BaseUrl));

        services.AddSingleton(options);

        services.AddScoped(sp =>
        {
            HttpClient http = new HttpClient
            {
                BaseAddress = new Uri(options.BaseUrl)
            };

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                http.DefaultRequestHeaders.Add("Accept", "application/json");
                http.DefaultRequestHeaders.Add("Authorization", options.ApiKey);
                try
                {
                    http.DefaultRequestHeaders.Add("User-Agent", "PocketBase.Blazor");
                }
                catch (Exception) when (OperatingSystem.IsBrowser())
                {
                    // Forbidden header in browser/WASM.
                }
            }

            return http;
        });

        services.AddScoped<IPocketBase, PocketBase>();

        return services;
    }
}
