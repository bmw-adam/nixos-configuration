using System.Security.Cryptography;

namespace TpvVyber.Extensions;

public static class ShuffleListExtension
{
    public static void ShuffleList<T>(this IList<T> list)
    {
        RandomNumberGenerator provider = RandomNumberGenerator.Create();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
