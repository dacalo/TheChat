using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using TheChat.Functions.Helpers;

namespace TheChat.Functions
{
    public static class ClearPhotos
    {
        [FunctionName("ClearPhotos")]
        public static async void Run([TimerTrigger("0 */60 * * * *")]TimerInfo myTimer, ILogger log)
        {
            await StorageHelper.Clear();
        }
    }
}
