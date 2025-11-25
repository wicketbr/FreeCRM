using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MySqlX.XDevAPI;

namespace CRM.Server.Hubs
{
    public partial interface IsrHub
    {
        Task SignalRUpdate(DataObjects.SignalRUpdate update);
    }

    [Authorize]
    public partial class crmHub : Hub<IsrHub>
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

        public async Task SignalRUpdate(DataObjects.SignalRUpdate update)
        {
            if (update.TenantId.HasValue) {
                await Clients.Group(update.TenantId.Value.ToString()).SignalRUpdate(update);
            } else {
                // This is a non-tenant-specific update.
                await Clients.All.SignalRUpdate(update);
            }
        }
    }
}
