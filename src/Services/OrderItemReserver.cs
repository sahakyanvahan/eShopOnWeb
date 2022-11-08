using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System;
using Microsoft.Azure.Cosmos;

namespace Services
{
    public static class OrderItemReserver
    {
        [FunctionName("OrderItemReserver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                string containerName = Environment.GetEnvironmentVariable("ContainerName");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var path = @"C:\Service\order.json";
                File.WriteAllText(path, requestBody);
                Stream myBlob = File.OpenRead(path);

                //myBlob = file.OpenReadStream();
                var blobClient = new BlobContainerClient(Connection, containerName);
                var blob = blobClient.GetBlobClient(@"order.json");
                //await blob.UploadAsync(myBlob);
                SaveToCosmos();
                return new OkObjectResult("file uploaded successfylly");
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
                throw;
            }

        }

        private static void SaveToCosmos()
        {
            var client = InitializeCosmosDbClient();
            var database = client.GetDatabase("EshopOrders");
            var container = client.GetContainer("EshopOrders", "reserved-orders");
            var order = new Order()
            {
                Id = 2,
                Name = "test order",
            };

            var response = container.CreateItemAsync<Order>(order);
        }

        private static CosmosClient InitializeCosmosDbClient()
        {
            var cosmosUri = Environment.GetEnvironmentVariable("CosmosDbUri");
            var key = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var connection = Environment.GetEnvironmentVariable("CosmosDbConnection");

            //using CosmosClient client = new(accountEndpoint: cosmosUri!, authKeyOrResourceToken: key!);
            using CosmosClient client = new(connectionString: connection!);
            return client;
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
    }
}
