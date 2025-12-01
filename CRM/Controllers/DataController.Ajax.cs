using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpPost]
    [Authorize]
    [Route("~/api/Data/AjaxUserSearch/")]
    public async Task<ActionResult<DataObjects.AjaxLookup>> AjaxUserSearch(DataObjects.AjaxLookup Lookup)
    {
        var output = await da.AjaxUserSearch(Lookup);
        return Ok(output);
    }
}
