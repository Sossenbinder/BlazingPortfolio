using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

namespace BlazingPortfolio.Pages
{

	public partial class Home
	{
		[Inject]
		public IJSRuntime JsRuntime { get; set; }

		[Inject]
		public IHttpClientFactory HttpClientFactory { get; set; }
		
		[Inject]
		private IWebAssemblyHostEnvironment HostEnvironment { get; set; }

		private bool _flipped;

		private int Age => (int)((DateTime.UtcNow - DateTime.Parse("1996-06-06")).TotalDays / 365.2425);
		
		private int YOE => (int)((DateTime.UtcNow - DateTime.Parse("2017-10-01")).TotalDays / 365.2425);

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			var client = HttpClientFactory.CreateClient();

			if (_flipped || HostEnvironment.IsDevelopment())
			{
				return;
			}

			try
			{
				await client.PostAsync("https://blazingportfolio.azurewebsites.net/api/AddVisit", null);
			}
			catch (Exception)
			{
				// Just swallow this exception. It's not horrible if a visit is not registered
			}
		}
	}
}