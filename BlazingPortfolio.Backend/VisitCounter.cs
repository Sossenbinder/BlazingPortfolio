using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using BlazingPortfolio.Backend.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;

namespace BlazingPortfolio.Backend
{
	public class DailyCounter
	{
		[JsonProperty("id")] public string Id { get; set; }

		public string Timestamp { get; set; }

		public int Counter { get; set; }
	}

	public class VisitCounter
	{
		private const string Database = "BlazingPortfolio";

		private const string Table = "Visits";

		private const string PartitionKey = "/Timestamp";

		private readonly CosmosClient _cosmosClient;

		private static string TimeStampToday => DateTime.UtcNow.ToString("MMddyyyy");

		public VisitCounter(
			CosmosClient cosmosClient)
		{
			_cosmosClient = cosmosClient;
		}

		[FunctionName("AddVisit")]
		public async Task<IActionResult> AddVisit(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post")]
			HttpRequest req,
			ILogger _)
		{
			var creationResult = await _cosmosClient.CreateDatabaseIfNotExistsAsync(Database);
			if (!creationResult.StatusCode.IsSuccess())
			{
				return new InternalServerErrorResult();
			}

			var db = _cosmosClient.GetDatabase(Database);

			var response = await db.CreateContainerIfNotExistsAsync(Table, PartitionKey);

			if (!response.StatusCode.IsSuccess())
			{
				return new InternalServerErrorResult();
			}

			var container = db.GetContainer(Table);

			var dateTimeStamp = TimeStampToday;
			var pk = new PartitionKey(dateTimeStamp);

			try
			{
				var existingItem = await container.ReadItemAsync<DailyCounter>(dateTimeStamp, pk);
				var newItem = existingItem.Resource;
				newItem.Counter += 1;
				await container.ReplaceItemAsync(newItem, dateTimeStamp, pk);
			}
			catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
			{
				await container.CreateItemAsync(new DailyCounter()
				{
					Id = dateTimeStamp,
					Counter = 1,
					Timestamp = dateTimeStamp,
				}, pk);
			}

			return new OkResult();
		}

		[FunctionName("GetVisits")]
		public async Task<IActionResult> GetVisits(
			[HttpTrigger(AuthorizationLevel.Function, "get")]
			HttpRequest req,
			ILogger _)
		{
			var db = _cosmosClient.GetDatabase(Database);
			var container = db.GetContainer(Table);

			var iterator = container.GetItemQueryIterator<DailyCounter>();

			var counterDict = new Dictionary<string, int>();
			while (iterator.HasMoreResults)
			{
				foreach (var item in await iterator.ReadNextAsync())
				{
					counterDict.Add(item.Timestamp, item.Counter);
				}
			}

			return new JsonResult(new
			{
				timeStamps = JsonConvert.SerializeObject(counterDict),
				total = counterDict.Sum(x => x.Value),
			});
		}
	}
}
