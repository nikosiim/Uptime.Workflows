namespace Uptime.Web.Application.Definitions;

public interface IWorkflowDefinition
{
    string Id { get; set; }
    string Name { get; set; }
    string InitiationPage { get; set; }
    string AssociationDialogType { get; set; }
    Type? AssociationDialogResolved { get; }
}