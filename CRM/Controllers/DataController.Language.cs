using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/AddLanguage")]
    public ActionResult<DataObjects.BooleanResponse> AddLanguage(DataObjects.SimplePost post)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (!String.IsNullOrEmpty(post.SingleItem)) {
            output = da.AddLanguage(CurrentUser.TenantId, post.SingleItem);
        } else {
            output.Messages.Add("Missing Culture Code");
        }

        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteLanguage")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteLanguage(DataObjects.SimplePost post)
    {
        var output = await da.DeleteLanguage(CurrentUser.TenantId, post.SingleItem);
        return Ok(output);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/GetDefaultLanguage")]
    public ActionResult<DataObjects.Language> GetDefaultLanguage()
    {
        var output = da.GetDefaultLanguage();
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveLanguage")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> SaveLanguage(DataObjects.Language language)
    {
        var output = await da.SaveLanguage(CurrentUser.TenantId, language, CurrentUser);
        return Ok(output);
    }
}
