using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteUserGroup/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteUserGroup(Guid id)
    {
        var output = await da.DeleteUserGroup(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/GetUserGroup/{id}")]
    public async Task<ActionResult<DataObjects.UserGroup>> GetUserGroup(Guid id)
    {
        var output = await da.GetUserGroup(id, true, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetUserGroups")]
    public async Task<ActionResult<List<DataObjects.UserGroup>>> GetUserGroups()
    {
        var output = await da.GetUserGroups(CurrentUser.TenantId, true, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveUserGroup")]
    public async Task<ActionResult<DataObjects.UserGroup>> SaveUserGroup(DataObjects.UserGroup group)
    {
        var output = await da.SaveUserGroup(group, CurrentUser);
        return Ok(output);
    }
}
