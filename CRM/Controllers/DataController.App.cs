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
}
