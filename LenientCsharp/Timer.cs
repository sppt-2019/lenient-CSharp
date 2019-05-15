using System;

namespace LenientBenchmark
{
    public class Timer
    {
        private DateTime _started;
 
        public long Check()
        {
            return DateTime.UtcNow.Subtract(_started).Ticks;
        }
 
        public void Play()
        {
            _started = DateTime.UtcNow;
        }
    }
}