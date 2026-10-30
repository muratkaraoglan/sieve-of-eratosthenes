namespace NumberTheory;

public static class SieveOfEratosthenes
{
    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using a sequential approach.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersSequentialAlgorithm(int n)
    {
        ThrowIfInputIsNotValid(n);
        return SieveOfEratosthenesHelpers.GetBasePrimes(n);
    }

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using a modified sequential approach.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersModifiedSequentialAlgorithm(int n)
    {
        ThrowIfInputIsNotValid(n);
        return GetSecondStage(n);

        static IEnumerable<int> GetSecondStage(int m)
        {
            int limit = SieveOfEratosthenesHelpers.GetIntegerSquareRoot(m);

            var basePrimes = SieveOfEratosthenesHelpers.GetBasePrimes(limit);

            foreach (var prime in basePrimes)
            {
                yield return prime;
            }

            int low = limit + 1;
            bool[] isPrimeInSecondStage = new bool[m - limit];
            Array.Fill(isPrimeInSecondStage, true);

            foreach (int prime in basePrimes)
            {
                int firstMultiple = Math.Max(prime * prime, (limit + prime) / prime * prime);

                for (int j = firstMultiple; j <= m; j += prime)
                {
                    isPrimeInSecondStage[j - low] = false;
                }
            }

            for (int i = low; i <= m; i++)
            {
                if (isPrimeInSecondStage[i - low])
                {
                    yield return i;
                }
            }
        }
    }

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using a concurrent approach by data decomposition.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersConcurrentDataDecomposition(int n)
    {
        ThrowIfInputIsNotValid(n);
        return ConcurrentDataDecompositon(n);
    }

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using a concurrent approach by "basic" primes decomposition.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersConcurrentBasicPrimesDecomposition(int n)
    {
        ThrowIfInputIsNotValid(n);
        return ConcurrentBasicPrimesDecomposition(n);
    }

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using thread pool and signaling construct.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersConcurrentWithThreadPool(int n)
    {
        ThrowIfInputIsNotValid(n);
        return ConcurrentWithThreadPool(n);
    }

    private static IEnumerable<int> ConcurrentWithThreadPool(int n)
    {
        int limit = SieveOfEratosthenesHelpers.GetIntegerSquareRoot(n);

        List<int> basePrimes = SieveOfEratosthenesHelpers.GetBasePrimes(limit);

        bool[] isPrime = SieveOfEratosthenesHelpers.CreatePrimeFlags(n + 1);

        int workItemCount = n - limit;

        if (workItemCount > 0)
        {
            using ManualResetEventSlim completed = new(false);
            int remainingWorkItems = workItemCount;

            for (int number = limit + 1; number <= n; number++)
            {
                _ = ThreadPool.QueueUserWorkItem(
                    state =>
                    {
                        int candidate = (int)state!;

                        if (basePrimes.Any(prime => candidate % prime == 0))
                        {
                            isPrime[candidate] = false;
                        }

                        if (Interlocked.Decrement(ref remainingWorkItems) == 0)
                        {
                            completed.Set();
                        }
                    },
                    number);
            }

            completed.Wait();
        }

        return SieveOfEratosthenesHelpers.EnumeratePrimes(basePrimes, isPrime, limit + 1, n);
    }

    private static void ThrowIfInputIsNotValid(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);
    }

    private static IEnumerable<int> ConcurrentDataDecompositon(int n)
    {
        int limit = SieveOfEratosthenesHelpers.GetIntegerSquareRoot(n);

        var basePrimes = SieveOfEratosthenesHelpers.GetBasePrimes(limit);
        bool[] isPrime = new bool[n + 1];

        int processorCount = Environment.ProcessorCount;
        int rangeStart = limit + 1;
        int rangeLength = n - rangeStart + 1;
        int chunkSize = (rangeLength + processorCount - 1) / processorCount;

        _ = Parallel.For(0, processorCount, threadIndex =>
        {
            long start = rangeStart + ((long)threadIndex * chunkSize);
            long end = Math.Min(start + chunkSize - 1, n);

            for (long number = start; number <= end; number++)
            {
                bool composite = false;

                foreach (int prime in basePrimes)
                {
                    if (number % prime == 0)
                    {
                        composite = true;
                        break;
                    }
                }

                isPrime[number] = !composite;
            }
        });

        return SieveOfEratosthenesHelpers.EnumeratePrimes(basePrimes, isPrime, rangeStart, n);
    }

    private static IEnumerable<int> ConcurrentBasicPrimesDecomposition(int n)
    {
        int limit = (int)Math.Sqrt(n);

        var basePrimes = SieveOfEratosthenesHelpers.GetBasePrimes(limit);

        bool[] isPrime = SieveOfEratosthenesHelpers.CreatePrimeFlags(n + 1);

        isPrime[0] = false;

        if (n >= 1)
        {
            isPrime[1] = false;
        }

        _ = Parallel.ForEach(basePrimes, prime =>
        {
            int start = prime * prime;

            for (int number = start; number <= n; number += prime)
            {
                isPrime[number] = false;
            }
        });

        for (int i = 2; i <= n; i++)
        {
            if (isPrime[i])
            {
                yield return i;
            }
        }
    }
}
