
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using myretainers; // Ensure we can access Plugin.Log

namespace myretainers.Helpers
{
    public static class FirebaseHelper
    {
        public static async Task UploadRetainersToFirebase(FirebaseUploader uploader, Plugin plugin)
        {
            var rawTimers = plugin.GetRetainerReturnTimes();
            var timers = new List<(string Name, long ReturnTimeUnix)>();

            foreach (var t in rawTimers)
            {
                timers.Add((t.Name, t.ReturnTimeUnix));
            }

            try
            {
                await uploader.UploadRetainerTimers(timers);
                Plugin.Log.Information($"Uploaded {timers.Count} retainer timers to Firestore.");
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Failed to upload to Firestore: {ex}");
            }
        }
    }
}
