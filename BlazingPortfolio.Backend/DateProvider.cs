using System;

namespace BlazingPortfolio.Backend
{
	public static class DateProvider
	{
		public static string TimeStampToday => DateTime.UtcNow.ToString(Format);

		public static string FormatWithDayStamp(DateTime dateTime) => dateTime.ToString(Format);

		public static DateTime ParseDate(string date) => DateTime.ParseExact(date, Format, null);

		private const string Format = "MMddyyyy";
	}
}
