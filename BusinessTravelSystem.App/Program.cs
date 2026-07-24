using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BusinessTravelSystem.App;
using BusinessTravelSystem.App.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IMockDatabaseService, MockDatabaseService>();
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddScoped<IMockAuthenticationService>(sp => sp.GetRequiredService<AuthSessionService>());

var host = builder.Build();
await host.Services.GetRequiredService<IMockDatabaseService>().InitializeAsync();
await host.Services.GetRequiredService<AuthSessionService>().InitializeAsync();
await host.RunAsync();
