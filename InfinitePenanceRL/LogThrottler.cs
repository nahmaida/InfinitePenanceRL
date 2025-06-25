using System;
using System.Collections.Generic;

namespace InfinitePenanceRL
{
    public static class LogThrottler
    {
        private static readonly Dictionary<string, DateTime> _lastLogTimes = new Dictionary<string, DateTime>();
        private static readonly TimeSpan _throttleInterval = TimeSpan.FromSeconds(1);

        public static void Log(string message, string category = "default")
        {
            var now = DateTime.Now;
            
            if (_lastLogTimes.ContainsKey(category))
            {
                if (now - _lastLogTimes[category] < _throttleInterval)
                {
                    return; // Скипаем запись в лог если не прошел интервал
                }
            }

            _lastLogTimes[category] = now;
            Console.WriteLine($"[{now:HH:mm:ss}] {message}");
        }
    }
}
