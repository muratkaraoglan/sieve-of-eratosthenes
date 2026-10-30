namespace NumberTheory;

internal static class SieveOfEratosthenesHelpers
{
    public static List<int> GetBasePrimes(int limit)
    {
        List<int> basePrimes = [];

        if (limit < 2)
        {
            return basePrimes;
        }

        bool[] isPrime = CreatePrimeFlags(limit);

        for (int i = 2; i * i <= limit; i++)
        {
            if (!isPrime[i])
            {
                continue;
            }

            for (int j = i * i; j <= limit; j += i)
            {
                isPrime[j] = false;
            }
        }

        for (int i = 2; i <= limit; i++)
        {
            if (isPrime[i])
            {
                basePrimes.Add(i);
            }
        }

        return basePrimes;
    }

    public static int GetIntegerSquareRoot(int n)
    {
        int root = (int)Math.Sqrt(n);

        while ((long)root * root > n)
        {
            root--;
        }

        while ((long)(root + 1) * (root + 1) <= n)
        {
            root++;
        }

        return root;
    }

    public static IEnumerable<int> EnumeratePrimes(
    List<int> basePrimes,
    bool[] isPrime,
    int rangeStart,
    int n)
    {
        foreach (int prime in basePrimes)
        {
            yield return prime;
        }

        for (int i = rangeStart; i <= n; i++)
        {
            if (isPrime[i])
            {
                yield return i;
            }
        }
    }

    public static bool[] CreatePrimeFlags(int n)
    {
        bool[] isPrime = new bool[n + 1];

        Array.Fill(isPrime, true);

        isPrime[0] = false;

        if (n >= 1)
        {
            isPrime[1] = false;
        }

        return isPrime;
    }
}
