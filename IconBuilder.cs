using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;
using MapIcons.Icons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapIcons;

public sealed class IconBuilder
{

    private readonly MapIcons _plugin;
    public IconBuilder(MapIcons plugin) { _plugin = plugin; }
    private MapIconsSettings Settings => _plugin.Settings;

    private static EntityType[] SkippedEntityTypes =>
    [
        EntityType.HideoutDecoration,
        EntityType.Effect,
        EntityType.Light,
        EntityType.ServerObject,
        EntityType.Daemon,
        EntityType.Error,
    ];
    private static string[] SkippedEntityPaths =>
    [
        "Metadata/NPC/Hideout",
    ];

    private List<string> IgnoredEntities { get; set; }
    private int RunCounter { get; set; }
    private int IconVersion;

    public void Initialise() {
        UpdateIgnoredEntities();
    }
    public void Tick() {
        RunCounter++;
        if (RunCounter % Settings.RunEveryXTicks != 0) return;

        foreach (var entity in _plugin.GameController.Entities) {
            if (entity.GetHudComponent<MapIcon>() is { Version: var version, } && version >= IconVersion) continue;
            MapIcon icon = CreateIcon(entity);
            if (icon == null) continue;
            icon.Version = IconVersion;
            entity.SetHudComponent(icon);
        }
    }

    private MapIcon CreateIcon(Entity entity) {
        if (SkipIcon(entity)) return null;
        if (SkipPath(entity)) return null;

        var icon = new MapIcon(entity, Settings, _plugin);
        return icon;
    }


    private bool SkipPath(Entity entity) {
        return SkippedEntityPaths.Any(path => entity.Path?.StartsWith(path) == true);
    }
    private bool SkipIcon(Entity entity) {
        if (entity is not { IsValid: true }) return true;
        if (SkippedEntityTypes.Any(x => x == entity.Type)) return true;
        if (IgnoredEntities.Any(x => entity.Path?.Contains(x) == true)) return true;

        return false;
    }
    public void RebuildIcons() {
        IconVersion++;
    }
    public void UpdateIgnoredEntities() {
        // Split the input by lines and add to the list, ignoring lines starting with #
        IgnoredEntities = new List<string>();
        var lines = Settings.ignoredEntities.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines) {
            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#")) {
                IgnoredEntities.Add(line.Trim());
            }
        }
        RebuildIcons(); // Forced update on all icons
    }
}
