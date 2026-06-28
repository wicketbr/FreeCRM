using Microsoft.JSInterop;
using Radzen;
using System.Text;

namespace CRM.Client;

public static class WorkflowEngine
{
    private static DataObjects.User? _currentUser = null;
    private static bool _initialized = false;
    private static IJSRuntime jsRuntime = null!;
    private static Guid _lastWorkflowProcessed = Guid.Empty;
    private static int _lastWorkflowProcessedLevel = 0;
    private static string _linkStyles = String.Empty;
    private static BlazorDataModel Model = null!;
    private static bool _readOnly = false;
    private static int _workflowLinkCount = 0;
    private static DataObjects.WorkflowRenderMode _workflowRenderMode = DataObjects.WorkflowRenderMode.Unknown;

    // using the same colors as Bootstrap for consistency.
    private static string _colorBlue = "#0a58ca";
    private static string _colorGreen = "#146c43";
    private static string _colorRed = "#b02a37";

    private static string _linkStroke = " stroke-width:2px,stroke:";

    public static void Init(IJSRuntime jSRuntime, BlazorDataModel model)
    {
        jsRuntime = jSRuntime;
        Model = model;

        _initialized = true;
    }

    /// <summary>
    /// Indicates if this helpers class has been initialized.
    /// </summary>
    public static bool Initialized {
        get {
            return _initialized;
        }
    }

    public static void DeleteWorkflow(string workflowId, List<DataObjects.Workflow> workflows, List<DataObjects.Workflow> orphans)
    {
        var workflow = GetWorkflow(workflowId, workflows, orphans);
        if (workflow != null) {
            // If this is already in the orphans collection, then just remove it.
            if (workflow.Orphaned) {
                orphans.Remove(workflow);
                return;
            }

            if (Helpers.GuidValue(workflow.ParentWorkflowId) != Guid.Empty) {
                // This item has a parent, so remove the reference in the parent.
                var parentWorkflow = GetWorkflow(workflow.ParentWorkflowId, workflows, orphans);
                if (parentWorkflow != null) {
                    if (parentWorkflow.OnSuccess == workflow.WorkflowId) {
                        parentWorkflow.OnSuccess = null;
                    } else if (parentWorkflow.OnFailure == workflow.WorkflowId) {
                        parentWorkflow.OnFailure = null;
                    } else if (parentWorkflow.OnComplete == workflow.WorkflowId) {
                        parentWorkflow.OnComplete = null;
                    }
                }
            }

            workflow.ParentAction = String.Empty;
            workflow.ParentWorkflowId = null;

            // Remove the item from the workflows collection and add it to the orphans collection.
            workflows.Remove(workflow);

            workflow.Orphaned = true;
            orphans.Add(workflow);
        }
    }

    /// <summary>
    /// Gets a workflow by its unique Guid id.
    /// </summary>
    /// <param name="workflowId">The unique id.</param>
    /// <returns>A Workflow object or null.</returns>
    public static DataObjects.Workflow? GetWorkflow(Guid? workflowId, List<DataObjects.Workflow> workflows, List<DataObjects.Workflow> orphans)
    {
        DataObjects.Workflow? output = null;

        if (workflowId.HasValue) {
            if (workflows.Count > 0) {
                output = workflows.FirstOrDefault(x => x.WorkflowId == workflowId);
            }

            if (output == null) {
                // Check the orphans
                if (orphans.Count > 0) {
                    output = orphans.FirstOrDefault(x => x.WorkflowId == workflowId);
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Gets a workflow by its unique Guid id as a string.
    /// </summary>
    /// <param name="workflowId">The unique id as a string.</param>
    /// <returns>A Workflow object or null.</returns>
    public static DataObjects.Workflow? GetWorkflow(string? workflowId, List<DataObjects.Workflow> workflows, List<DataObjects.Workflow> orphans)
    {
        DataObjects.Workflow? output = null;

        if (!String.IsNullOrWhiteSpace(workflowId)) {
            output = workflows.FirstOrDefault(x => x.WorkflowId.ToString() == workflowId);

            if (output == null) {
                // Check the orphans
                if (orphans.Count > 0) {
                    output = orphans.FirstOrDefault(x => x.WorkflowId.ToString() == workflowId);
                    if (output != null) {
                        output.Orphaned = true;
                    }
                }
            } else {
                output.Orphaned = false;
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the list of available workflow types.
    /// </summary>
    /// <returns></returns>
    public static List<DataObjects.SimpleList> GetWorkflowTypes()
    {
        // TO DO: Add the remaining types (C#, EmailApproval, etc.)

        var output = new List<DataObjects.SimpleList> {
            new DataObjects.SimpleList {
                Value = "Email",
                Label = Helpers.Text("WorkflowTypeEmail"),
            },
            new DataObjects.SimpleList {
                Value = "EmailApproval",
                Label = Helpers.Text("WorkflowTypeEmailApproval"),
            },
            new DataObjects.SimpleList {
                Value = "CSharp",
                Label = Helpers.Text("WorkflowTypeCSharp"),
            },
            new DataObjects.SimpleList {
                Value = "Filter",
                Label = Helpers.Text("WorkflowTypeFilter"),
            },
            new DataObjects.SimpleList {
                Value = "CustomVariable",
                Label = Helpers.Text("WorkflowTypeCustomVariable"),
            },
            new DataObjects.SimpleList {
                Value = "ClearCustomVariable",
                Label = Helpers.Text("WorkflowTypeClearCustomVariable"),
            },
            new DataObjects.SimpleList {
                Value = "Delete",
                Label = Helpers.Text("WorkflowTypeDelete"),
            },
            new DataObjects.SimpleList {
                Value = "ExternalApp",
                Label = Helpers.Text("WorkflowTypeExternalApp"),
            },
        };

        // Add any plugins
        foreach (var plugin in Model.Plugins_Workflows) {
            output.Add(new DataObjects.SimpleList {
                Value = "plugin:" + plugin.Id.ToString(),
                Label = Helpers.Text("Plugin") + ": " + plugin.Name,
            });
        }

        return output.OrderBy(x => x.Label).ToList();
    }

    public static void MoveWorkflow(string workflowId, string target, string targetWorkflowId, List<DataObjects.Workflow> workflows, List<DataObjects.Workflow> orphans)
    {
        // Targets will either be "root", "success", "complete", or "failure".
        var workflow = GetWorkflow(workflowId, workflows, orphans);
        var targetWorkflow = GetWorkflow(targetWorkflowId, workflows, orphans);

        if (workflow != null) {
            var parentWorkflow = GetWorkflow(workflow.ParentWorkflowId, workflows, orphans);

            if (target == "root") {
                // If we are moving this item to the root, then the current items at the root get moved to orphans.
                var firstWorkflow = workflows.FirstOrDefault(x => x.ParentWorkflowId == null || x.ParentWorkflowId == Guid.Empty);

                if (firstWorkflow != null) {
                    firstWorkflow.Orphaned = true;
                    firstWorkflow.ParentAction = String.Empty;
                    firstWorkflow.ParentWorkflowId = null;
                    workflows.Remove(firstWorkflow);
                    orphans.Add(firstWorkflow);
                }

                // Set this item as the root.
                workflow.ParentAction = String.Empty;
                workflow.ParentWorkflowId = null;
            } else if (targetWorkflow != null) {
                // If there was an existing item in the target location move it to orphans.
                DataObjects.Workflow? existingTarget = null;

                switch (target.ToLower()) {
                    case "success":
                        existingTarget = GetWorkflow(targetWorkflow.OnSuccess, workflows, orphans);

                        targetWorkflow.OnSuccess = workflow.WorkflowId;
                        workflow.ParentAction = "success";
                        workflow.ParentWorkflowId = targetWorkflow.WorkflowId;
                        break;
                    case "complete":
                        existingTarget = GetWorkflow(targetWorkflow.OnComplete, workflows, orphans);

                        targetWorkflow.OnComplete = workflow.WorkflowId;
                        workflow.ParentAction = "complete";
                        workflow.ParentWorkflowId = targetWorkflow.WorkflowId;
                        break;
                    case "failure":
                        existingTarget = GetWorkflow(targetWorkflow.OnFailure, workflows, orphans);

                        targetWorkflow.OnFailure = workflow.WorkflowId;
                        workflow.ParentAction = "failure";
                        workflow.ParentWorkflowId = targetWorkflow.WorkflowId;
                        break;
                }

                if (existingTarget != null) {
                    // There was a workflow item in the target location, so move it to orphans.
                    existingTarget.Orphaned = true;
                    existingTarget.ParentAction = String.Empty;
                    existingTarget.ParentWorkflowId = null;
                    workflows.Remove(existingTarget);
                    orphans.Add(existingTarget);
                }
            }

            // If the workflow was previously orphaned, then remove it from the orphans collection and add it to the workflows collection.
            if (workflow.Orphaned) {
                orphans.Remove(workflow);
                workflows.Add(workflow);
                workflow.Orphaned = false;
            }

            if (parentWorkflow != null) {
                // Remove the workflow from its previous parent.
                if (parentWorkflow.OnComplete == workflow.WorkflowId) {
                    parentWorkflow.OnComplete = null;
                } else if (parentWorkflow.OnFailure == workflow.WorkflowId) {
                    parentWorkflow.OnFailure = null;
                } else if (parentWorkflow.OnSuccess == workflow.WorkflowId) {
                    parentWorkflow.OnSuccess = null;
                }
            }
        }
    }

    //public static string ObjectPropertiesToUnorderedList(object o)
    //{
    //    string output = String.Empty;

    //    // Go through each property in the object and output nest UL/LI items.
    //    // If the property is a class, call this function recursively.
    //    try {
    //        foreach (var prop in o.GetType().GetProperties()) {
    //            try {
    //                var value = prop.GetValue(o, null);
    //                if (value != null) {
    //                    if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string)) {
    //                        output += $"<li>{prop.Name}:<ul>{ObjectPropertiesToUnorderedList(value)}</ul></li>";
    //                    } else {
    //                        output += $"<li>{prop.Name}: {value}</li>";
    //                    }
    //                }
    //            } catch { }
    //        }
    //    } catch { }

    //    if (!String.IsNullOrWhiteSpace(output)) {
    //        return "<ul>" + output + "</ul>";
    //    } else {
    //        return String.Empty;
    //    }
    //}

    /// <summary>
    /// Renders a workflow diagram using mermaid.js.
    /// </summary>
    /// <param name="elementId">The id of the element where the graph will render.</param>
    /// <param name="data">The graph data.</param>
    public static async Task RenderWorkflowDiagram(string elementId, string data, double zoom)
    {
        await jsRuntime.InvokeVoidAsync("RenderWorkflowDiagram", elementId, data, zoom);
    }

    /// <summary>
    /// Sets the zoom for the workflow diagram.
    /// </summary>
    /// <param name="elementId">The id of the element where the graph will render.</param>
    /// <param name="zoom">The zoom level between 0.3 and 4.0.</param>
    public static async Task RenderWorkflowDiagramZoom(string elementId, double zoom)
    {
        await jsRuntime.InvokeVoidAsync("RenderWorkflowDiagramZoom", elementId, zoom);
    }

    /// <summary>
    /// Renders an individual workflow element. Gets called recursively for the various child elements.
    /// </summary>
    /// <param name="workflow">The workflow object to render.</param>
    /// <param name="condition">The condition: success, failure, complete</param>
    /// <param name="enabledTree">Indicates if there should be a difference in appearance for Enabled/Disabled items.</param>
    /// <param name="hideButtons">An option to hide some of the buttons.</param>
    /// <param name="level">The level in the tree.</param>
    /// <param name="orphan">Indicates if the item being rendered is an orphan.</param>
    /// <returns>The string containing the format required for Mermaid.js</returns>
    public static string RenderWorkflowItem
    (
        List<DataObjects.Workflow>? dataWorkflowItems,
        List<DataObjects.Workflow> workflows,
        List<DataObjects.Workflow> orphans,
        DataObjects.Workflow workflow,
        string condition,
        bool enabledTree,
        bool hideButtons,
        int level,
        bool orphan = false
    )
    {
        StringBuilder output = new StringBuilder();

        if (_readOnly || _workflowRenderMode == DataObjects.WorkflowRenderMode.Data) {
            hideButtons = true;

            if (dataWorkflowItems != null) {
                var wf = dataWorkflowItems.FirstOrDefault(x => x.WorkflowId == workflow.WorkflowId);
                if (wf != null) {
                    workflow = wf;
                }
            }
        }

        if (workflow != null) {
            workflow.Orphaned = orphan;

            var onSuccess = GetWorkflow(workflow.OnSuccess, workflows, orphans);
            var onComplete = GetWorkflow(workflow.OnComplete, workflows, orphans);
            var onFailure = GetWorkflow(workflow.OnFailure, workflows, orphans);

            string inactiveDivClass = String.Empty;
            string inactiveStyleOpacity = String.Empty;

            if (enabledTree && !workflow.Enabled) {
                inactiveDivClass = " inactive";
                //inactiveStyleOpacity = "opacity:0.6,";
            }

            string id = workflow.WorkflowId.ToString();

            if (!orphan && Helpers.GuidValue(workflow.ParentWorkflowId) == Guid.Empty) {
                output.AppendLine("S-->" + id + ";");
                output.AppendLine("");
                _workflowLinkCount++;

                _linkStyles += "linkStyle " + _workflowLinkCount.ToString() + _linkStroke + _colorGreen + Environment.NewLine;
            }

            output.AppendLine("style " + id + " fill:#555,stroke:#fff,stroke-width:2px,color:#fff;");
            output.AppendLine("");

            string buttons = "<table border='0' class='workflow-buttons' style='width:100%;'>" +
                "<tr class='tr-target " + id + "'>";

            string moveButtonSuccess =
                Helpers.DotNetHelperCallbackButton(
                    id + "-move-success",
                    "",
                    new List<string> { "workflows", "button", "move-here", "success", id },
                    "btn btn-xs btn-warning btn-move-target btn-move-target-success " + id,
                    "MoveHere",
                    Helpers.Text("WorkflowMoveHere")
                );

            string moveButtonComplete =
                Helpers.DotNetHelperCallbackButton(
                    id + "-move-complete",
                    "",
                    new List<string> { "workflows", "button", "move-here", "complete", id },
                    "btn btn-xs btn-warning btn-move-target btn-move-target-complete " + id,
                    "MoveHere",
                    Helpers.Text("WorkflowMoveHere")
                );

            string moveButtonFailure =
                Helpers.DotNetHelperCallbackButton(
                    id + "-move-failure",
                    "",
                    new List<string> { "workflows", "button", "move-here", "failure", id },
                    "btn btn-xs btn-warning btn-move-target btn-move-target-failure " + id,
                    "MoveHere",
                    Helpers.Text("WorkflowMoveHere")
                );

            buttons += "<td class='left'>" + moveButtonSuccess;

            if (onSuccess != null) {
                output.AppendLine(RenderWorkflowItem(dataWorkflowItems, workflows, orphans, onSuccess, "success", enabledTree, hideButtons, level + 1, orphan));
                output.AppendLine("");

                output.AppendLine(RenderWorkflowLine(workflow, onSuccess, "success"));
                output.AppendLine("");

                _linkStyles += "linkStyle " + _workflowLinkCount.ToString() + _linkStroke + _colorGreen + Environment.NewLine;

                buttons += "<button class='btn btn-xs btn-success' disabled>" + Helpers.Text("WorkflowOnSuccess") + "</button>";
                //buttons += "<span class='badge text-bg-success'>" + Helpers.Text("WorkflowOnSuccess") + "</span>";
            } else {
                buttons += Helpers.DotNetHelperCallbackButton(
                    id + "on-success",
                    Helpers.Text("WorkflowOnSuccess"),
                    new List<string> { "workflows", "button", "addchild", "success", id },
                    "btn btn-xs btn-success btn-add-on-success",
                    Helpers.Text("WorkflowSuccessTip")
                );
            }

            buttons += "</td><td class='center'>" + moveButtonComplete;

            if (onComplete != null) {
                output.AppendLine(RenderWorkflowItem(dataWorkflowItems, workflows, orphans, onComplete, "complete", enabledTree, hideButtons, level + 1, orphan));
                output.AppendLine("");

                output.AppendLine(RenderWorkflowLine(workflow, onComplete, "complete"));
                output.AppendLine("");

                _linkStyles += "linkStyle " + _workflowLinkCount.ToString() + _linkStroke + _colorBlue + Environment.NewLine;
                output.AppendLine("");

                buttons += "<button class='btn btn-xs btn-primary' disabled>" + Helpers.Text("WorkflowOnComplete") + "</button>";
                //buttons += "<span class='badge text-bg-primary'>" + Helpers.Text("WorkflowOnComplete") + "</span>";
            } else {
                buttons += Helpers.DotNetHelperCallbackButton(
                    id + "on-complete",
                    Helpers.Text("WorkflowOnComplete"),
                    new List<string> { "workflows", "button", "addchild", "complete", id },
                    "btn btn-xs btn-primary btn-add-on-complete",
                    Helpers.Text("WorkflowCompleteTip")
                );
            }

            buttons += "</td><td class='right'>" + moveButtonFailure;

            if (onFailure != null) {
                output.AppendLine(RenderWorkflowItem(dataWorkflowItems, workflows, orphans, onFailure, "failure", enabledTree, hideButtons, level + 1, orphan));
                output.AppendLine("");

                output.AppendLine(RenderWorkflowLine(workflow, onFailure, "failure"));
                output.AppendLine("");

                _linkStyles += "linkStyle " + _workflowLinkCount.ToString() + _linkStroke + _colorRed + Environment.NewLine;
                output.AppendLine("");

                buttons += "<button class='btn btn-xs btn-danger' disabled>" + Helpers.Text("WorkflowOnFailure") + "</button>";
                //buttons += "<span class='badge text-bg-danger'>" + Helpers.Text("WorkflowOnFailure") + "</span>";
            } else {
                buttons += Helpers.DotNetHelperCallbackButton(
                    id + "on-failure",
                    Helpers.Text("WorkflowOnFailure"),
                    new List<string> { "workflows", "button", "addchild", "failure", id },
                    "btn btn-xs btn-danger btn-add-on-failure",
                    Helpers.Text("WorkflowFailureTip")
                );
            }


            buttons += "</td></tr></table>";

            if (hideButtons) {
                buttons = String.Empty;
            }

            var workflowName = workflow.Name.Replace("\"", "&quot;").Replace("'", "&#39;");

            string title = workflow.Enabled
                ? "<i class='" + Helpers.Icon("Checked") + "'></i> " + workflowName
                : "<div class='note'><i class='" + Helpers.Icon("Unchecked") + "'></i> " + workflowName + "</div>";

            output.Append(id + "(\"<div class='workflow-item" + inactiveDivClass + "" + (_workflowRenderMode == DataObjects.WorkflowRenderMode.Data ? " data-view" : "") + "'>");

            output.Append(RenderWorkflowButtons(workflow, hideButtons, orphan));
            output.Append("<h1>" + title + "</h1>");

            if (_workflowRenderMode == DataObjects.WorkflowRenderMode.Editor) {
                output.Append("<div>" + buttons + "</div>");
            } else if (_workflowRenderMode == DataObjects.WorkflowRenderMode.Data) {
                if (_currentUser != null && _currentUser.Admin && workflow.Enabled) {
                    // Allow admin users (the only ones that ever see this view) to reprocess a workflow item.
                    var reprocessButton = Helpers.DotNetHelperCallbackButton(
                        id + "-reprocess",
                        Helpers.Text(workflow.WorkScheduled.HasValue ? "ReProcess" : "Process"),
                        new List<string> {
                            "reprocessworkflow",
                            workflow.WorkflowId.ToString() ,
                        },
                        "btn btn-xs me-1 " + (workflow.WorkScheduled.HasValue ? "btn-warning" : "btn-danger"),
                        "Reprocess",
                        Helpers.Text("ReProcess")
                    );
                    output.Append("  <div class='mb-1'>" + reprocessButton + "</div>");
                }

                if (!workflow.WorkScheduled.HasValue) {
                    //output.Append("<br /><br />");
                    output.Append("<span class='badge bg-secondary'>");
                    output.Append("  <i class='far fa-calendar-times' title='" + Helpers.Text("WorkflowUnscheduled") + "'></i>");
                    output.Append("</span> " + Helpers.Text("WorkflowUnscheduled"));
                } else {
                    //output.Append("<br /><br />");
                    output.Append("<span class='badge bg-secondary'>");
                    output.Append("  <i class='far fa-calendar-times' title='" + Helpers.Text("WorkflowScheduled") + "'></i>");
                    output.Append("</span> " + Helpers.FormatDateTime(workflow.WorkScheduled));

                    output.Append("<br />");
                    output.Append("<span class='badge bg-primary'>");
                    output.Append("  <i class='fas fa-running' title='" + Helpers.Text("WorkflowStarted") + "'></i>");
                    output.Append("</span> " + Helpers.FormatDateTime(workflow.WorkStarted));

                    if (workflow.WorkCompleted.HasValue) {
                        if (level > _lastWorkflowProcessedLevel) {
                            _lastWorkflowProcessed = workflow.WorkflowId;
                            _lastWorkflowProcessedLevel = level;
                        }

                        if (workflow.Success) {
                            output.Append("<br />");
                            output.Append("<span class='badge bg-success'>");
                            output.Append("  <i class='far fa-calendar-check' title='" + Helpers.Text("WorkflowCompleted") + "'></i>");
                            output.Append("</span> " + Helpers.FormatDateTime(workflow.WorkCompleted));
                            output.Append(" <span class='badge bg-success'>" + Helpers.Text("WorkflowSuccess") + "</span>");
                        } else {
                            output.Append("<br />");
                            output.Append("<span class='badge bg-danger'>");
                            output.Append("  <i class='far fa-calendar-times' title='" + Helpers.Text("WorkflowFailed") + "'></i>");
                            output.Append("</span> " + Helpers.FormatDateTime(workflow.WorkCompleted));
                            output.Append(" <span class='badge bg-danger'>" + Helpers.Text("WorkflowFailed") + "</span>");
                        }
                    } else {
                        output.Append("<br />");
                        output.Append("<span class='badge bg-secondary'>");
                        output.Append("  <i class='far fa-calendar-check' title='" + Helpers.Text("WorkflowNotCompleted") + "'></i>");
                        output.Append("</span>");
                    }

                    if (workflow.State != null) {
                        output.Append("<div class='state'>");
                        output.Append("  <h2 class='state'>" + Helpers.Text("WorkflowState") + "</h2>");

                        if (workflow.State.CustomFieldValues != null && workflow.State.CustomFieldValues.Count > 0) {
                            output.Append("  <h3>" + Helpers.Text("WorkflowCustomFieldValues") + "</h3>");
                            output.Append("  <ul>");

                            foreach (var customFieldValue in workflow.State.CustomFieldValues) {
                                output.Append("  <li>" + customFieldValue.Key + ": " + customFieldValue.Value + "</li>");
                            }

                            output.Append("  </ul>");
                        }

                        if (workflow.State.QueryStringValues != null && workflow.State.QueryStringValues.Count > 0) {
                            output.Append("  <h3>" + Helpers.Text("WorkflowQuerystringValues") + "</h3>");
                            output.Append("  <ul>");

                            foreach (var qsValue in workflow.State.QueryStringValues) {
                                output.Append("  <li>" + qsValue.Key + ": " + qsValue.Value + "</li>");
                            }

                            output.Append("  </ul>");
                        }

                        if (workflow.State.Objects != null && workflow.State.Objects.Count > 0) {
                            output.Append("  <h3>" + Helpers.Text("WorkflowObjects") + "</h3>");
                            int objectIndex = -1;

                            foreach (var o in workflow.State.Objects) {
                                objectIndex++;

                                var objectCopyButton = Helpers.DotNetHelperCallbackButton(
                                    id + "-copyobject",
                                    Helpers.Text("Copy"),
                                    new List<string> {
                                            "workflows",
                                            "copyobject",
                                            workflow.WorkflowId.ToString() ,
                                            objectIndex.ToString()
                                    },
                                    "btn btn-xs btn-primary me-1",
                                    "CopyToClipboard",
                                    Helpers.Text("CopyToClipboard")
                                );

                                output.Append("  <div class='mb-1'>" + objectCopyButton + o.Key + "</div>");

                                //string renderedObject = String.Empty;

                                //string oValue = String.Empty;
                                //try {
                                //    oValue += o.Value.ToString();
                                //} catch { }

                                //if (!String.IsNullOrWhiteSpace(oValue)) {
                                //    renderedObject = oValue
                                //        .Replace("\",\"", "&quot;, &quot;")
                                //        .Replace("\"", "&quot;")
                                //        .Replace("'", "&apos;")
                                //        .Replace("/", "\\/")
                                //        .Replace("\b", "{B} ")
                                //        .Replace("\f", "{F} ")
                                //        .Replace("\n", "{N} ")
                                //        .Replace("\r", "{R} ")
                                //        .Replace("\t", "{T} ");
                                //}

                                //if (!String.IsNullOrEmpty(renderedObject)) {


                                //    //output.Append("  <div>" + o.Key + " " + objectCopyButton + "</div>");
                                //    //output.Append("  <div class='workflow-object'>");
                                //    //output.Append("    <textarea id='workflow-object-" + Helpers.NewCleanGuid() + "' disabled aria-label='" + Helpers.Text("WorkflowObjectData") + "'>" + renderedObject + "</textarea>");
                                //    //output.Append("  </div>");
                                //}
                            }
                        }

                        output.Append("</div>");

                        if (workflow.WorkCompleted.HasValue && !String.IsNullOrWhiteSpace(workflow.Result)) {
                            output.Append("  <div>" + Helpers.Text("WorkflowResult") + ":" + workflow.Result + "</div>");
                        }
                    }
                }
            }

            output.AppendLine("</div>\");");
            output.AppendLine("");

            if (_workflowRenderMode == DataObjects.WorkflowRenderMode.Editor) {
                switch (condition) {
                    case "success":
                        output.AppendLine("style " + id + " fill:" + _colorGreen + "," + inactiveStyleOpacity + "stroke:#fff,stroke-width:2px,color:#fff;");
                        output.AppendLine("");
                        break;

                    case "failure":
                        output.AppendLine("style " + id + " fill:" + _colorRed + "," + inactiveStyleOpacity + "stroke:#fff,stroke-width:2px,color:#fff;");
                        output.AppendLine("");
                        break;

                    case "complete":
                        output.AppendLine("style " + id + " fill:" + _colorBlue + "," + inactiveStyleOpacity + "stroke:#fff,stroke-width:2px,color:#fff;");
                        output.AppendLine("");
                        break;
                }
            }
        }

        return output.ToString();
    }

    /// <summary>
    /// Renders the buttons that are shown at the top-right of a workflow element.
    /// </summary>
    /// <param name="workflow">The Workflow object.</param>
    /// <param name="hideButtons">Hides some buttons.</param>
    /// <param name="orphan">Indicates if the item being rendered is in the orphans collection.</param>
    /// <returns>The string containing the buttons.</returns>
    public static string RenderWorkflowButtons(DataObjects.Workflow workflow, bool hideButtons, bool orphan)
    {
        string output = String.Empty;

        string id = workflow.WorkflowId.ToString();

        if (!hideButtons && orphan) {
            hideButtons = true;
        }

        if (_workflowRenderMode == DataObjects.WorkflowRenderMode.Editor) {
            output += "<div class='right'>";

            if (orphan || Helpers.GuidValue(workflow.ParentWorkflowId) != Guid.Empty) {
                // Not the start node
                output += Helpers.DotNetHelperCallbackButton(
                    id + "-move",
                    "",
                    new List<string> { "workflows", "button", "move", workflow.WorkflowId.ToString() },
                    "btn btn-xs btn-dark btn-move-workflow",
                    "Move",
                    Helpers.Text("WorkflowMove")
                );
            }

            if (_readOnly) {
                output += Helpers.DotNetHelperCallbackButton(
                    id + "-view",
                    Helpers.Text("View"),
                    new List<string> { "workflows", "button", "view", workflow.WorkflowId.ToString() },
                    "btn btn-xs btn-primary btn-view-workflow",
                    "View",
                    Helpers.Text("ViewWorkflow")
                );
            } else {
                output += Helpers.DotNetHelperCallbackButton(
                    id + "-copy",
                    "",
                    new List<string> { "workflows", "button", "copy", workflow.WorkflowId.ToString() },
                    "btn btn-xs btn-dark btn-copy-workflow",
                    "Copy",
                    Helpers.Text("WorkflowCopy")
                );

                if (!hideButtons) {
                    output += Helpers.DotNetHelperCallbackButton(
                        id + "-paste",
                        "",
                        new List<string> { "workflows", "button", "paste", workflow.WorkflowId.ToString() },
                        "btn btn-xs btn-dark btn-paste-workflow",
                        "Paste",
                        Helpers.Text("WorkflowPaste")
                    );
                }

                output += Helpers.DotNetHelperCallbackButton(
                    id + "-edit",
                    Helpers.Text("Edit"),
                    new List<string> { "workflows", "button", "edit", workflow.WorkflowId.ToString() },
                    "btn btn-xs btn-primary btn-edit-workflow",
                    "Edit",
                    Helpers.Text("EditWorkflow")
                );

                output += Helpers.DotNetHelperCallbackButton(
                    id + "-delete",
                    "",
                    new List<string> { "workflows", "button", "delete", workflow.WorkflowId.ToString() },
                    "btn btn-xs btn-danger btn-delete-step1",
                    "Delete",
                    Helpers.Text("Delete")
                );

                output += Helpers.DotNetHelperCallbackButton(
                    id + "-delete-confirm",
                    Helpers.Text("Confirm"),
                    new List<string> { "workflows", "button", "delete-confirm", workflow.WorkflowId.ToString() },
                    "btn btn-xs btn-danger btn-delete-confirm initially-hidden",
                    "Delete",
                    Helpers.Text("Confirm")
                );

                output += Helpers.DotNetHelperCallbackButton(
                    id + "-delete-cancel",
                    Helpers.Text("Cancel"),
                    new List<string> { "workflows", "button", "delete-cancel", workflow.WorkflowId.ToString() },
                    "btn btn-xs btn-dark btn-delete-cancel initially-hidden",
                    "Cancel",
                    Helpers.Text("Cancel")
                );
            }

            output += "</div>";
        } else if (_workflowRenderMode == DataObjects.WorkflowRenderMode.Data) {
            output += Helpers.DotNetHelperCallbackButton(
                    id + "-reprocess",
                    "",
                    new List<string> { "workflows", "button", "reprocess", workflow.WorkflowId.ToString() },
                    "btn btn-xs btn-warning margin-right reprocess-icon",
                    "Reprocess",
                    Helpers.Text("WorkflowReprocessHere")
                );
        }

        return output;
    }

    /// <summary>
    /// Renders the dependency lines between workflow items.
    /// </summary>
    /// <param name="fromWorkflow">The parent workflow object.</param>
    /// <param name="toWorkflow">The child workflow object.</param>
    /// <param name="condition">The condition: success, failure, or complete.</param>
    /// <returns>The string containing the Mermaid.js formatting for the lines.</returns>
    public static string RenderWorkflowLine(DataObjects.Workflow fromWorkflow, DataObjects.Workflow toWorkflow, string condition)
    {
        bool enabled = fromWorkflow.Enabled;
        if (enabled) {
            enabled = toWorkflow.Enabled;
        }

        string lineStyleStart = enabled ? "--" : "-.";
        string lineStyleEnd = enabled ? "-->" : ".->";

        string badgeClass = String.Empty;
        string badgeLabel = String.Empty;

        switch (condition.ToLower()) {
            case "success":
                badgeClass = "workflow-badge badge rounded-pill bg-success" + (enabled ? "" : " bg-inactive");
                badgeLabel = Helpers.Text("WorkflowOnSuccess") + "" + (enabled ? "" : " - inactive");
                break;
            case "failure":
                badgeClass = "workflow-badge badge rounded-pill bg-danger" + (enabled ? "" : " bg-inactive");
                badgeLabel = Helpers.Text("WorkflowOnFailure") + "" + (enabled ? "" : " - inactive");
                break;
            case "complete":
                badgeClass = "workflow-badge badge rounded-pill bg-primary" + (enabled ? "" : " bg-inactive");
                badgeLabel = Helpers.Text("WorkflowOnComplete") + "" + (enabled ? "" : " - inactive");
                break;
        }

        _workflowLinkCount++;

        string output = fromWorkflow.WorkflowId.ToString() + lineStyleStart +
            " <span class='" + badgeClass + "'>" + badgeLabel + "</span> " +
            lineStyleEnd + " " + toWorkflow.WorkflowId.ToString() + ";";

        return output;
    }

    /// <summary>
    /// Renders the workflows diagram in the syntax required by Mermaid.js.
    /// </summary>
    /// <param name="renderMode">The mode to render in (eg: the Editor view, the Data view, etc.)</param>
    /// <param name="readOnly">Indicates if this is a read-only view.</param>
    /// <param name="showWorkflowComplete">Option to show the completed step.</param>
    /// <returns>The syntax in the Mermaid.js syntax to render the diagram.</returns>
    public static string RenderWorkflows
    (
        List<DataObjects.Workflow>? dataWorkflowItems,
        List<DataObjects.Workflow> workflows,
        List<DataObjects.Workflow> orphans,
        DataObjects.WorkflowRenderMode renderMode = DataObjects.WorkflowRenderMode.Editor,
        bool readOnly = false,
        bool showWorkflowComplete = false,
        DataObjects.User? CurrentUser = null
    )
    {
        _currentUser = CurrentUser;
        _lastWorkflowProcessed = Guid.Empty;
        _lastWorkflowProcessedLevel = -1;
        _linkStyles = String.Empty;
        _readOnly = readOnly;
        _workflowLinkCount = -1;
        _workflowRenderMode = renderMode;

        System.Text.StringBuilder output = new System.Text.StringBuilder();

        output.AppendLine("graph TD;");
        output.AppendLine("");

        if (workflows.Any(x => x.ParentWorkflowId == null || x.ParentWorkflowId == Guid.Empty)) {
            output.Append("S((<div class='wf-start center'>");

            if (!_readOnly) {
                output.Append(Helpers.DotNetHelperCallbackButton(
                    "workflow-root-move-here",
                    "",
                    new List<string> { "workflows", "button", "move-here", "root", Guid.Empty.ToString() },
                    "btn btn-xs btn-warning btn-move-target",
                    "MoveHere",
                    Helpers.Text("WorkflowMoveHere")
                ));
            }

            output.Append(Helpers.Text("WorkflowStart"));
            output.AppendLine("</div>));");
            output.AppendLine("");

            output.AppendLine("style S fill:" + _colorGreen + ",stroke:#fff,stroke-width:2px,color:#fff,font-size:18pt;");
            output.AppendLine("");

            // Recursively render elements here. Start with the first element, which has no parent.
            var firstWorkflow = workflows.FirstOrDefault(x => x.ParentWorkflowId == null || x.ParentWorkflowId == Guid.Empty);

            if (firstWorkflow != null) {
                var renderedWorkflows = RenderWorkflowItem(dataWorkflowItems, workflows, orphans, firstWorkflow, "root", true, false, 0, false);
                output.AppendLine(renderedWorkflows);
                output.AppendLine("");
            }

            if (_workflowRenderMode == DataObjects.WorkflowRenderMode.Data && showWorkflowComplete && _lastWorkflowProcessed != Guid.Empty) {
                output.AppendLine("END(<div class='center'>" + Helpers.Text("WorkflowProcessingComplete") + "</div>)");
                output.AppendLine("");

                output.AppendLine("style END fill:" + _colorGreen + ",stroke:#000,stroke-width:2px,color:#fff;");
                output.AppendLine("");

                output.AppendLine(_lastWorkflowProcessed.ToString() + " --> END");
                output.AppendLine("");
            }
        } else {
            output.Append("S(<div class='wf-start wf-start-empty'>" + Helpers.Text("WorkflowNoWorkflows") + "<br />");

            if (!_readOnly) {
                output.Append(Helpers.DotNetHelperCallbackButton(
                    "workflow-root-move-here",
                    "",
                    new List<string> { "workflows", "button", "move-here", "root", Guid.Empty.ToString() },
                    "btn btn-xs btn-warning btn-move-target",
                    "MoveHere",
                    Helpers.Text("WorkflowMoveHere")
                ));

                output.Append(Helpers.DotNetHelperCallbackButton(
                    "workflow-root-add-workflow",
                    Helpers.Text("AddWorkflow"),
                    new List<string> { "workflows", "button", "start" },
                    "btn btn-xs btn-dark",
                    "Add",
                    Helpers.Text("AddFirstWorkflow")
                ));
            }

            output.AppendLine("</div>);");
            output.AppendLine("");

            output.AppendLine("style S fill:#777,stroke:#000,stroke-width:4px,color:#fff;");
            output.AppendLine("");
        }

        if (orphans.Any()) {
            output.AppendLine("");
            output.AppendLine("subgraph " + Helpers.Text("WorkflowOrphanedAndDeleted"));

            foreach (var orphan in orphans) {
                var renderedOrphans = RenderWorkflowItem(dataWorkflowItems, workflows, orphans, orphan, "", true, true, 0, true);
                output.AppendLine(renderedOrphans);
                output.AppendLine("");
            }

            output.AppendLine("end");
            output.AppendLine("");
        }

        if (!String.IsNullOrWhiteSpace(_linkStyles)) {
            output.AppendLine(_linkStyles);
            output.AppendLine("");
        }

        string outputString = output.ToString();

        return outputString;
    }

    public static async Task ResetWorkflowDeleteConfirmationButtons()
    {
        await Helpers.HideElementByClass("btn-delete-confirm");
        await Helpers.HideElementByClass("btn-delete-cancel");
        await Helpers.ShowElementByClass("btn-delete-step1", "inline-block");
    }

    public static async Task WorkflowsZoomIn(string elementId)
    {
        // max 4
        var zoom = Model.User.UserPreferences.WorkflowsZoom;
        zoom = zoom + .1;
        if (zoom <= 4) {
            Model.User.UserPreferences.WorkflowsZoom = zoom;
            await WorkflowsUpdatedZoom(elementId);
        }
    }

    public static async Task WorkflowsZoomOut(string elementId)
    {
        // min 0.3
        var zoom = Model.User.UserPreferences.WorkflowsZoom;
        zoom = zoom - .1;
        if (zoom >= .3) {
            Model.User.UserPreferences.WorkflowsZoom = zoom;
            await WorkflowsUpdatedZoom(elementId);
        }
    }

    public static async Task WorkflowsZoomReset(string elementId)
    {
        Model.User.UserPreferences.WorkflowsZoom = 1;

        await WorkflowsUpdatedZoom(elementId);
    }

    public static async Task WorkflowsUpdatedZoom(string elementId)
    {
        await RenderWorkflowDiagramZoom(elementId, Model.User.UserPreferences.WorkflowsZoom);
    }
}
