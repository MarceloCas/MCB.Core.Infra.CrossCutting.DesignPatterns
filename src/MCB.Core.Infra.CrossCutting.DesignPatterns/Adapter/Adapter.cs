using Mapster;
using MapsterMapper;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Adapter;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.Adapter;

public class Adapter
    : IAdapter
{
    // Fields
    private readonly IMapper _mapper;

    // Constructors
    public Adapter(IMapper mapper)
    {
        _mapper = mapper;
    }

    // Public Methods
    public object? Adapt(Type targetType, object? source)
    {
        if (targetType is null || source is null)
            return null;

        return source.Adapt(sourceType: source.GetType(), destinationType: targetType);
    }
    public object? Adapt(Type targetType, object? source, Type sourceType)
    {
        if (targetType is null || source is null || sourceType is null)
            return null;

        return source.Adapt(sourceType: sourceType, destinationType: targetType);
    }

    public object? Adapt(Type targetType, object? source, object? existingTarget)
    {
        if (targetType is null || source is null || existingTarget is null)
            return null;

        return source.Adapt(destination: existingTarget, sourceType: source.GetType(), destinationType: targetType);
    }
    public object? Adapt(Type targetType, Type sourceType, object? source, object? existingTarget)
    {
        if (targetType is null || sourceType is null || source is null || existingTarget is null)
            return null;

        return source.Adapt(destination: existingTarget, sourceType, destinationType: targetType);
    }

    public object? Adapt(object? source, object? target)
    {
        if (source is null || target is null)
            return null;

        return source.Adapt(destination: target, sourceType: source.GetType(), destinationType: target.GetType());
    }

    public TTarget? Adapt<TSource, TTarget>(TSource? source)
    {
        if (source is null)
            return default;

        return (TTarget?)Adapt(targetType: typeof(TTarget), source);
    }
    public TTarget? Adapt<TSource, TTarget>(TSource? source, TTarget? existingTarget)
    {
        if (source is null)
            return default;
        else if (existingTarget is null)
            return Adapt<TSource, TTarget>(source);
        else
            return (TTarget?)source.Adapt(destination: existingTarget, sourceType: source.GetType(), destinationType: existingTarget.GetType());
    }

    public TTarget? Adapt<TTarget>(object? source)
    {
        if (source is null)
            return default;

        return (TTarget?)Adapt(targetType: typeof(TTarget), source);
    }
    public TTarget? Adapt<TTarget>(object? source, TTarget? existingTarget)
    {
        if (source is null || existingTarget is null)
            return default;

        return (TTarget?)Adapt(targetType: typeof(TTarget), source, existingTarget);
    }
}
