using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Demo.VMStarted
{
    public static class VMStarted
    {
        
        [FunctionName("VMStarted_EventGrid")]
        public static async Task Run([EventGridTrigger]EventGridEvent eventGridEvent,
        [OrchestrationClient] IDurableOrchestrationClient starter,
        ILogger log)
        {
            log.LogInformation(eventGridEvent.Subject);
            var VMId = eventGridEvent.Subject;
            System.DateTime currentTime = System.DateTime.Now;

            VMInfo VMInput;           
            VMInput.VMId = VMId.Replace('/','-'); // Replace / escape character
            VMInput.Hour = currentTime.Hour;
            VMInput.Action = "Set";
      
            string instanceId = await starter.StartNewAsync("VMStarted", VMInput);
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            return;
        }

        [FunctionName("VMStarted")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            VMInfo input = context.GetInput<VMInfo>();
            EntityId id = new EntityId("VM",input.VMId);

            await context.CallEntityAsync(id, input.Action, input.Hour);
            return;
        }
        struct VMInfo
        {
            public string VMId;
            public int Hour;
            public string Action;
        };
        public class VM
        {
         [JsonProperty("hour")]
            public int CurrentValue { get; set; }
            public void Set(int hour)
            {
                if (this.CurrentValue > hour)
                {
                    this.CurrentValue = hour;
                }
            }
            public void Reset(int hour)
            {
                this.CurrentValue = hour;
            }
            [FunctionName(nameof(VM))]
            public static Task Run([EntityTrigger] IDurableEntityContext ctx)
                => ctx.DispatchAsync<VM>();
        }
        
        [FunctionName("VMStartedGet")]
        public static async Task<IActionResult> VMStartedGet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequest req,
            [OrchestrationClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string VMId = req.Query["VMId"];
            string requestVMIdBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic VMIdData = JsonConvert.DeserializeObject(requestVMIdBody);
            VMId = VMId ?? VMIdData?.VMId;

            // Return bad request if VMId is null
            if (VMId == null){
                return new BadRequestObjectResult("Please pass VMId on the query string or in the request body");
            }

            // Return value of VM entity
            EntityId id = new EntityId("VM",VMId.Replace('/','-')); // Replace escape / character
            var response = await starter.ReadEntityStateAsync<VM>(id);
                        return response.EntityExists
                        ? (ActionResult)new OkObjectResult(response)
                        : new BadRequestObjectResult(response);
        }    
    }
}
