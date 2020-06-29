using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using TheChat.Messages;
using Microsoft.Azure.Cosmos.Table;
using TheChat.Functions.Models;

namespace TheChat.Functions
{
    public static class RemoveFromGroup
    {
        [FunctionName("RemoveFromGroup")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRGroupAction> signalRGroupActions,
            [Table("Users", Connection = "StorageConnection")]CloudTable usersTable,
            ILogger log)
        {
            var message = new JsonSerializer().Deserialize<UserConnectedMessage>(new JsonTextReader(new StreamReader(req.Body)));

            //Eliminar Ususriode SignalR
            await signalRGroupActions.AddAsync(new SignalRGroupAction
            {
                ConnectionId = message.Token,
                UserId = message.Sender,
                GroupName = message.GroupName,
                Action = GroupAction.Remove
            });

            //Eliminar el usuario de la tabla
            TableQuery<UserEntity> rangeQuery = new TableQuery<UserEntity>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, message.Sender));

            foreach (var entity in await usersTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                TableOperation deleteOperation = TableOperation.Delete(entity);
                TableResult result = await usersTable.ExecuteAsync(deleteOperation);
            }
        }
    }
}
