using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteEmailTemplate/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteEmailTemplate(Guid id)
    {
        var output = await da.DeleteEmailTemplate(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetEmailTemplate/{id}")]
    public async Task<ActionResult<DataObjects.EmailTemplate>> GetEmailTemplate(Guid id)
    {
        var output = await da.GetEmailTemplate(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetEmailTemplates")]
    public async Task<ActionResult<List<DataObjects.EmailTemplate>>> GetEmailTemplates()
    {
        var output = await da.GetEmailTemplates(CurrentUser.TenantId, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveEmailTemplate")]
    public async Task<ActionResult<DataObjects.EmailTemplate>> SaveEmailTemplate(DataObjects.EmailTemplate emailTemplate)
    {
        var output = await da.SaveEmailTemplate(emailTemplate, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SendTemplateEmail")]
    public ActionResult<DataObjects.BooleanResponse> SendTemplateEmail(DataObjects.EmailTemplate template)
    {
        var output = da.SendTemplateEmail(template, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SendTemplateEmailTest")]
    public ActionResult<DataObjects.BooleanResponse> SendTemplateEmailTest(DataObjects.EmailTemplate template)
    {
        var output = da.SendTemplateEmailTest(template, CurrentUser);
        return Ok(output);
    }
}
