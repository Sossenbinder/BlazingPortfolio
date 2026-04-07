namespace BlazingPortfolio.Pages
{
	public partial class Home
	{
		private int Age => (int)((DateTime.UtcNow - DateTime.Parse("1996-06-06")).TotalDays / 365.2425);

		private int YOE => (int)((DateTime.UtcNow - DateTime.Parse("2017-10-01")).TotalDays / 365.2425);
	}
}
