using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteService/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteService(Guid id)
    {
        var output = await da.DeleteService(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetService/{id}")]
    public async Task<ActionResult<DataObjects.Service>> GetService(Guid id)
    {
        var output = await da.GetService(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetServices")]
    public async Task<ActionResult<List<DataObjects.Service>>> GetServices()
    {
        var output = await da.GetServices(CurrentUser.TenantId, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveService")]
    public async Task<ActionResult<DataObjects.Service>> SaveService(DataObjects.Service service)
    {
        var output = await da.SaveService(service, CurrentUser);
        return Ok(output);
    }
}
