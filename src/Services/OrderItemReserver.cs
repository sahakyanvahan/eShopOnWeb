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
using Newtonsoft.Json;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using MessageBus;

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
                var messageReceiver = new MessageReceiver();
                await messageReceiver.ReceiveMessageAsync();

                string connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                string containerName = Environment.GetEnvironmentVariable("ContainerName");

                using (var stream = new MemoryStream())
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    var order = JsonConvert.DeserializeObject<Order>(requestBody);

                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(requestBody);
                    writer.Flush();
                    stream.Position = 0;

                    var blobClient = new BlobContainerClient(connection, containerName);
                    var blob = blobClient.GetBlobClient($"Order-{order.Id}");
                    await blob.UploadAsync(stream);
                    await SaveToCosmosDb(requestBody, log);
                    return new OkObjectResult("File uploaded successfully");
                }
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
                throw;
            }

        }

        private static async Task SaveToCosmosDb(string requestBody, ILogger log)
        {
            try
            {
                var cosmosUri = Environment.GetEnvironmentVariable("CosmosDbUri");
                var connection = Environment.GetEnvironmentVariable("CosmosDbConnection");
                using CosmosClient client = new(connectionString: connection!);
                var database = client.GetDatabase("orders");
                var container = client.GetContainer("orders", "processed-orders");

                var order = JsonConvert.DeserializeObject<Order>(requestBody);
                var response = await container.CreateItemAsync(order);
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
                throw;
            }
            
        }
    }
}
