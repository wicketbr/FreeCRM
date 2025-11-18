using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteInvoice/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteInvoice(Guid id)
    {
        var output = await da.DeleteInvoice(id, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize]
    [Route("~/api/Data/GenerateInvoiceImages")]
    public async Task<ActionResult<DataObjects.Invoice>> GenerateInvoiceImages(DataObjects.Invoice invoice)
    {
        if (CurrentUser.Admin || CurrentUser.UserId == invoice.UserId) {
            var output = await da.GenerateInvoiceImages(invoice, CurrentUser);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpPost]
    [Authorize]
    [Route("~/api/Data/GenerateInvoicePDF")]
    public async Task<ActionResult<DataObjects.Invoice>> GenerateInvoicePDF(DataObjects.Invoice invoice)
    {
        if (CurrentUser.Admin || CurrentUser.UserId == invoice.UserId) {
            var output = await da.GenerateInvoicePDF(invoice, CurrentUser);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetInvoice/{id}")]
    public async Task<ActionResult<DataObjects.Invoice>> GetInvoice(Guid id)
    {
        var output = await da.GetInvoice(id, CurrentUser);

        if (CurrentUser.Admin || CurrentUser.UserId == output.UserId) {
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetInvoiceRendered/{id}")]
    public async Task<ActionResult<DataObjects.Invoice>> GetInvoiceRendered(Guid id)
    {
        var output = await da.GetInvoice(id, CurrentUser, true, true);

        if (CurrentUser.Admin || CurrentUser.UserId == output.UserId) {
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/GetInvoices")]
    public async Task<ActionResult<List<DataObjects.Invoice>>> GetInvoices()
    {
        var output = await da.GetInvoices(CurrentUser.TenantId, false, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize]
    [Route("~/api/Data/GetInvoicesFiltered")]
    public ActionResult<DataObjects.FilterInvoices> GetInvoicesFiltered(DataObjects.FilterInvoices filter)
    {
        var output = da.GetInvoicesFiltered(filter, CurrentUser);
        return Ok(output);
    }

    // {{ModuleItemStart:Appointments}}
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/GetInvoicesForAppointment/{id}")]
    public async Task<ActionResult<List<DataObjects.Invoice>>> GetInvoicesForAppointment(Guid id)
    {
        var output = await da.GetInvoicesForAppointment(id, false, CurrentUser);
        return Ok(output);
    }
    // {{ModuleItemEnd:Appointments}}

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetInvoicesForUser/{id}")]
    public async Task<ActionResult<List<DataObjects.Invoice>>> GetInvoicesForUser(Guid id)
    {
        if (CurrentUser.Admin || CurrentUser.UserId == id) {
            var output = await da.GetInvoicesForUser(id, false, CurrentUser);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveInvoice")]
    public async Task<ActionResult<DataObjects.Invoice>> SaveInvoice(DataObjects.Invoice invoice)
    {
        var output = await da.SaveInvoice(invoice, CurrentUser);
        return Ok(output);
    }
}