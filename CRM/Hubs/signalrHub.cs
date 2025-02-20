using Microsoft.AspNetCore.SignalR;

namespace CRM.Server.Hubs
{
    public class crmhub : Hub
    {
        private List<string> tenants = new List<string>();

        public async Task JoinTenantId(string TenantId)
        {
            if (!tenants.Contains(TenantId)) {
                tenants.Add(TenantId);
            }

            // Before adding a user to a Tenant group remove them from any groups they were in before.
            if (tenants != null && tenants.Count() > 0) {
                foreach (var tenant in tenants) {
                    try {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenant);
                    } catch { }
                }
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, TenantId);
        }
    }
}
