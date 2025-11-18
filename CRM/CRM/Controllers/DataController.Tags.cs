using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteTag/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteTag(Guid id)
    {
        var output = await da.DeleteTag(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetTag/{id}")]
    public async Task<ActionResult<DataObjects.Tag>> GetTag(Guid id)
    {
        var output = await da.GetTag(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetTags")]
    public async Task<ActionResult<List<DataObjects.Tag>>> GetTags()
    {
        var output = await da.GetTags(CurrentUser.TenantId, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveTag")]
    public async Task<ActionResult<DataObjects.Tag>> SaveTag(DataObjects.Tag tag)
    {
        var output = await da.SaveTag(tag, CurrentUser);
        return Ok(output);
    }
}
