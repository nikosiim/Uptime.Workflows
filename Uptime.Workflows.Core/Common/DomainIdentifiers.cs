namespace Uptime.Workflows.Core.Common;

public readonly record struct PhaseId(Guid Value)
{
    public static implicit operator Guid(PhaseId id) => id.Value;
    public static implicit operator PhaseId(Guid v) => new(v);
    public override string ToString() => Value.ToString("D");
}

public readonly record struct PrincipalId(int Value)
{
    public override string ToString() => Value.ToString();
    public static explicit operator PrincipalId(int id) => new(id);
    public static explicit operator int(PrincipalId id) => id.Value;

    public static PrincipalId Parse(string? text) 
        => DomainIdParser.Parse(text, nameof(PrincipalId), v => new PrincipalId(v));

    public static bool TryParse(string? text, out PrincipalId id)
        => DomainIdParser.TryParse(text, v => new PrincipalId(v), out id);
}

public readonly record struct PrincipalSid(string Value)
{
    public override string ToString() => Value;

    public static explicit operator PrincipalSid(string sid) => new(sid);
    public static explicit operator string(PrincipalSid sid) => sid.Value;
}

public readonly record struct WorkflowId(int Value)
{
    public override string ToString() => Value.ToString();
    public static explicit operator WorkflowId(int id) => new(id);
    public static explicit operator int(WorkflowId id) => id.Value;
    public static WorkflowId Parse(string? text)
        => DomainIdParser.Parse(text, nameof(WorkflowId), v => new WorkflowId(v));
}

public readonly record struct DocumentId(int Value)
{
    public override string ToString() => Value.ToString();
    public static explicit operator DocumentId(int id) => new(id);
    public static explicit operator int(DocumentId id) => id.Value;
    public static DocumentId Parse(string? text)
        => DomainIdParser.Parse(text, nameof(DocumentId), v => new DocumentId(v));
}

public readonly record struct WorkflowTemplateId(int Value)
{
    public override string ToString() => Value.ToString();
    public static explicit operator WorkflowTemplateId(int id) => new(id);
    public static explicit operator int(WorkflowTemplateId id) => id.Value;

    public static WorkflowTemplateId Parse(string? text)
        => DomainIdParser.Parse(text, nameof(WorkflowTemplateId), v => new WorkflowTemplateId(v));
}