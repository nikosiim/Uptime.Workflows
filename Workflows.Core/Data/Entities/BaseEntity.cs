namespace Workflows.Core.Data;

public interface IEntity
{
    public int Id { get; init; }
}

/// <summary>
/// Base class for all persisted entities, with audit fields.
/// </summary>
public abstract class BaseEntity : IEntity
{
    public int Id { get; init; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public int? CreatedByPrincipalId { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public int? UpdatedByPrincipalId { get; set; }

    public DateTimeOffset? DeletedAtUtc { get; set; }
    public int? DeletedByPrincipalId { get; set; }

    public bool IsDeleted { get; set; }
    public byte[] RowVersion { get; set; }
}