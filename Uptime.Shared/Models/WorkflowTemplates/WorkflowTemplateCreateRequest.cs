﻿namespace Uptime.Shared.Models.WorkflowTemplates;

public record WorkflowTemplateCreateRequest
{
    public required string TemplateName { get; init; }
    public required string WorkflowName { get; init; }
    public required string WorkflowBaseId { get; init; }
    public required int LibraryId { get; init; }
    public required string AssociationDataJson { get; init; }
}