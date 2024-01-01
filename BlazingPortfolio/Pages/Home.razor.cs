using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazingPortfolio.Pages
{
	internal record LinkItemInformation(string IconUrl, string Link, string Title);

	public partial class Home
	{
		[Inject]
		public IJSRuntime JsRuntime { get; set; }

		[Inject]
		public IHttpClientFactory HttpClientFactory { get; set; }

		private List<LinkItemInformation> _linkItemInformations;

		private bool _flipped;

		protected override void OnInitialized()
		{
			_linkItemInformations = new List<LinkItemInformation>()
			{
				new("resources/medium.png", "https://stefansch.medium.com", "Blog (Medium)"),
				new("resources/github.png", "https://github.com/Sossenbinder", "Github"),
				new("resources/twitter.png", "https://twitter.com/DotSchranz", "Twitter"),
				new("resources/linkedin.png", "https://www.linkedin.com/in/stefan-schranz-1aa8a6196/", "LinkedIn"),
				new ("resources/telegram.png", "https://telegram.me/Sossenbinder", "Telegram"),
			};
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			var client = HttpClientFactory.CreateClient();

			if (_flipped)
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