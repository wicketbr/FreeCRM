using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteAllPendingDeletedRecords")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteAllPendingDeletedRecords()
    {
        var output = await da.DeleteAllPendingDeletedRecords(CurrentUser.TenantId);
        return Ok(output);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/DeletePendingDeletedRecords")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeletePendingDeletedRecords()
    {
        var output = await da.DeletePendingDeletedRecords();
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteRecordImmediately/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteRecordImmediately(Guid id, DataObjects.SimplePost post)
    {
        var output = await da.DeleteRecordImmediately(post.SingleItem, id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/GetBlazorDataModel")]
    public ActionResult<DataObjects.BlazorDataModelLoader> GetBlazorDataModel()
    {
        var output = da.GetBlazorDataModel();
        return Ok(output);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/GetBlazorDataModel/{id}")]
    public async Task<ActionResult<DataObjects.BlazorDataModelLoader>> GetBlazorDataModel(Guid id)
    {
        var output = new DataObjects.BlazorDataModelLoader();

        if (CurrentUser.Enabled) {
            output = await da.GetBlazorDataModel(CurrentUser, _fingerprint);

            if (!CurrentUser.Sudo) {
                await da.UpdateUserLastLoginTime(CurrentUser.UserId);
            }
        } else {
            output = da.GetBlazorDataModel();
        }

        return Ok(output);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/GetBlazorDataModelByTenantCode/{TenantCode}")]
    public async Task<ActionResult<DataObjects.BlazorDataModelLoader>> GetBlazorDataModelByTenantCode(string TenantCode)
    {
        var output = new DataObjects.BlazorDataModelLoader();

        if (CurrentUser.Enabled) {
            output = await da.GetBlazorDataModel(CurrentUser, _fingerprint);

            if (!CurrentUser.Sudo) {
                await da.UpdateUserLastLoginTime(CurrentUser.UserId);
            }
        } else {
            output = await da.GetBlazorDataModelByTenantCode(TenantCode);
        }

        return Ok(output);
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/GetDeletedRecordCounts")]
    public async Task<ActionResult<DataObjects.DeletedRecordCounts>> GetDeletedRecordCounts()
    {
        var output = await da.GetDeletedRecordCounts(CurrentUser.TenantId);
        return Ok(output);
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/GetDeletedRecords")]
    public async Task<ActionResult<DataObjects.DeletedRecords>> GetDeletedRecords()
    {
        var output = await da.GetDeletedRecords(CurrentUser.TenantId);
        return Ok(output);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/GetGloballyDisabledModules")]
    public ActionResult<List<string>> GetGloballyDisabledModules()
    {
        var output = new List<string>();

        if (configurationHelper.GloballyDisabledModules != null && configurationHelper.GloballyDisabledModules.Any()) {
            output = configurationHelper.GloballyDisabledModules;
        }

        return Ok(output);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/GetGloballyEnabledModules")]
    public ActionResult<List<string>> GetGloballyEnabledModules()
    {
        var output = new List<string>();

        if (configurationHelper.GloballyEnabledModules != null && configurationHelper.GloballyEnabledModules.Any()) {
            output = configurationHelper.GloballyEnabledModules;
        }

        return Ok(output);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/GetStartupState")]
    public DataObjects.BooleanResponse GetStartupState()
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (GlobalSettings.StartupError) {
            if (!String.IsNullOrWhiteSpace(GlobalSettings.StartupErrorCode)) {
                output.Messages.Add(GlobalSettings.StartupErrorCode);
            } else {
                output.Messages.Add("Unknown");
            }
        } else {
            output.Result = true;
        }

        return output;
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("~/api/Data/GetStartupState")]
    public DataObjects.BooleanResponse GetStartupState(DataObjects.SimplePost post)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (GlobalSettings.StartupError) {
            if (!String.IsNullOrWhiteSpace(GlobalSettings.StartupErrorCode)) {
                output.Messages.Add(GlobalSettings.StartupErrorCode);
            } else {
                output.Messages.Add("Unknown");
            }

            if (GlobalSettings.StartupErrorMessages.Any()) {
                foreach (var msg in GlobalSettings.StartupErrorMessages) {
                    output.Messages.Add(msg);
                }
            }
        } else {
            da.UpdateApplicationURL(post.SingleItem);
            output.Result = true;
        }

        return output;
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/GetVersionInfo")]
    public async Task<ActionResult<DataObjects.VersionInfo>> GetVersionInfo()
    {
        if (CurrentUser.Enabled && !CurrentUser.Sudo) {
            await da.UpdateUserLastLoginTime(CurrentUser.UserId);
        }

        var output = da.VersionInfo;
        return Ok(output);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("~/api/Data/SignalRUpdate")]
    public async Task SignalRUpdate(DataObjects.SignalRUpdate update)
    {
        if (_signalR != null) {
            var processedInApp = await SignalRUpdateApp(update);
            if (!processedInApp) {
                if (update.TenantId.HasValue) {
                    // This is a tenant-specific update. Send only to those people in that tenant group.
                    await _signalR.Clients.Group(update.TenantId.Value.ToString()).SignalRUpdate(update);
                } else {
                    // This is a non-tenant-specific update.
                    await _signalR.Clients.All.SignalRUpdate(update);
                }
            }
        }
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/UndeleteRecord/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> UndeleteRecord(Guid id, DataObjects.SimplePost post)
    {
        var output = await da.UndeleteRecord(post.SingleItem, id, CurrentUser);
        return Ok(output);
    }
}
