using System.Diagnostics.CodeAnalysis;

namespace OpenAuth.Domain.Shared.Interfaces;

public interface ICreate<in TIn, TOut>
{
    static abstract TOut Create(TIn value);
    static abstract bool TryCreate(TIn value, [NotNullWhen(true)] out TOut? name);
}