namespace Uptime.Application.Models.Approval;

public record ApprovalTaskPayload(string AssignedTo, string? Description, DateTime? DueDate);