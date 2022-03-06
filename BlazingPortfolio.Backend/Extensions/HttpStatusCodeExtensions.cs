using System.Net;

namespace BlazingPortfolio.Backend.Extensions
{
	public static class HttpStatusCodeExtensions
	{
		public static bool IsSuccess(this HttpStatusCode statusCode)
		{
			return (int)statusCode / 100 == 2;
		}
	}
}