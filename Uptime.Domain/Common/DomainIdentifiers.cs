namespace Uptime.Domain.Common;

public readonly record struct WorkflowId(int Value)
{
    public override string ToString() => Value.ToString();
    public static explicit operator WorkflowId(int id) => new(id);
    public static explicit operator int(WorkflowId id) => id.Value;
}

public readonly record struct TaskId(int Value)
{
    public override string ToString() => Value.ToString();
    public static explicit operator TaskId(int id) => new(id);
    public static explicit operator int(TaskId id) => id.Value;
}

public readonly record struct DocumentId(int Value)
{
    public override string ToString() => Value.ToString();
    public static explicit operator DocumentId(int id) => new(id);
    public static explicit operator int(DocumentId id) => id.Value;
}

public readonly record struct WorkflowTemplateId(int Value)
{
    public override string ToString() => Value.ToString();
    public static explicit operator WorkflowTemplateId(int id) => new(id);
    public static explicit operator int(WorkflowTemplateId id) => id.Value;
}

public readonly record struct LibraryId(int Value)
{
    public override string ToString() => Value.ToString();
    public static explicit operator LibraryId(int id) => new(id);
    public static explicit operator int(LibraryId id) => id.Value;
}