using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class PluginCache
{
    public Guid RecordId { get; set; }

    public Guid Id { get; set; }

    public string? Author { get; set; }

    public string? Name { get; set; }

    public string? Type { get; set; }

    public string? Version { get; set; }

    public string? Properties { get; set; }

    public string? Namespace { get; set; }

    public string? ClassName { get; set; }

    public string? Code { get; set; }

    public string? AdditionalAssemblies { get; set; }

    public bool StillExists { get; set; }
}
