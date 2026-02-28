using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AHS.Web.BentoUI;
using AHS.Web.BentoUI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ShipmentService>();

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.TypeInfoResolver = AppJsonSerializerContext.Default;
});

await builder.Build().RunAsync();
