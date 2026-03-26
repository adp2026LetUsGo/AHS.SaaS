using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AHS.Web.UI;
using AHS.Web.UI.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5000") });
builder.Services.AddScoped<GatewayClient>();

// Demo Data Layer
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<CsvXinferService>(client =>
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped<IXinferDemoService, CsvXinferService>();

await builder.Build().RunAsync().ConfigureAwait(false);
