using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BlazingPortfolio.Backend.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;

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
		private readonly CosmosClient _cosmosClient;

		public VisitCounter(CosmosClient cosmosClient)
		{
			_cosmosClient = cosmosClient;
		}

		[Function("AddVisit")]
		public async Task<IActionResult> AddVisit(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post")]
			HttpRequest req)
		{
			var creationResult = await _cosmosClient.CreateDatabaseIfNotExistsAsync(Constants.Database);
			if (!creationResult.StatusCode.IsSuccess())
			{
				return new StatusCodeResult(500);
			}

			var db = _cosmosClient.GetDatabase(Constants.Database);

			var response = await db.CreateContainerIfNotExistsAsync(Constants.Table, Constants.PartitionKey);

			if (!response.StatusCode.IsSuccess())
			{
				return new StatusCodeResult(500);
			}

			var container = db.GetContainer(Constants.Table);

			var dateTimeStamp = DateProvider.TimeStampToday;
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

			return new JsonResult(new
			{
				timeStamps = JsonConvert.SerializeObject(counterDict),
				total = counterDict.Sum(x => x.Value),
			});
		}
	}
}
