using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteLocation/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteLocation(Guid id)
    {
        var output = await da.DeleteLocation(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetLocation/{id}")]
    public async Task<ActionResult<DataObjects.Location>> GetLocation(Guid id)
    {
        var output = await da.GetLocation(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetLocations")]
    public async Task<ActionResult<List<DataObjects.Location>>> GetLocations()
    {
        var output = await da.GetLocations(CurrentUser.TenantId, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveLocation")]
    public async Task<ActionResult<DataObjects.Location>> SaveLocation(DataObjects.Location location)
    {
        var output = await da.SaveLocation(location, CurrentUser);
        return Ok(output);
    }
}
