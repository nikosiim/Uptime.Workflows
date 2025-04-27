namespace Uptime.Workflows.Core.Data;

public interface IEntity
{
    public int Id { get; init; }
}

public abstract class BaseEntity : IEntity
{
    public int Id { get; init; }
}