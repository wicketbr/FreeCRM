using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetUdfLabels")]
    public async Task<ActionResult<List<DataObjects.udfLabel>>> GetUdfLabels()
    {
        var output = await da.GetUDFLabels(CurrentUser.TenantId, true);
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveUDFLabels")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> SaveUDFLabels(List<DataObjects.udfLabel> labels)
    {
        var output = await da.SaveUDFLabels(CurrentUser.TenantId, labels, CurrentUser);
        return Ok(output);
    }
}
