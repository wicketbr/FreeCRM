using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Radzen;

namespace CRM.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddSingleton<BlazorDataModel>();
            builder.Services.AddBlazorBootstrap();
            builder.Services.AddMudServices();
            builder.Services.AddRadzenComponents();
            builder.Services.AddScoped<Radzen.DialogService>();
            builder.Services.AddScoped<Radzen.NotificationService>();
            builder.Services.AddScoped<Radzen.ThemeService>();

            await builder.Build().RunAsync();
        }
    }
}
