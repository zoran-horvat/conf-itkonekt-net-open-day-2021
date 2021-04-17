using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Demo.Primes;

namespace Demo
{
    class Program
    {
        static IPrimesGenerator Demonstrate(string label, Func<IPrimesGenerator> primesFactory)
        {
            Console.WriteLine(label);

            IPrimesGenerator generator = primesFactory();

            int start = 1_000_000;
            int step = 1_000_000;
            TimeSpan maxTime = TimeSpan.FromSeconds(10);

            for (int pass = 0; pass < 2; pass += 1)
            {
                Stopwatch simulationClock = Stopwatch.StartNew();
                Console.WriteLine();
                int max = start - step;
                while (simulationClock.Elapsed < maxTime)
                {
                    max += step;
                    Stopwatch clock = Stopwatch.StartNew();
                    int count = generator.GetAll()
                        .TakeWhile(prime => prime <= max).Count();

                    Console.WriteLine(
                        $"{max,15:#,##0} " +
                        $"{count,10:#,##0} " +
                        $"{clock.Elapsed,12:mm\\:ss\\:fff}");

                    if (pass > 0 && max >= start + 2 * step) break;
                }

                start = Math.Max(max - 2 * step, start);
            }
            Console.WriteLine(new string('-', 60));

            return generator;
        }

        static void Main(string[] args)
        {
            // Demonstrate("Naive division (Procedural)", DivisionPrimes.Create);
            // Demonstrate("Caching division-based (Object-oriented)", PrimeList.Create);
            // Demonstrate("Sieve of Eratosthenes (Object-oriented)", EratosthenesSieve.Create);
            IPrimesGenerator primes = 
                Demonstrate("Caching Sieve of Eratosthenes (Object-oriented)", CachingEratosthenesSieve.Create);
            // Demonstrate("Division-based (Functional)", PrimeNumbers.Create);
            // Demonstrate("Caching division-based (Functional)", CachingPrimeNumbers.Create);
            // Demonstrate("Parallel division-based (Functional)", ParallelPrimeNumbers.Create);

            long sumOfFirst1M = primes.GetAll()
                .Take(1_000_000)
                .Sum(x => (long)x);

            long sumBelow1M = primes.GetAll()
                .TakeWhile(prime => prime <= 1_000_000)
                .Sum(x => (long)x);

            IEnumerable<int> periodic1Mth = primes.GetAll()
                .Select((prime, index) => (prime: prime, index: index))
                .Where(tuple => (tuple.index + 1) % 1_000_000 == 0)
                .Select(tuple => tuple.prime)
                .Take(5);
            string periodic1MthReport = string.Join(" ",
                periodic1Mth.Select((prime, index) => $"{index + 1}/{prime,-11:#,##0}"));

            IEnumerable<int> groupCounts1M = primes.GetAll()
                .TakeWhile(prime => prime <= 5_000_000)
                .GroupBy(prime => prime / 1_000_000)
                .Select(group => group.Count());
            string groupCounts1MReport = string.Join(" ",
                groupCounts1M.Select((count, index) => $"{index + 1}/{count,-11:#,##0}"));

            IEnumerable<int> mersenePrimes = primes.GetAll()
                .Where(prime => (prime + 1).IsPowerOf2())
                .Take(7);
            string mersenePrimesReport = string.Join("  ",
                mersenePrimes.Select((prime, index) => $"{index + 1}/{prime:#,##0}"));

            Console.WriteLine($"  Sum of 1M primes: {sumOfFirst1M:#,##0}");
            Console.WriteLine($"      Sum below 1M: {sumBelow1M:#,##0}");
            Console.WriteLine($"  Millionth primes: {periodic1MthReport}");
            Console.WriteLine($" Count per million: {groupCounts1MReport}");
            Console.WriteLine($"    Mersene primes: {mersenePrimesReport}");
        }
    }
}
