using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpPost]
    [AllowAnonymous]
    [Route("~/api/Data/Authenticate")]
    public async Task<ActionResult<DataObjects.User>> Authenticate(DataObjects.Authenticate authenticate)
    {
        DataObjects.User output = new DataObjects.User();

        if (authenticate != null && !String.IsNullOrEmpty(authenticate.Username) && !String.IsNullOrEmpty(authenticate.Password)) {
            output = await da.Authenticate(authenticate, _fingerprint);

            if (output.ActionResponse.Result && output.Enabled && context != null) {
                if (String.IsNullOrWhiteSpace(output.AuthToken)) {
                    output.AuthToken = da.GetUserToken(output.TenantId, output.UserId, _fingerprint, output.Sudo);
                }

                await CustomAuthorization.AddAuthetication(output, context, _fingerprint, "local");
            }
        }

        return Ok(output);
    }
}