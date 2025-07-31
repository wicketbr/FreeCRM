namespace CRM;

public partial class DataObjects
{
    public partial class AjaxLookup : ActionResponseObject
    {
        public Guid TenantId { get; set; }
        public string? Search { get; set; }
        public List<string>? Parameters { get; set; }
        public List<AjaxResults>? Results { get; set; }
    }

    public partial class AjaxResults
    {
        public string? label { get; set; }
        public string? value { get; set; }
        public string? email { get; set; }
        public string? username { get; set; }
        public string? extra1 { get; set; }
        public string? extra2 { get; set; }
        public string? extra3 { get; set; }
    }
}