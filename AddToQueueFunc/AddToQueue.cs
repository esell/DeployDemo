using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json.Linq;

namespace DeployDemo
{
    public static class Function1
    {
    [FunctionName("AddToQueue")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var STORAGE_ACCT = System.Environment.GetEnvironmentVariable("STORAGE_ACCT", EnvironmentVariableTarget.Process);
            var STORAGE_ACCT_KEY = System.Environment.GetEnvironmentVariable("STORAGE_ACCT_KEY", EnvironmentVariableTarget.Process);
            var EVENT_HUB_CONN = System.Environment.GetEnvironmentVariable("EVENT_HUB_CONN", EnvironmentVariableTarget.Process);
            var EhEntityPath = "deployqueue";

            // Connect to EH
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EVENT_HUB_CONN)
            {
               
                EntityPath = EhEntityPath
            };
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
 
            CloudStorageAccount storageAccount = new CloudStorageAccount(
            new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                STORAGE_ACCT,
                STORAGE_ACCT_KEY), true);

            // Create a blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get a reference to a container named "mycontainer."
            CloudBlobContainer container = blobClient.GetContainerReference("testblah");

            // Get blob
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("azuredeploy.json");

            // Save blog to text var
            var templateContent = await blockBlob.DownloadTextAsync();
            //log.Info(templateContent);
            JObject template = JObject.Parse(templateContent);
            
            JObject templateParams = (JObject)template["parameters"];

            //log.Info(templateParams["adminUsername"]["defaultValue"].ToString());
            templateParams["adminUsername"]["defaultValue"] = "esell";
            templateParams["adminPassword"]["defaultValue"] = "B3stP4ss0nTheN3t!";
            templateParams["dnsLabelPrefix"]["defaultValue"] = "esellautogen";
            
            try
            {
               await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(template.ToString())));
            }
            catch (Exception ex) {
                log.Error(ex.Message);
            }
            return req.CreateResponse(HttpStatusCode.OK, templateContent);
        }
    }
}
