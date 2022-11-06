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

        return _mapper.Map(source, source.GetType(), targetType);
    }
    public object? Adapt(Type targetType, object? source, Type sourceType)
    {
        if (targetType is null || source is null || sourceType is null)
            return null;

        return _mapper.Map(source, sourceType, targetType);
    }

    public object? Adapt(Type targetType, object? source, object? existingTarget)
    {
        if (targetType is null || source is null || existingTarget is null)
            return null;

        return _mapper.Map(source, existingTarget, source.GetType(), targetType);
    }
    public object? Adapt(Type targetType, Type sourceType, object? source, object? existingTarget)
    {
        if (targetType is null || sourceType is null || source is null || existingTarget is null)
            return null;

        return _mapper.Map(source, existingTarget, sourceType, targetType);
    }

    public object? Adapt(object? source, object? target)
    {
        if (source is null || target is null)
            return null;

        return _mapper.Map(source, target, source.GetType(), target.GetType());
    }

    public TTarget? Adapt<TSource, TTarget>(TSource? source)
    {
        if (source is null)
            return default;

        return _mapper.Map<TTarget>(source);
    }
    public TTarget? Adapt<TSource, TTarget>(TSource? source, TTarget? existingTarget)
    {
        if(existingTarget is null)
            return Adapt<TSource, TTarget>(source);
        else
            return _mapper.Map(source, existingTarget);
    }

    public TTarget? Adapt<TTarget>(object? source)
    {
        if (source is null)
            return default;

        return _mapper.Map<TTarget>(source);
    }
    public TTarget? Adapt<TTarget>(object? source, TTarget? existingTarget)
    {
        if (source is null || existingTarget is null)
            return default;

        return (TTarget?)_mapper.Map(source, destination: existingTarget, sourceType: source.GetType(), destinationType: typeof(TTarget));
    }
}
