using CRM.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CRM.Server.Controllers;

[ApiController]
public partial class DataController : ControllerBase
{
    private HttpContext? context;
    private ICustomAuthentication? authenticationProviders;
    private IDataAccess da;
    private DataObjects.User CurrentUser;
    private Guid TenantId = Guid.Empty;
    private IConfigurationHelper configurationHelper;
    private Plugins.IPlugins plugins;

    private readonly IHubContext<crmHub, IsrHub>? _signalR;

    private string _fingerprint = "";
    private string _returnCodeAccessDenied = "{{AccessDenied}}";

    public DataController(IDataAccess daInjection,
        IHttpContextAccessor httpContextAccessor,
        ICustomAuthentication auth,
        IHubContext<crmHub, IsrHub> hubContext,
        IConfigurationHelper configHelper,
        Plugins.IPlugins diPlugins)
    {
        da = daInjection;
        authenticationProviders = auth;
        configurationHelper = configHelper;
        plugins = diPlugins;
        _signalR = hubContext;

        if (authenticationProviders != null) {
            da.SetAuthenticationProviders(new DataObjects.AuthenticationProviders {
                UseApple = authenticationProviders.UseApple,
                UseFacebook = authenticationProviders.UseFacebook,
                UseGoogle = authenticationProviders.UseGoogle,
                UseMicrosoftAccount = authenticationProviders.UseMicrosoftAccount,
                UseOpenId = authenticationProviders.UseOpenId,
                OpenIdButtonText = authenticationProviders.OpenIdButtonText,
                OpenIdButtonClass = authenticationProviders.OpenIdButtonClass,
                OpenIdButtonIcon = authenticationProviders.OpenIdButtonIcon
            });
        }

        if (httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
            context = httpContextAccessor.HttpContext;
        }

        // See if a TenantId is included in the header or querystring.
        string tenantId = HeaderValue("TenantId");
        if (String.IsNullOrEmpty(tenantId)) {
            tenantId = QueryStringValue("TenantId");
        }
        if (!String.IsNullOrEmpty(tenantId)) {
            try {
                TenantId = new Guid(tenantId);
            } catch { }
        }

        // See if a Token is included in the header or querystring.
        string Token = HeaderValue("Token");
        if (String.IsNullOrWhiteSpace(Token)) {
            Token = QueryStringValue("Token");
        }

        // See if a Fingerprint is included in the header or querystring.
        string fingerprint = HeaderValue("Fingerprint");
        if (String.IsNullOrWhiteSpace(fingerprint)) {
            fingerprint = QueryStringValue("Fingerprint");
        }
        if (!String.IsNullOrWhiteSpace(fingerprint)) {
            _fingerprint = fingerprint;
        }

        // Set the CurrentUser to a new User object and if we have a valid Token
        // use that to get set the CurrentUser.
        CurrentUser = new DataObjects.User();
        if (context?.User?.Identity?.IsAuthenticated == true) {
            try {
                string token = context?.User?.FindFirstValue(ClaimTypes.Hash) ?? string.Empty;
                if (!String.IsNullOrWhiteSpace(token)) {
                    try {
                        _fingerprint = context?.User?.FindFirstValue(ClaimTypes.Thumbprint) ?? string.Empty;
                        string tenantIdString = context?.User?.FindFirstValue(ClaimTypes.GroupSid) ?? string.Empty;

                        TenantId = Guid.Empty;
                        Guid.TryParse(tenantIdString, out TenantId);

                        CurrentUser = da.GetUserFromToken(TenantId, token, _fingerprint).Result;
                    } catch { }
                }
            } catch { }
        }

        // If the user wasn't loaded from the custom auth provider, but we have a token, load the user from the token.
        if (!CurrentUser.ActionResponse.Result && !String.IsNullOrWhiteSpace(Token)) {
            CurrentUser = da.GetUserFromToken(TenantId, Token, _fingerprint).Result;
        }
    }

    private string HeaderValue(String ValueName)
    {
        string output = String.Empty;
        try {
            if (Request != null) {
                output += Request.Headers[ValueName];
            } else if (context != null && context.Request != null) {
                output += context.Request.Headers[ValueName];
            }
        } catch { }

        return output;
    }

    private string QueryStringValue(String ValueName)
    {
        string output = String.Empty;
        try {
            if (Request != null) {
                output = Request.Query[ValueName].ToString();
            } else if (context != null && context.Request != null) {
                output = context.Request.Query[ValueName].ToString();
            }
        } catch { }
        return output;
    }
}