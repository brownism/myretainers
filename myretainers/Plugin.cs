﻿using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using myretainers.Windows;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using FFXIVClientStructs.FFXIV.Client.Game;
using System.Runtime.CompilerServices;
using System.Timers;
using myretainers.Helpers;

namespace myretainers;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/myretainers";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("myretainers");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private Timer retainerUploadTimer;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");

        PluginInterface.GetIpcProvider<List<RetainerReturn>>("RetainerTimer.GetReturnTimes")
            .RegisterFunc(GetRetainerReturnTimes);

        // Start periodic Firestore upload
        var uploader = new FirebaseUploader();
        retainerUploadTimer = new Timer(1000); // every 1 second
        retainerUploadTimer.Elapsed += async (_, _) => await uploader.UploadRetainersToFirebase(this);
        retainerUploadTimer.AutoReset = true;
        retainerUploadTimer.Enabled = true;
    }

    public void Dispose()
    {
        retainerUploadTimer?.Stop();
        retainerUploadTimer?.Dispose();
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        MainWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    public unsafe List<RetainerReturn> GetRetainerReturnTimes()
    {
        var result = new List<RetainerReturn>();
        var manager = RetainerManager.Instance();
        if (manager == null || !manager->IsReady)
            return result;

        byte count = manager->GetRetainerCount();
        for (byte i = 0; i < count; i++)
        {
            var retainer = manager->GetRetainerBySortedIndex(i);
            if (retainer == null || !retainer->Available)
                continue;

            var returnTimestamp = retainer->VentureComplete;
            if (returnTimestamp == 0)
                continue;

            byte* namePtr = (byte*)retainer + 0x08;
            var nameBytes = new Span<byte>(namePtr, 64);
            int nullIndex = nameBytes.IndexOf((byte)0);
            if (nullIndex >= 0)
                nameBytes = nameBytes.Slice(0, nullIndex);

            string name = System.Text.Encoding.UTF8.GetString(nameBytes).Trim();

            if (string.IsNullOrWhiteSpace(name))
                continue;

            Plugin.Log.Debug($"Retainer parsed: {name} | Return: {returnTimestamp}");

            result.Add(new RetainerReturn
            {
                Name = name,
                ReturnTimeUnix = returnTimestamp
            });
        }

        return result;
    }

    public struct RetainerReturn
    {
        public string Name;
        public uint ReturnTimeUnix;
    }
}
