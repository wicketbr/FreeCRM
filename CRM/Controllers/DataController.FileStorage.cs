using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.ManageFiles)]
    [Route("~/api/Data/DeleteFileStorage/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteFileStorage(Guid id)
    {
        var output = await da.DeleteFileStorage(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetFileStorage/{id}")]
    public async Task<ActionResult<DataObjects.FileStorage>> GetFileStorage(Guid id)
    {
        var output = await da.GetFileStorage(id, CurrentUser);
        bool allowAccess = da.UserCanViewFile(output, CurrentUser);

        if (allowAccess) {
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpPost]
    [Authorize(Policy = Policies.ManageFiles)]
    [Route("~/api/Data/GetFileStorageItems")]
    public async Task<ActionResult<DataObjects.FilterFileStorage>> GetFileStorageItems(DataObjects.FilterFileStorage filter)
    {
        var output = await da.GetFileStorageItems(filter, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize(Policy = Policies.ManageFiles)]
    [Route("~/api/Data/GetFileStorageItems")]
    public async Task<ActionResult<List<DataObjects.FileStorage>>> GetFileStorageItems()
    {
        var output = await da.GetFileStorageItems(CurrentUser.TenantId);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetImageFiles")]
    public async Task<ActionResult<List<DataObjects.FileStorage>>> GetImageFiles()
    {
        var output = await da.GetImageFiles(CurrentUser.TenantId);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetUniqueFileExtensions")]
    public async Task<ActionResult<List<string>>> GetUniqueFileExtensions()
    {
        var output = await da.GetUniqueFileExtensions(CurrentUser.TenantId);
        return Ok(output);
    }

    [HttpPost]
    [Authorize]
    [Route("~/api/Data/SaveFile")]
    public async Task<ActionResult<DataObjects.FileStorage>> SaveFile(DataObjects.FileStorage fileStorage)
    {
        var output = await da.SaveFileStorage(fileStorage, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveTenantLogo")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> SaveTenantLogo(DataObjects.FileStorage fileStorage)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        // First, remove any existing logo
        await da.DeleteTenantLogo(fileStorage.TenantId);

        var saved = await da.SaveFileStorage(fileStorage, CurrentUser);
        if (saved.ActionResponse.Result) {
            output.Result = true;
        } else {
            output.Messages = saved.ActionResponse.Messages;
        }

        return Ok(output);
    }

    [HttpPost]
    [Authorize]
    [Route("~/api/Data/SaveUserPhoto")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> SaveUserPhoto(DataObjects.FileStorage fileStorage)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        // First, remove any existing photo
        if (fileStorage.UserId.HasValue) {
            await da.DeleteUserPhoto((Guid)fileStorage.UserId);
        }

        var saved = await da.SaveFileStorage(fileStorage, CurrentUser);
        if (saved.ActionResponse.Result) {
            output.Result = true;

            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = CurrentUser.TenantId,
                ItemId = fileStorage.UserId,
                UpdateType = DataObjects.SignalRUpdateType.User,
                Message = "SavedUserPhoto",
                UserId = CurrentUser != null ? CurrentUser.UserId : null,
            });
        } else {
            output.Messages = saved.ActionResponse.Messages;
        }
        return Ok(output);
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/UndeleteFileStorage/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> UndeleteFileStorage(Guid id)
    {
        var output = await da.UndeleteFileStorage(id);
        return Ok(output);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/File/Embed/{id}")]
    public async Task<IActionResult> EmbedFile(Guid id)
    {
        string filename = String.Empty;
        byte[]? fileContent = null;
        string mimeType = "";

        if (id != Guid.Empty && context != null) {
            DataObjects.FileStorage file = await da.GetFileStorage(id);
            if (file != null && file.ActionResponse != null && file.ActionResponse.Result) {
                string extension = da.StringValue(file.Extension).Replace(".", "").ToLower();
                mimeType = Utilities.GetMimeType(extension);
                filename = da.StringValue(file.FileName);
                fileContent = file.Value;
            }
        }

        if (fileContent == null) {
            return new EmptyResult();
        } else {
            return new FileStreamResult(new MemoryStream(fileContent), mimeType);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/File/View/{id}")]
    public async Task<IActionResult> ViewFile(Guid id)
    {
        string filename = String.Empty;
        byte[]? fileContent = null;
        string mimeType = "";

        if (id != Guid.Empty && context != null) {
            DataObjects.FileStorage file = await da.GetFileStorage(id, CurrentUser);
            if (file != null && file.ActionResponse != null && file.ActionResponse.Result) {
                string extension = da.StringValue(file.Extension).Replace(".", "").ToLower();
                mimeType = Utilities.GetMimeType(extension);
                filename = da.StringValue(file.FileName);
                fileContent = file.Value;
            }
        }

        if (fileContent == null) {
            return new EmptyResult();
        } else {
            return new FileStreamResult(new MemoryStream(fileContent), mimeType);
        }
    }
}
