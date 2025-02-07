using LuminaBrain.Data.Aggregates;

namespace LuminaBrain.Data.Auditing;

public interface IAuditEntity<out TKey> : IEntity<TKey>,ICreator,IModifier
{
}