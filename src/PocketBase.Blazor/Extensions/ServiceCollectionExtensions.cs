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
        var options = new PocketBaseOptions();
        configure(options);

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            throw new ArgumentException("BaseUrl must be provided in PocketBaseOptions", nameof(options.BaseUrl));

        services.AddSingleton(options);

        services.AddScoped(sp =>
        {
            var http = new HttpClient
            {
                BaseAddress = new Uri(options.BaseUrl)
            };

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                http.DefaultRequestHeaders.Add("Accept", "application/json");
                http.DefaultRequestHeaders.Add("Authorization", options.ApiKey);
                http.DefaultRequestHeaders.Add("User-Agent", "PocketBase.Blazor");
            }

            return http;
        });

        services.AddScoped<IPocketBase, PocketBase>();

        return services;
    }
}
