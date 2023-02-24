using System.Collections;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.BloomFilter;

/// <summary>
/// https://eximia.co/como-bloom-filter-pode-ser-utilizada-para-melhorar-a-performance/
/// </summary>
/// <typeparam name="T"></typeparam>
public delegate int HashFunction<T>(T input);

public class BloomFilter<T>
{
    // Delegates

    // Fields
    private readonly HashFunction<T> getHashPrimary;
    private readonly HashFunction<T> getHashSecondary;
    private readonly int hashFunctionCount;
    private readonly BitArray hashBits;

    // Constructors
    public BloomFilter(
        int capacity,
        float errorRate,
        HashFunction<T> primaryHashFunction,
        HashFunction<T> secondaryHashFunction
    )
    {
        var m = BestM(capacity, errorRate);
        var k = BestK(capacity, errorRate);

        hashBits = new BitArray(m);
        hashFunctionCount = k;
        getHashPrimary = primaryHashFunction ?? throw new ArgumentNullException(nameof(secondaryHashFunction));
        getHashSecondary = secondaryHashFunction ?? throw new ArgumentNullException(nameof(secondaryHashFunction));
    }

    // Public Methods
    public void Add(T item)
    {
        if(item is null)
            throw new ArgumentNullException(nameof(item));

        // start flipping bits for each hash of item
        int primaryHash = item.GetHashCode();
        int secondaryHash = getHashSecondary(item);
        for (int i = 0; i < hashFunctionCount; i++)
        {
            int hash = ComputeHashUsingDillingerManoliosMethod(primaryHash, secondaryHash, i);
            hashBits[hash] = true;
        }
    }
    public bool MaybeContains(T item)
    {
        int primaryHash = getHashPrimary(item);
        int secondaryHash = getHashSecondary(item);
        for (int i = 0; i < hashFunctionCount; i++)
        {
            int hash = ComputeHashUsingDillingerManoliosMethod(primaryHash, secondaryHash, i);
            if (hashBits[hash] == false)
                return false;
        }
        return true;
    }

    // Private Methods
    private int ComputeHashUsingDillingerManoliosMethod(int primaryHash, int secondaryHash, int i)
    {
        var resultingHash = (primaryHash + (i * secondaryHash)) % hashBits.Count;
        return Math.Abs(resultingHash);
    }
    private static int BestM(int capacity, float errorRate)
      => (int)Math.Ceiling(capacity * Math.Log(errorRate, 1.0 / Math.Pow(2, Math.Log(2.0))));
    private static int BestK(int capacity, float errorRate)
      => (int)Math.Round(Math.Log(2.0) * BestM(capacity, errorRate) / capacity);
}
