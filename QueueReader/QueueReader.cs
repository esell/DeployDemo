using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;

namespace QueueReader
{
    public static class QueueReader
    {
        [FunctionName("QueueReader")]
        public static void Run([EventHubTrigger("deployqueue", Connection = "EVENT_HUB_CONN")]string myEventHubMessage, TraceWriter log)
        {
            log.Info($"C# Event Hub trigger function processed a message: {myEventHubMessage}");
        }
    }
}
