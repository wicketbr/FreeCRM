namespace CRM;

public partial class DataObjects
{
    public enum WorkflowRenderMode
    {
        Editor,
        Data,
        Unknown,
    }

    public partial class Approval
    {
        public string? Approver { get; set; }
        public Guid DataId { get; set; }
        public Guid? WorkflowProcessId { get; set; }
        public bool Approved { get; set; }
        public string? Signature { get; set; }
    }

    public partial class ApprovalResponse : Approval
    {
        public BooleanResponse ActionResponse { get; set; } = new BooleanResponse();
        public WorkflowProcessing? WorkflowProcessing { get; set; }
        public User? User { get; set; }
        public string? ApprovalReportDetails { get; set; }
    }

    public partial class DynamicWorkflowResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string>? Messages { get; set; }
        public WorkflowProcessState? State { get; set; }
        public dynamic? Object { get; set; }
    }

    public partial class ReProcessWorkflows : ActionResponseObject
    {
        public Guid ParentItemId { get; set; }
        public Guid TenantId { get; set; }
        public Guid DataItemId { get; set; }
        public string? QueryStringValues { get; set; }
        public Guid WorkflowId { get; set; }
        public string WorkflowType { get; set; } = String.Empty;
    }

    public partial class Workflow
    {
        public BooleanResponse ActionResponse { get; set; } = new BooleanResponse();
        public Guid WorkflowId { get; set; }
        public Guid? ParentWorkflowId { get; set; }
        public string? ParentAction { get; set; }
        public Guid TenantId { get; set; }
        public Guid ParentItemId { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string? Rule { get; set; }
        public bool Enabled { get; set; }
        public DateTime? WorkScheduled { get; set; }
        public DateTime? WorkStarted { get; set; }
        public DateTime? WorkCompleted { get; set; }
        public string? Result { get; set; }
        public bool Success { get; set; }
        public Guid? OnSuccess { get; set; }
        public Guid? OnFailure { get; set; }
        public Guid? OnComplete { get; set; }
        public bool Orphaned { get; set; }
        public WorkflowProcessState? State { get; set; }
        public DateTime Added { get; set; }
        public string? AddedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? WorkflowType { get; set; }
    }

    public class WorkflowButtonConfiguration
    {
        public string ButtonClass { get; set; } = "btn btn-primary";
        public string ButtonIcon { get; set; } = String.Empty;
        public string ButtonTitle { get; set; } = String.Empty;
    }

    public partial class WorkflowClearCustomVariable
    {
        public string Name { get; set; } = "";
    }

    public partial class WorkflowCSharp
    {
        public string Code { get; set; } = "";
        public string InvokerFunction { get; set; } = "";
        public List<string> AdditionalAssemblies { get; set; } = new List<string>();
    }

    public partial class WorkflowCustomVariable
    {
        public string AddText { get; set; } = "";
        public string Name { get; set; } = "";
        public List<string> SearchFields { get; set; } = new List<string>();
        public List<string> SearchWords { get; set; } = new List<string>();
    }

    public partial class WorkflowEmail
    {
        public string From { get; set; } = "";
        public List<string> To { get; set; } = new List<string>();
        public List<string> Cc { get; set; } = new List<string>();
        public List<string> Bcc { get; set; } = new List<string>();
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
        public bool IncludeAttachments { get; set; }
        public bool IncludeItemAsAttachment { get; set; }
        public bool AppendRenderedItemToBody { get; set; }
        public AttachmentOptions AttachmentOptions { get; set; } = new AttachmentOptions();
    }

    public partial class WorkflowEmailApproval : WorkflowEmail
    {
        public string ApprovalOption { get; set; } = "";
        public int Retries { get; set; }
        public int RetryInterval { get; set; }
    }

    public partial class WorkflowExternalApp
    {
        public string PathToExe { get; set; } = "";
        public string? Arguments { get; set; }
    }

    public partial class WorkflowFilter
    {
        public List<string> SearchWords { get; set; } = new List<string>();
        public string Action { get; set; } = "";
    }

    public partial class WorkflowProcessing : Workflow
    {
        public Guid WorkflowProcessId { get; set; }
        //public Guid FormId { get; set; }
        public Guid DataItemId { get; set; }
    }

    public partial class WorkflowProcessState
    {
        public Dictionary<string, object>? Objects { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, string>? QueryStringValues { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string>? CustomFieldValues { get; set; } = new Dictionary<string, string>();
    }
}