using CRM.Client;
using Blazored.LocalStorage;
using MudBlazor.Services;
using Radzen;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace CRM.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddSingleton<BlazorDataModel>();

            builder.Services.AddBlazorBootstrap();
            builder.Services.AddMudServices();
            builder.Services.AddRadzenComponents();

            await builder.Build().RunAsync();
        }
    }
}