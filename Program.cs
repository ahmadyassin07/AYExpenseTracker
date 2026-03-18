using AYExpenseTracker;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AYExpenseTracker.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<FirebaseAuthService>();
builder.Services.AddScoped<FirebaseService>();
builder.Services.AddSingleton<ToastService>();
builder.Services.AddScoped<OcrService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IntelligenceService>();
builder.Services.AddScoped<StateService>();
builder.Services.AddScoped<PWAService>();
await builder.Build().RunAsync();
