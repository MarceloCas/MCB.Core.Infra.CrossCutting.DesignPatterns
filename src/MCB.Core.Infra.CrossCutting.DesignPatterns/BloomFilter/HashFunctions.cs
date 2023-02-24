
namespace MCB.Core.Infra.CrossCutting.DesignPatterns.BloomFilter;
public static class HashFunctions
{
    public static HashFunction<T> FromType<T>() =>
        (obj) => obj!.GetHashCode();

    public static int ForStrings(string input)
    {
        var hash = 0;

        for (var i = 0; i < input.Length; i++)
        {
            hash += input[i];
            hash += (hash << 10);
            hash ^= (hash >> 6);
        }
        hash += (hash << 3);
        hash ^= (hash >> 11);
        hash += (hash << 15);
        return hash;
    }

    public static int ForInt32(int input)
    {
        uint x = (uint)input;

        unchecked
        {
            x = ~x + (x << 15);
            x = x ^ (x >> 12);
            x = x + (x << 2);
            x = x ^ (x >> 4);
            x = x * 2057;
            x = x ^ (x >> 16);
            return (int)x;
        }
    }
}
