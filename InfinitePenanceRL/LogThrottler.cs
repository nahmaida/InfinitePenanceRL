using System;
using System.Collections.Generic;
using System.IO;

namespace InfinitePenanceRL
{
    public static class LogThrottler
    {
        private static readonly Dictionary<string, DateTime> _lastLogTimes = new Dictionary<string, DateTime>();
        private static readonly TimeSpan _throttleInterval = TimeSpan.FromSeconds(1);

        public static void Log(string message, string category = "default")
        {
            try
            {
                using (StreamWriter writer = File.AppendText("game.log"))
                {
                    writer.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{category}] {message}");
                }
            }
            catch (Exception ex)
            {
                // Если не удалось записать лог — пишем через RenderComponent.Log
                try
                {
                    var rc = new InfinitePenanceRL.RenderComponent();
                    rc.GetType().GetMethod("Log", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.Invoke(rc, new object[] { $"LogThrottler ERROR: {ex.Message}" });
                }
                catch { /* если и это не сработало — просто молчим */ }
            }
        }
    }
}

