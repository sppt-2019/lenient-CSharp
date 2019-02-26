using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LenientBenchmark
{
    public static class Extensions
    {
        public static long SumUnchecked(this IEnumerable<long> numbers)
        {
            return numbers.Aggregate((current, n) => unchecked(current + n));
        }
        
        public static long SumUnchecked(this IEnumerable<Task<long>> numbers)
        {
            return numbers.Select(t => t.Result).Aggregate((current, n) => unchecked(current + n));
        }
    }
}