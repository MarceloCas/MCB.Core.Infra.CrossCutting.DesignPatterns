using MapsterMapper;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Abstractions.Adapter;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Bechmarks")]
[assembly: InternalsVisibleTo("MCB.Core.Infra.CrossCutting.DesignPatterns.Tests")]

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
    public object Adapt(Type targetType, object source)
    {
        if (targetType is null)
            throw new ArgumentNullException(nameof(targetType));
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        return _mapper.Map(source, source.GetType(), targetType);
    }
    public object Adapt(Type targetType, object source, Type sourceType)
    {
        if (targetType is null)
            throw new ArgumentNullException(nameof(targetType));
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (sourceType is null)
            throw new ArgumentNullException(nameof(sourceType));

        return _mapper.Map(source, sourceType, targetType);
    }

    public object Adapt(Type targetType, object source, object existingTarget)
    {
        if (targetType is null)
            throw new ArgumentNullException(nameof(targetType));
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (existingTarget is null)
            throw new ArgumentNullException(nameof(existingTarget));

        return _mapper.Map(source, existingTarget, source.GetType(), targetType);
    }
    public object Adapt(Type targetType, Type sourceType, object source, object existingTarget)
    {
        if (targetType is null)
            throw new ArgumentNullException(nameof(targetType));
        if (sourceType is null)
            throw new ArgumentNullException(nameof(sourceType));
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (existingTarget is null)
            throw new ArgumentNullException(nameof(existingTarget));

        return _mapper.Map(source, existingTarget, sourceType, targetType);
    }

    public object Adapt(object source, object target)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        return _mapper.Map(source, target, source.GetType(), target.GetType());
    }

    public TTarget Adapt<TSource, TTarget>(TSource source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        return _mapper.Map<TTarget>(source);
    }
    public TTarget Adapt<TSource, TTarget>(TSource source, TTarget existingTarget)
    {
        if(existingTarget is null)
            return Adapt<TSource, TTarget>(source);
        else
            return _mapper.Map(source, existingTarget);
    }

    public TTarget Adapt<TTarget>(object source)
    {
        throw new NotImplementedException();
    }
    public TTarget Adapt<TTarget>(object source, TTarget existingTarget)
    {
        throw new NotImplementedException();
    }

}
