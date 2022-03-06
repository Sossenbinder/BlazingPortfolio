using System;
using BlazingPortfolio.Backend;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]

namespace BlazingPortfolio.Backend
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			builder.Services.AddSingleton(_ => new CosmosClient(builder.GetContext().Configuration["CosmosConnectionString"]));
		}
	}
}