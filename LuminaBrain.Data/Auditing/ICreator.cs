namespace LuminaBrain.Data.Auditing;

public interface ICreator
{
    string? Creator { get; }

    DateTimeOffset? CreationTime { get; }
}