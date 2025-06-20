using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Timers;
using System.Threading.Tasks;
using Dalamud.Logging;


namespace myretainers.Windows;

public class MainWindow : Window, IDisposable
{
    private string goatImagePath;
    private Plugin plugin;
    private FirebaseUploader uploader;
    private Timer retainerUploadTimer;

    public MainWindow(Plugin plugin, string goatImagePath)
        : base("My Amazing Window##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.goatImagePath = goatImagePath;
        this.plugin = plugin;

        uploader = new FirebaseUploader();
        retainerUploadTimer = new Timer(600000); // 10 minutes
        retainerUploadTimer.AutoReset = true;
        retainerUploadTimer.Enabled = true;
    }

    public void Dispose() {
        retainerUploadTimer?.Dispose();
    }

    public override void Draw()
    {
        ImGui.TextUnformatted($"Will be adding submarine timers and retainer jobs soon! {plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");

       // if (ImGui.Button("Show Settings"))
       //     plugin.ToggleConfigUI();

        ImGui.Spacing();

        using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
        {
            if (!child.Success) return;

            ImGui.TextUnformatted(":catbugnod:");
            var goatImage = Plugin.TextureProvider.GetFromFile(goatImagePath).GetWrapOrDefault();
            if (goatImage != null)
            {
                using (ImRaii.PushIndent(55f))
                {
                    ImGui.Image(goatImage.ImGuiHandle, new Vector2(goatImage.Width, goatImage.Height));
                }
            }
            else
            {
                ImGui.TextUnformatted("Image not found.");
            }

            ImGuiHelpers.ScaledDummy(20.0f);

            var localPlayer = Plugin.ClientState.LocalPlayer;
            if (localPlayer == null)
            {
                ImGui.TextUnformatted("Our local player is currently not loaded.");
                return;
            }

            if (!localPlayer.ClassJob.IsValid)
            {
                ImGui.TextUnformatted("Our current job is currently not valid.");
                return;
            }

            ImGui.TextUnformatted($"Our current job is ({localPlayer.ClassJob.RowId}) \"{localPlayer.ClassJob.Value.Abbreviation.ExtractText()}\"");

            var zoneId = Plugin.ClientState.TerritoryType;
            if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(zoneId, out var zoneInfo))
            {
                ImGui.TextUnformatted($"We are currently in ({zoneId}) \"{zoneInfo.PlaceName.Value.Name.ExtractText()}\"");
            }
            else
            {
                ImGui.TextUnformatted("Invalid territory.");
            }

            ImGui.Separator();
            ImGui.TextUnformatted("Retainer Venture Timers:");
            var timers = plugin.GetRetainerReturnTimes();

            if (timers.Count == 0)
            {
                ImGui.TextUnformatted("No active ventures found.");
            }
            else
            {
                foreach (var retainer in timers)
                {
                    var returnTime = DateTimeOffset.FromUnixTimeSeconds(retainer.ReturnTimeUnix).ToLocalTime();
                    ImGui.TextUnformatted($"{retainer.Name}: {returnTime:HH:mm:ss}");
                }
            }
        }
    }
}
