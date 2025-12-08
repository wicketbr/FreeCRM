using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;

namespace CRM;

public interface ICustomAuthentication
{
    bool Enabled { get; }
    bool UseApple { get; }
    bool UseFacebook { get; }
    bool UseMicrosoftAccount { get; }
    bool UseOpenId { get; }
    public string? OpenIdButtonText { get; }
    public string? OpenIdButtonClass { get; }
    public string? OpenIdButtonIcon { get; }

    bool UseGoogle { get; }
}

public class CustomAuthentication : ICustomAuthentication
{
    private CustomAuthenticationConfiguration _config;

    public CustomAuthentication(CustomAuthenticationConfiguration config)
    {
        _config = config;
    }

    public bool Enabled {
        get {
            return _config.Enabled;
        }
    }

    public bool UseApple {
        get {
            return _config.UseApple;
        }
    }

    public bool UseFacebook {
        get {
            return _config.UseFacebook;
        }
    }

    public bool UseMicrosoftAccount {
        get {
            return _config.UseMicrosoftAccount;
        }
    }

    public bool UseOpenId {
        get {
            return _config.UseOpenId;
        }
    }

    public string? OpenIdButtonText {
        get {
            return _config.OpenIdButtonText;
        }
    }

    public string? OpenIdButtonClass {
        get {
            return _config.OpenIdButtonClass;
        }
    }

    public string? OpenIdButtonIcon {
        get {
            return _config.OpenIdButtonIcon;
        }
    }

    public bool UseGoogle {
        get {
            return _config.UseGoogle;
        }
    }
}

public class CustomAuthenticationConfiguration
{
    public bool Enabled { get; set; }
    public bool UseApple { get; set; }
    public bool UseFacebook { get; set; }
    public bool UseMicrosoftAccount { get; set; }
    public bool UseOpenId { get; set; }
    public string? OpenIdButtonText { get; set; }
    public string? OpenIdButtonClass { get; set; }
    public string? OpenIdButtonIcon { get; set; }
    public bool UseGoogle { get; set; }
}


public static class CustomAuthenticationProviders
{
    public static CustomAuthenticationConfiguration UseAuthorization(WebApplicationBuilder applicationBuilder)
    {
        CustomAuthenticationConfiguration output = new CustomAuthenticationConfiguration();

        string appleClientId = String.Empty;
        string appleKeyId = String.Empty;
        string appleTeamId = String.Empty;
        var appleSignInKey = applicationBuilder.Environment.ContentRootFileProvider.GetFileInfo("SignInWithAppleKey.p8");

        try { appleClientId += applicationBuilder.Configuration["AuthenticationProviders:Apple:ClientId"]; } catch { }
        try { appleKeyId += applicationBuilder.Configuration["AuthenticationProviders:Apple:KeyId"]; } catch { }
        try { appleTeamId += applicationBuilder.Configuration["AuthenticationProviders:Apple:TeamId"]; } catch { }
        if (appleSignInKey.Exists && !String.IsNullOrWhiteSpace(appleClientId) && !String.IsNullOrWhiteSpace(appleKeyId) && !String.IsNullOrWhiteSpace(appleTeamId)) {
            output.Enabled = true;
            output.UseApple = true;
        }

        string facebookAppId = String.Empty;
        string facebookAppSecret = String.Empty;
        try { facebookAppId += applicationBuilder.Configuration["AuthenticationProviders:Facebook:AppId"]; } catch { }
        try { facebookAppSecret += applicationBuilder.Configuration["AuthenticationProviders:Facebook:AppSecret"]; } catch { }
        if (!String.IsNullOrWhiteSpace(facebookAppId) && !String.IsNullOrWhiteSpace(facebookAppSecret)) {
            output.Enabled = true;
            output.UseFacebook = true;
        }

        string microsoftAccountClientId = String.Empty;
        string microsoftAccountClientSecret = String.Empty;
        try { microsoftAccountClientId += applicationBuilder.Configuration["AuthenticationProviders:MicrosoftAccount:ClientId"]; } catch { }
        try { microsoftAccountClientSecret += applicationBuilder.Configuration["AuthenticationProviders:MicrosoftAccount:ClientSecret"]; } catch { }
        if (!String.IsNullOrEmpty(microsoftAccountClientId) && !String.IsNullOrEmpty(microsoftAccountClientSecret)) {
            output.Enabled = true;
            output.UseMicrosoftAccount = true;
        }

        string openIdClientId = String.Empty;
        string openIdClientSecret = String.Empty;
        string openIdAuthority = String.Empty;
        string openIdButtonText = String.Empty;
        string openIdButtonClass = String.Empty;
        string openIdButtonIcon = String.Empty;

        try { openIdClientId += applicationBuilder.Configuration["AuthenticationProviders:OpenId:ClientId"]; } catch { }
        try { openIdClientSecret += applicationBuilder.Configuration["AuthenticationProviders:OpenId:ClientSecret"]; } catch { }
        try { openIdAuthority += applicationBuilder.Configuration["AuthenticationProviders:OpenId:Authority"]; } catch { }
        try { openIdButtonText += applicationBuilder.Configuration["AuthenticationProviders:OpenId:ButtonText"]; } catch { }
        try { openIdButtonClass += applicationBuilder.Configuration["AuthenticationProviders:OpenId:ButtonClass"]; } catch { }
        try { openIdButtonIcon += applicationBuilder.Configuration["AuthenticationProviders:OpenId:ButtonIcon"]; } catch { }

        if (!String.IsNullOrEmpty(openIdClientId) && !String.IsNullOrEmpty(openIdClientSecret) && !String.IsNullOrEmpty(openIdAuthority)) {
            output.Enabled = true;
            output.UseOpenId = true;
            output.OpenIdButtonText = openIdButtonText;
            output.OpenIdButtonClass = openIdButtonClass;
            output.OpenIdButtonIcon = openIdButtonIcon;
        }

        string googleClientId = String.Empty;
        string googleClientSecret = String.Empty;
        try { googleClientId += applicationBuilder.Configuration["AuthenticationProviders:Google:ClientId"]; } catch { }
        try { googleClientSecret += applicationBuilder.Configuration["AuthenticationProviders:Google:ClientSecret"]; } catch { }
        if (!String.IsNullOrEmpty(googleClientId) && !String.IsNullOrEmpty(googleClientSecret)) {
            output.Enabled = true;
            output.UseGoogle = true;
        }

        var auth = applicationBuilder.Services.AddAuthentication(options => {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        });

        auth.AddCookie();

        if (output.UseApple) {
            //var memory = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(applePrivateKey));
            //var bytes = Encoding.UTF8.GetBytes(applePrivateKey);
            //var str = new ReadOnlyMemory<char>(applePrivateKey.ToCharArray());

            auth.AddApple(o => {
                o.ClientId = appleClientId;
                o.KeyId = appleKeyId;
                o.TeamId = appleTeamId;
                o.AccessDeniedPath = "/Authorization/AccessDenied";

                o.UsePrivateKey((keyId) =>
                    applicationBuilder.Environment.ContentRootFileProvider.GetFileInfo("SignInWithAppleKey.p8"));
            });
        }

        if (output.UseFacebook) {
            auth.AddFacebook("Facebook", o => {
                o.AppId = facebookAppId;
                o.AppSecret = facebookAppSecret;
                o.AccessDeniedPath = "/Authorization/AccessDenied";
            });
        }

        if (output.UseGoogle) {
            auth.AddGoogle("Google", o => {
                o.ClientId = googleClientId;
                o.ClientSecret = googleClientSecret;
                o.AccessDeniedPath = "/Authorization/AccessDenied";
            });
        }

        if (output.UseMicrosoftAccount) {
            auth.AddMicrosoftAccount("MicrosoftAccount", o => {
                o.ClientId = microsoftAccountClientId;
                o.ClientSecret = microsoftAccountClientSecret;
                o.AuthorizationEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
                o.AccessDeniedPath = "/Authorization/AccessDenied";
            });
        }

        if (output.UseOpenId) {
            auth.AddOpenIdConnect("OpenId", o => {
                o.ClientId = openIdClientId;
                o.ClientSecret = openIdClientSecret;
                o.Authority = openIdAuthority;
                o.ResponseType = "code";
                o.GetClaimsFromUserInfoEndpoint = true;
                o.AccessDeniedPath = "/Authorization/AccessDenied";
            });
        }

        return output;
    }
}