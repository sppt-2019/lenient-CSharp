using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LenientBenchmark
{
    public class Linpack
    {
        private static Random Rnd { get; set; }

        static Linpack()
        {
            Rnd = new Random();
        }

        public static long[,] Setup(int n)
        {
            var matrix = new long[n, n];
            
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    matrix[i, j] = Rnd.NextLong();
                }
            }

            return matrix;
        }

        public static long SumSeq(long[,] matrix)
        {
            var sum = 0L;
            
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(1); j++)
                {
                    sum = unchecked(sum + matrix[i, j]);
                }
            }

            return sum;
        }

        

        public static long SumMapReduce(long[,] matrix)
        {
            var columns = Enumerable.Range(0, matrix.GetLength(0));
            return columns.Select(column =>
            {
                var sum = 0L;
                for (var i = 0; i < matrix.GetLength(1); i++) {
                    sum = unchecked(sum + matrix[column, i]);
                }
                return sum;
            }).SumUnchecked();
        }

        public static long SumParallel(long[,] matrix)
        {
            var columns  = Enumerable.Range(0, matrix.GetLength(0));
            var sums = new long[matrix.GetLength(0)];
            
            Parallel.ForEach(columns, column => {
                var sum = 0L;
                for (var i = 0; i < matrix.GetLength(1); i++) {
                    sum = unchecked(sum + matrix[column, i]);
                }
                sums[column] = sum;
            });

            return sums.SumUnchecked();
        }

        public static async Task<long> SumTask(long[,] matrix)
        {
            var columns  = Enumerable.Range(0, matrix.GetLength(0));
            var sums = columns.Select(c => Task.Run(() =>
            {
                var sum = 0L;
                for(var i = 0; i < matrix.GetLength(1); i++)
                {
                    sum = unchecked(sum + matrix[c, i]);
                }

                return sum;
            })).ToList();

            await Task.WhenAll(sums);
            return sums.SumUnchecked();
        }
    }
}