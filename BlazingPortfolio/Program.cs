using BlazingPortfolio;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddHttpClient();
builder.RootComponents.Add<App>("#app");

await builder.Build().RunAsync();