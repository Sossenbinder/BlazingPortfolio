using Microsoft.AspNetCore.Components;

namespace BlazingPortfolio.Components
{
	public partial class MaterialIcon
	{
		[Parameter]
		public MaterialIconType MaterialIconType { get; set; }

		[Parameter]
		public string IconName { get; set; } = null!;

		private string _materialIconType = default!;

		protected override void OnParametersSet()
		{
			var iconType = MaterialIconType switch
			{
				MaterialIconType.Filled => "",
				MaterialIconType.Outlined => "-outline",
				MaterialIconType.Round => "-round",
				MaterialIconType.Sharp => "-round",
				MaterialIconType.TwoTone => "two-tone",
				_ => throw new ArgumentOutOfRangeException()
			};

			_materialIconType = $"material-icons{iconType}";
		}
	}
}