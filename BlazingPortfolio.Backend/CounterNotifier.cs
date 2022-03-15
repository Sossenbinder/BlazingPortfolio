using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace BlazingPortfolio.Backend
{
	public class CounterNotifier
	{
		private readonly TelegramBotClient _botClient;

		private readonly CosmosClient _cosmosClient;

		private readonly IConfiguration _configuration;

		public CounterNotifier(
			TelegramBotClient botClient, 
			CosmosClient cosmosClient, 
			IConfiguration configuration)
		{
			_botClient = botClient;
			_cosmosClient = cosmosClient;
			_configuration = configuration;
		}

		[FunctionName("NotifyTelegram")]
		public async Task NotifyTelegram(
#if DEBUG
			[TimerTrigger("* * * * *")]
#elif RELEASE
			[TimerTrigger("0 8 * * *")]
#endif
			TimerInfo timerInfo,
			ILogger log)
		{
			var db = _cosmosClient.GetDatabase(Constants.Database);
			var container = db.GetContainer(Constants.Table);

			var iterator = container.GetItemQueryIterator<DailyCounter>();

			var counterDict = new Dictionary<string, int>();
			while (iterator.HasMoreResults)
			{
				foreach (var item in await iterator.ReadNextAsync())
				{
					counterDict.Add(item.Timestamp, item.Counter);
				}
			}

			var today = DateTime.UtcNow;
			var visitsYesterday = GetCounterForLastDays(counterDict, today, 1);
			var visitsLastSevenDays = GetCounterForLastDays(counterDict, today, 7);
			var visitsThisMonth = counterDict
				.Select(x => (Key: DateProvider.ParseDate(x.Key), x.Value))
				.Where(x => x.Key.Month == today.Month)
				.Sum(x => x.Value);
			var visitsTotal = counterDict.Sum(x => x.Value);
			var message = $"Visits yesterday: {visitsYesterday}\n Visits last 7 days: {visitsLastSevenDays}\n Visits this month: {visitsThisMonth}\n Visits total: {visitsTotal}";

			await _botClient.SendTextMessageAsync(_configuration["Telegram_ChannelId"], message);
		}

		private int GetCounterForLastDays(Dictionary<string, int> counterDict, DateTime startingDate, int days)
		{
			var counter = 0;

			for (var i = 0; i < days; ++i)
			{
				if (counterDict.TryGetValue(DateProvider.FormatWithDayStamp(startingDate.AddDays(-i)), out var value))
				{
					counter += value;
				}
			}

			return counter;
		}
	}
}
