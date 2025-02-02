namespace Uptime.Web.Application.Definitions;

public class WorkflowDefinition : IWorkflowDefinition
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string InitiationPage { get; set; } = null!;
    public string AssociationDialogType { get; set; } = null!;
    public Type? AssociationDialogResolved { get; private set; }
    
    public void ResolveDialogType()
    {
        if (string.IsNullOrWhiteSpace(AssociationDialogType))
        {
            throw new Exception($"DialogType is missing for workflow {Name}.");
        }

        var type = Type.GetType(AssociationDialogType);

        AssociationDialogResolved = type ?? throw new Exception($"Invalid dialog type specified: {AssociationDialogType}");
    }
}