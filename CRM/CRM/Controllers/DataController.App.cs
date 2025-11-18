using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

// Use this file as a place to put any application-specific API endpoints.

public partial class DataController
{
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/YourEndpoint/")]
    public ActionResult<DataObjects.BooleanResponse> YourEndpoint()
    {
        var output = new DataObjects.BooleanResponse {
            Result = true,
            Messages = new List<string> { "Your messages here" },
        };

        return Ok(output);
    }

    private async Task<bool> SignalRUpdateApp(DataObjects.SignalRUpdate update)
    {
        await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

        bool processedInApp = false;

        // Do any app-specific SignalR processing here.
        // If your app handles the sending of the message to the clients, set processedInApp to true.

        return processedInApp;
    }
}
