using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteUser/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteUser(Guid id)
    {
        var output = await da.DeleteUser(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/DeleteUserPhoto/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteUserPhoto(Guid id)
    {
        if (CurrentUser.Admin || CurrentUser.UserId == id) {
            var output = await da.DeleteUserPhoto(id);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("~/api/Data/ForgotPassword")]
    public async Task<ActionResult<DataObjects.User>> ForgotPassword(DataObjects.User user)
    {
        var output = await da.ForgotPassword(user);
        return Ok(output);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("~/api/Data/ForgotPasswordConfirm")]
    public async Task<ActionResult<DataObjects.User>> ForgotPasswordConfirm(DataObjects.User user)
    {
        var output = await da.ForgotPasswordConfirm(user);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetActiveUser/{id}")]
    public async Task<ActionResult<DataObjects.ActiveUser>> GetActiveUser(Guid id)
    {
        var output = await da.GetActiveUser(id);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetActiveUsers")]
    public async Task<ActionResult<List<DataObjects.ActiveUser>>> GetActiveUsers()
    {
        var output = await da.GetActiveUsers(CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetUser/{id}")]
    public async Task<ActionResult<DataObjects.User>> GetUser(Guid id)
    {
        if (CurrentUser.Admin || CurrentUser.UserId == id) {
            var output = await da.GetUser(id, false, CurrentUser);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetUserDisplayName/{id}")]
    public async Task<ActionResult<DataObjects.SimpleResponse>> GetUserDisplayName(Guid id)
    {
        var displayName = await da.GetUserDisplayName(id);
        var output = new DataObjects.SimpleResponse {
            Result = true,
            Message = displayName,
        };
        return Ok(output);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("~/api/Data/GetUserFromToken/")]
    public async Task<ActionResult<DataObjects.User>> GetUserFromToken(DataObjects.SimplePost post)
    {
        var output = await da.GetUserFromToken(post.SingleItem, _fingerprint);

        if (output.ActionResponse.Result && output.Enabled && context != null) {
            if (String.IsNullOrWhiteSpace(output.AuthToken)) {
                output.AuthToken = post.SingleItem;
            }

            await CustomAuthorization.AddAuthetication(output, context, _fingerprint, String.Empty + output.LastLoginSource);
        }

        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetUserPhotoId/{id}")]
    public async Task<ActionResult<DataObjects.SimpleResponse>> GetUserPhotoId(Guid id)
    {
        if (CurrentUser.Admin || CurrentUser.UserId == id) {
            var output = await da.GetUserPhotoId(id);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/GetUsers")]
    public async Task<ActionResult<DataObjects.FilterUsers>> GetUsers(DataObjects.FilterUsers filter)
    {
        var output = await da.GetUsersFiltered(filter, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/ReloadUser/{id}")]
    public async Task<ActionResult<DataObjects.User>> ReloadUser(Guid id)
    {
        if (CurrentUser.Admin || CurrentUser.UserId == id) {
            var output = await da.GetUser(id, false, CurrentUser);
            if (output.ActionResponse.Result) {
                output.AuthToken = da.GetUserToken(output.TenantId, output.UserId, _fingerprint, CurrentUser.Sudo);
            }
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpPost]
    [Authorize]
    [Route("~/api/Data/ResetUserPassword")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> ResetUserPassword(DataObjects.UserPasswordReset reset)
    {
        if (CurrentUser.Admin || (CurrentUser.UserId == reset.UserId && !CurrentUser.PreventPasswordChange)) {
            var output = await da.ResetUserPassword(reset, CurrentUser);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpPost]
    [Authorize]
    [Route("~/api/Data/SaveUser/")]
    public async Task<ActionResult<DataObjects.User>> SaveUser(DataObjects.User user)
    {
        DataObjects.User output = new DataObjects.User();

        if (await da.UserCanEditUser(CurrentUser.UserId, user.UserId)) {
            output = await da.SaveUser(user, CurrentUser);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpPost]
    [Authorize]
    [Route("~/api/Data/SaveUserPreferences/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> SaveUserPreferences(Guid id, DataObjects.UserPreferences userPreferences)
    {
        if (CurrentUser.Admin || CurrentUser.UserId == id) {
            var output = await da.SaveUserPreferences(id, userPreferences);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/UnlockUserAccount/{id}")]
    public async Task<ActionResult<DataObjects.User>> UnlockUserAccount(Guid id)
    {
        var output = await da.UnlockUserAccount(id);
        return Ok(output);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/Data/UserSignout")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> UserSignout()
    {
        if (context != null) {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        var output = new DataObjects.BooleanResponse { Result = true };
        return Ok(output);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("~/api/Data/UserSignup")]
    public async Task<ActionResult<DataObjects.User>> UserSignup(DataObjects.User user)
    {
        var output = await da.UserSignup(user);
        return Ok(output);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("~/api/Data/UserSignupConfirm")]
    public async Task<ActionResult<DataObjects.User>> UserSignupConfirm(DataObjects.User user)
    {
        var output = await da.UserSignupConfirm(user);
        return Ok(output);
    }
}
