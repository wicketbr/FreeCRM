using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeletePayment/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeletePayment(Guid id)
    {
        var output = await da.DeletePayment(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetPayment/{id}")]
    public async Task<ActionResult<DataObjects.Payment>> GetPayment(Guid id)
    {
        var output = await da.GetPayment(id, CurrentUser);

        if (CurrentUser.Admin || CurrentUser.UserId == output.UserId) {
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/GetPayments")]
    public async Task<ActionResult<List<DataObjects.Payment>>> GetPayments()
    {
        var output = await da.GetPayments(CurrentUser.TenantId, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetPaymentsForUser")]
    public async Task<ActionResult<List<DataObjects.Payment>>> GetPaymentsForUser()
    {
        var output = await da.GetPaymentsForUser(CurrentUser.UserId, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SavePayment")]
    public async Task<ActionResult<DataObjects.Payment>> SavePayment(DataObjects.Payment payment)
    {
        var output = await da.SavePayment(payment, CurrentUser);
        return Ok(output);
    }
}