using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.AppAdmin)]
    [Route("~/api/Data/DeleteTenant/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteTenant(Guid id)
    {
        var output = await da.DeleteTenant(id);
        return Ok(output);
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteTenantLogo")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteTenantLogo()
    {
        var output = await da.DeleteTenantLogo(CurrentUser.TenantId);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetTenant/{id}")]
    public ActionResult<DataObjects.Tenant> GetTenant(Guid id)
    {
        if (CurrentUser.AppAdmin || (CurrentUser.Admin && CurrentUser.TenantId == id)) {
            var output = da.GetTenant(id, CurrentUser);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/GetTenantList")]
    public async Task<ActionResult<List<DataObjects.TenantList>>> GetTenantList()
    {
        var output = await da.GetTenantList();
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetTenantLogo")]
    public async Task<ActionResult<DataObjects.SimpleResponse>> GetTenantLogo()
    {
        var output = await da.GetTenantLogoId(CurrentUser.TenantId);
        return Ok(output);
    }

    [HttpGet]
    [Authorize(Policy = Policies.AppAdmin)]
    [Route("~/api/Data/GetTenants")]
    public async Task<ActionResult<List<DataObjects.Tenant>>> GetTenants()
    {
        var output = await da.GetTenants();
        return Ok(output);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/GetTenantsForLogin")]
    public async Task<ActionResult<DataObjects.LoginTenantListing>> GetTenantsForLogin()
    {
        var output = await da.GetTenantsForLogin();
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetTenantSettings/{id}")]
    public ActionResult<DataObjects.TenantSettings> GetTenantSettings(Guid id)
    {
        if (CurrentUser.AppAdmin || (CurrentUser.Admin && CurrentUser.TenantId == id)) {
            var output = da.GetTenantSettings(id);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/ReloadTenantUsers")]
    public ActionResult<List<DataObjects.UserListing>> ReloadTenantUsers()
    {
        var output = da.GetTenantUsers(CurrentUser.TenantId, 0, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize]
    [Route("~/api/Data/SaveTenant")]
    public async Task<ActionResult<DataObjects.Tenant>> SaveTenant(DataObjects.Tenant tenant)
    {
        if (CurrentUser.AppAdmin || (CurrentUser.Admin && CurrentUser.TenantId == tenant.TenantId)) {
            var output = await da.SaveTenant(tenant, CurrentUser);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }
}
