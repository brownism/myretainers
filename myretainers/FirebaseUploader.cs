using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Grpc.Core;

public class FirebaseUploader {
    private readonly FirestoreDb db;

    // Track last uploaded state
    private static List<(string Name, long ReturnTimeUnix)> LastUploadedTimers = new();

    public FirebaseUploader() {
        var path = Path.Combine(myretainers.Plugin.PluginInterface.AssemblyLocation.DirectoryName!, "ffxiv-calendar-01-firebase-adminsdk-fbsvc-400753f80d.json");
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

        if (FirebaseApp.DefaultInstance == null) {
            FirebaseApp.Create(new AppOptions {
                Credential = GoogleCredential.GetApplicationDefault()
            });
        }

        db = FirestoreDb.Create("ffxiv-calendar-01");
    }

    public async Task UploadRetainerTimers(List<(string Name, long ReturnTimeUnix)> timers) {
        // Check if timers are the same as last upload
        if (Enumerable.SequenceEqual(timers, LastUploadedTimers)) {
            myretainers.Plugin.Log.Information("No changes in retainer timers; skipping upload.");
            return;
        }

        var colRef = db.Collection("retainerTimers");
        var batch = db.StartBatch();

        foreach (var retainer in timers) {
            var docRef = colRef.Document(retainer.Name);
            batch.Set(docRef, new Dictionary<string, object> {
                { "returnTime", retainer.ReturnTimeUnix },
                { "updatedAt", Timestamp.GetCurrentTimestamp() }
            });
        }

        try {
            await batch.CommitAsync();
            LastUploadedTimers = new List<(string, long)>(timers); // deep copy
            myretainers.Plugin.Log.Information($"Uploaded {timers.Count} retainer timers to Firestore.");
        } catch (RpcException ex) when (ex.StatusCode == StatusCode.ResourceExhausted) {
            myretainers.Plugin.Log.Warning("Upload skipped: Firestore quota exceeded.");
        } catch (Exception ex) {
            myretainers.Plugin.Log.Error($"Failed to upload to Firestore: {ex}");
        }
    }

    public async Task UploadRetainersToFirebase(myretainers.Plugin plugin) {
        var rawTimers = plugin.GetRetainerReturnTimes();

        var timers = new List<(string Name, long ReturnTimeUnix)>();
        foreach (var t in rawTimers) {
            timers.Add((t.Name, t.ReturnTimeUnix));
        }

        await UploadRetainerTimers(timers);
    }
}
