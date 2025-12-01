using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Plugins;
using SQLitePCL;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CRM;

public static class CustomAuthorization
{
    public static async Task AddAuthetication(DataObjects.User user, HttpContext context, string fingerprint, string loginSource)
    {
        if (user.ActionResponse.Result && user.Enabled) {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.Role, "Enabled"));
            identity.AddClaim(new Claim("Source", loginSource));
            identity.AddClaim(new Claim(ClaimTypes.Thumbprint, fingerprint));

            if (!String.IsNullOrWhiteSpace(user.AuthToken)) {
                identity.AddClaim(new Claim(ClaimTypes.Hash, user.AuthToken));
            }

            if (!String.IsNullOrWhiteSpace(user.Email)) {
                identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
            }

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.GroupSid, user.TenantId.ToString()));

            if (!String.IsNullOrWhiteSpace(user.Username)) {
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
            }

            if (user.Admin) {
                identity.AddClaim(new Claim(ClaimTypes.Role, Policies.Admin));
            }

            if (user.AppAdmin) {
                identity.AddClaim(new Claim(ClaimTypes.Role, Policies.AppAdmin));
            }

            if (user.CanBeScheduled) {
                identity.AddClaim(new Claim(ClaimTypes.Role, Policies.CanBeScheduled));
            }

            if (user.ManageAppointments) {
                identity.AddClaim(new Claim(ClaimTypes.Role, Policies.ManageAppointments));
            }

            if (user.ManageFiles) {
                identity.AddClaim(new Claim(ClaimTypes.Role, Policies.ManageFiles));
            }

            if (user.PreventPasswordChange) {
                identity.AddClaim(new Claim(ClaimTypes.Role, Policies.PreventPasswordChange));
            }

            var principal = new ClaimsPrincipal(identity);
            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
                    IssuedUtc = DateTimeOffset.UtcNow,
                }
            );
        }
    }
}

public static class Policies
{
    public const string Admin = "Admin";
    public const string AppAdmin = "AppAdmin";
    public const string CanBeScheduled = "CanBeScheduled";
    public const string ManageFiles = "ManageFiles";
    public const string ManageAppointments = "ManageAppointments";
    public const string PreventPasswordChange = "PreventPasswordChange";
}