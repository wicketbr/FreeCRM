using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plugins;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/GetBlazorCachedPluginBinary/{Hash}")]
    public async Task<ActionResult<byte[]>> GetBlazorCachedPluginBinary(string Hash)
    {
        var output = await da.GetBlazorCachedPluginBinary(Hash);
        return Ok(output);
    }

    [HttpPost]
    [Authorize]
    [Route("~/api/Data/ExecutePlugin")]
    public ActionResult<PluginExecuteRequest> ExecutePlugin(PluginExecuteRequest request)
    {
        var output = da.ExecutePlugin(request, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("~/api/Data/SaveBlazorCachedPluginBinary/{Hash}")]
    public async Task<ActionResult> SaveBlazorCachedPluginBinary(string Hash, byte[] BinaryData)
    {
        // Let this run in the background and return immediately
        var response = Ok();

        HttpContext.Response.OnCompleted(async () => {
            await da.SaveBlazorCachedPluginBinary(Hash, BinaryData);
        });

        return response;
    }
}
