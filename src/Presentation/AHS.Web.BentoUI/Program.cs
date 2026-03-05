using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AHS.Web.BentoUI;
using AHS.Web.BentoUI.Services;
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5000") });
builder.Services.AddScoped<GatewayClient>();
await builder.Build().RunAsync().ConfigureAwait(false);
