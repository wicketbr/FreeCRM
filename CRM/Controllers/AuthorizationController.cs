using CRM;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Plugins;
using System.Security.Claims;

namespace CRM.Server.Controllers;

public class AuthorizationController : ControllerBase
{
    private HttpContext? context;
    private IDataAccess da;
    private IPlugins plugins;
    private string _baseUrl = "";
    private string _requestedUrl = "";
    private string _fingerprint = "";

    public AuthorizationController(IDataAccess daInjection, IHttpContextAccessor httpContextAccessor, IPlugins daPlugins)
    {
        da = daInjection;
        plugins = daPlugins;

        if (httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
            context = httpContextAccessor.HttpContext;
        }

        da.SetHttpContext(context);

        _fingerprint = da.Request("Fingerprint");

        _baseUrl = da.ApplicationURL;

        _requestedUrl = da.CookieRead("requested-url");
    }

    [HttpPost]
    [Route("~/Authorization/Custom")]
    public IActionResult CustomLogin()
    {
        string tenantId = da.Request("TenantId");
        string ssoToken = da.Request("sso-token");

        return Redirect(_baseUrl + "Authorization/Custom?TenantId=" + tenantId + "&sso-token=" + ssoToken + "&Fingerprint=" + _fingerprint);
    }

    [HttpPost]
    [Route("~/Authorization/Plugin")]
    public IActionResult PluginLogin()
    {
        string tenantId = da.Request("TenantId");
        string ssoToken = da.Request("sso-token");
        string pluginName = da.Request("Name");

        return Redirect(_baseUrl + "Authorization/Plugin?Name=" + pluginName + "&TenantId=" + tenantId + "&sso-token=" + ssoToken + "&Fingerprint=" + _fingerprint);
    }

    private void CookieWrite(string cookieName, string value)
    {
        if (context != null) {
            DateTime now = DateTime.Now;
            if (String.IsNullOrEmpty(cookieName)) { return; }

            Microsoft.AspNetCore.Http.CookieOptions option = new Microsoft.AspNetCore.Http.CookieOptions();
            option.Expires = now.AddYears(1);

            context.Response.Cookies.Append(da.CookiePrefix + cookieName, value, option);
        }
    }

    private string QueryStringValue(string valueName)
    {
        string output = "";

        if (context != null) {
            try {
                output += context.Request.Query[valueName].ToString();
            } catch { }
        }

        return output;
    }

    private string RequestValue(string parameter)
    {
        string output = "";

        if (context != null) {
            output = QueryStringValue(parameter);

            if (String.IsNullOrWhiteSpace(output)) {
                output += context.Request.Form[parameter].ToString();
            }
        }

        return output;
    }

    [Authorize(AuthenticationSchemes = "Apple")]
    [Route("~/Authorization/Apple/{id}")]
    public IActionResult Apple(Guid id)
    {
        return Redirect(da.ApplicationURL + "Authorization/AppleAuthorized/" + id.ToString() + "?Fingerprint=" + _fingerprint);
        //return RedirectToAction("AppleAuthorized", new { id = id.ToString() });
    }

    [Route("~/Authorization/AppleAuthorized/{id}")]
    public async Task<IActionResult> AppleAuthorized(Guid id)
    {
        var result = await ProcessClaims("Apple", id);
        if (result.Result) {
            if (!String.IsNullOrWhiteSpace(_requestedUrl)) {
                da.CookieWrite("requested-url", "");
                return Redirect(_requestedUrl);
            } else {
                return Redirect(_baseUrl);
            }
        } else {
            if (!String.IsNullOrWhiteSpace(result.Message)) {
                return Redirect(_baseUrl + "Authorization/" + result.Message);
            } else {
                return Redirect(_baseUrl + "Authorization/InvalidUser?AuthMethod=Apple");
            }
        }
    }

    [Authorize(AuthenticationSchemes = "Facebook")]
    [Route("~/Authorization/Facebook/{id}")]
    public IActionResult Facebook(Guid id)
    {
        return Redirect(da.ApplicationURL + "Authorization/FacebookAuthorized/" + id.ToString() + "?Fingerprint=" + _fingerprint);
        //return RedirectToAction("FacebookAuthorized", new { id = id.ToString() });
    }

    [Route("~/Authorization/FacebookAuthorized/{id}")]
    public async Task<IActionResult> FacebookAuthorized(Guid id)
    {
        var result = await ProcessClaims("Facebook", id);
        if (result.Result) {
            if (!String.IsNullOrWhiteSpace(_requestedUrl)) {
                da.CookieWrite("requested-url", "");
                return Redirect(_requestedUrl);
            } else {
                return Redirect(_baseUrl);
            }
        } else {
            if (!String.IsNullOrWhiteSpace(result.Message)) {
                return Redirect(_baseUrl + "Authorization/" + result.Message);
            } else {
                return Redirect(_baseUrl + "Authorization/InvalidUser?AuthMethod=Facebook");
            }
        }
    }

    [Authorize(AuthenticationSchemes = "Google")]
    [Route("~/Authorization/Google/{id}")]
    public IActionResult Google(Guid id)
    {
        return Redirect(da.ApplicationURL + "Authorization/GoogleAuthorized/" + id.ToString() + "?Fingerprint=" + _fingerprint);
        //return RedirectToAction("GoogleAuthorized", new { id = id.ToString() });
    }

    [Route("~/Authorization/GoogleAuthorized/{id}")]
    public async Task<IActionResult> GoogleAuthorized(Guid id)
    {
        var result = await ProcessClaims("Google", id);
        if (result.Result) {
            if (!String.IsNullOrWhiteSpace(_requestedUrl)) {
                da.CookieWrite("requested-url", "");
                return Redirect(_requestedUrl);
            } else {
                return Redirect(_baseUrl);
            }
        } else {
            if (!String.IsNullOrWhiteSpace(result.Message)) {
                return Redirect(_baseUrl + "Authorization/" + result.Message);
            } else {
                return Redirect(_baseUrl + "Authorization/InvalidUser?AuthMethod=Google");
            }
        }
    }

    [Authorize(AuthenticationSchemes = "MicrosoftAccount")]
    [Route("~/Authorization/MicrosoftAccount/{id}")]
    public IActionResult MicrosoftAccount(Guid id)
    {
        return Redirect(da.ApplicationURL + "Authorization/MicrosoftAccountAuthorized/" + id.ToString() + "?Fingerprint=" + _fingerprint);
        //return RedirectToAction("MicrosoftAccountAuthorized", new { id = id.ToString() });
    }

    [Route("~/Authorization/MicrosoftAccountAuthorized/{id}")]
    public async Task<IActionResult> MicrosoftAccountAuthorized(Guid id)
    {
        var result = await ProcessClaims("MicrosoftAccount", id);
        if (result.Result) {
            if (!String.IsNullOrWhiteSpace(_requestedUrl)) {
                da.CookieWrite("requested-url", "");
                return Redirect(_requestedUrl);
            } else {
                return Redirect(_baseUrl);
            }
        } else {
            if (!String.IsNullOrWhiteSpace(result.Message)) {
                return Redirect(_baseUrl + "Authorization/" + result.Message);
            } else {
                return Redirect(_baseUrl + "Authorization/InvalidUser?AuthMethod=MicrosoftAccount");
            }
        }
    }

    [Authorize(AuthenticationSchemes = "OpenId")]
    [Route("~/Authorization/OpenId/{id}")]
    public IActionResult OpenId(Guid id)
    {
        return Redirect(da.ApplicationURL + "Authorization/OpenIdAuthorized/" + id.ToString() + "?Fingerprint=" + _fingerprint);
        //return RedirectToAction("OpenIdAuthorized", new { id = id.ToString() });
    }

    [Route("~/Authorization/OpenIdAuthorized/{id}")]
    public async Task<IActionResult> OpenIdAuthorized(Guid id)
    {
        var result = await ProcessClaims("OpenId", id);
        if (result.Result) {
            if (!String.IsNullOrWhiteSpace(_requestedUrl)) {
                da.CookieWrite("requested-url", "");
                return Redirect(_requestedUrl);
            } else {
                return Redirect(_baseUrl);
            }
        } else {
            if (!String.IsNullOrWhiteSpace(result.Message)) {
                return Redirect(_baseUrl + "Authorization/" + result.Message);
            } else {
                return Redirect(_baseUrl + "Authorization/InvalidUser?AuthMethod=OpenId");
            }
        }
    }

    private async Task<DataObjects.SimpleResponse> ProcessClaims(string Source, Guid TenantId)
    {
        DataObjects.SimpleResponse output = new DataObjects.SimpleResponse();

        bool validUser = false;
        bool noLocalAccount = false;

        DateTime now = DateTime.UtcNow;

        if (context != null) {
            if (context.User != null) {
                if (context.User.Identity != null) {
                    if (context.User.Identity.IsAuthenticated) {
                        validUser = true;

                        var claims = (System.Security.Claims.ClaimsIdentity)context.User.Identity;

                        if (claims != null && claims.Claims != null && claims.Claims.Any()) {
                            //Dictionary<string, string> allClaims = new Dictionary<string, string>();

                            string name = String.Empty;
                            string preferredUsername = String.Empty;
                            string givenName = String.Empty;
                            string familyName = String.Empty;

                            foreach (var claim in claims.Claims) {
                                var claimType = GetClaimType(claim.Type).ToLower();

                                //allClaims.Add(claim.Type, claim.Value);

                                switch (claimType) {
                                    case "name":
                                        name += claim.Value;
                                        break;

                                    case "emailaddress":
                                    case "preferred_username":
                                        preferredUsername += claim.Value;
                                        break;

                                    case "givenname":
                                    case "given_name":
                                        givenName += claim.Value;
                                        break;

                                    case "surname":
                                    case "family_name":
                                        familyName += claim.Value;
                                        break;
                                }
                            }

                            if (!String.IsNullOrWhiteSpace(preferredUsername)) {
                                noLocalAccount = true;

                                DataObjects.User user = new DataObjects.User();

                                var tenant = da.GetTenant(TenantId);

                                user = await da.GetUserByUsernameOrEmail(TenantId, preferredUsername);
                                if (user == null || !user.ActionResponse.Result) {
                                    // See if this tenant allows for creating new accounts automatically.
                                    var settings = da.GetTenantSettings(TenantId);
                                    if (!settings.RequirePreExistingAccountToLogIn) {
                                        // Create the new account
                                        DataObjects.User addUser = new DataObjects.User {
                                            Added = now,
                                            AddedBy = Source,
                                            Admin = false,
                                            // {{ModuleItemStart:Appointments}}
                                            CanBeScheduled = false,
                                            ManageAppointments = false,
                                            // {{ModuleItemEnd:Appointments}}
                                            Deleted = false,
                                            Email = preferredUsername,
                                            FirstName = givenName,
                                            Enabled = true,
                                            LastModified = now,
                                            LastModifiedBy = Source,
                                            LastName = familyName,
                                            ManageFiles = false,
                                            PreventPasswordChange = false,
                                            Source = Source,
                                            TenantId = TenantId,
                                            UserId = Guid.Empty,
                                            Username = preferredUsername,
                                        };

                                        user = await da.SaveUser(addUser);
                                    }
                                }

                                if (user != null && user.ActionResponse.Result && user.Enabled) {
                                    output.Result = true;
                                    noLocalAccount = false;

                                    await da.UpdateUserFromPlugins(user.UserId);

                                    if (String.IsNullOrWhiteSpace(user.AuthToken)) {
                                        user.AuthToken = da.GetUserToken(TenantId, user.UserId, _fingerprint, user.Sudo);
                                    }
                                    await CustomAuthorization.AddAuthetication(user, context, _fingerprint, Source);

                                    // Write out the user token
                                    CookieWrite("user-token", da.GetUserToken(TenantId, user.UserId, _fingerprint, user.Sudo));
                                    CookieWrite("Login-Method", Source);

                                    if (!user.Sudo) {
                                        await da.UpdateUserLastLoginTime(user.UserId, Source);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (validUser && noLocalAccount) {
            output.Message = "NoLocalAccount";
        }

        return output;
    }

    private string GetClaimType(string claimType)
    {
        string output = claimType;

        if (!String.IsNullOrWhiteSpace(claimType)) {
            if (claimType.Contains(@"\")) {
                claimType = claimType.Replace(@"\", "/");
            }

            if (claimType.Contains("/")) {
                int pos = claimType.LastIndexOf("/");
                output = claimType.Substring(pos + 1);
            }
        }

        return output;
    }
}