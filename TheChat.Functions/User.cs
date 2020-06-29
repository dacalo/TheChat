using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using TheChat.Functions.Models;
using System.Collections.Generic;

namespace TheChat.Functions
{
    public static class User
    {
        [FunctionName("User")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "User/{userId}")] HttpRequest req,
            [Table("Users", Connection = "StorageConnection")]CloudTable usersTable,
            string userId,
            ILogger log)
        {

            TableQuery<UserEntity> rangeQuery = new TableQuery<UserEntity>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, userId));

            var users = new List<UserEntity>();

            foreach (var entity in await usersTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                users.Add(entity);
            }

            return new OkObjectResult(users);
        }
    }
}
