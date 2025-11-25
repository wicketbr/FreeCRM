using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.AppAdmin)]
    [Route("~/api/Data/GetApplicationSettings")]
    public ActionResult<DataObjects.ApplicationSettings> GetApplicationSettings()
    {
        var output = da.GetApplicationSettings();
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveApplicationSettings")]
    public async Task<ActionResult<DataObjects.ApplicationSettings>> SaveApplicationSettings(DataObjects.ApplicationSettings settings)
    {
        var output = await da.SaveApplicationSettings(settings, CurrentUser);
        return Ok(output);
    }
}
