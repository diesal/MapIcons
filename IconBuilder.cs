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

        //var metadata = entity.Metadata;
        //if (Settings.CustomIcons.Content
        //        .FirstOrDefault(x => _regexes.GetValue(x.MetadataRegex.Value, p => new Regex(p))!.IsMatch(metadata)) is { } customIconConfig) {
        //    return new CustomIcon(entity, Settings, customIconConfig);
        //}

        if (entity.Type == EntityType.WorldItem) {
            //if (Settings.UseReplacementsForItemIconsWhenOutOfRange &&
            //    entity.TryGetComponent<WorldItem>(out var worldItem) &&
            //    worldItem.Icon is var icon &&
            //    icon != MapIconsIndex.None) {
            //    return new IngameItemReplacerIcon(entity, Settings, icon);
            //}
            //else {
            //    return null;
            //}
            return null;
        }

        //if (entity.Type == EntityType.IngameIcon) {
            if (entity.TryGetComponent<MinimapIcon>(out var minimapIconComponent) && !minimapIconComponent.IsHide) {
                var name = minimapIconComponent.Name;
                if (!string.IsNullOrEmpty(name)) {
                    return new Ingame_MapIcon(entity, Settings);
                }
            }
        //}

        //NPC
        if (entity.Type == EntityType.Monster || entity.Type == EntityType.Npc) {
            if (!entity.IsAlive) return null;
            return new NPC_MapIcon(entity, Settings);
        }

        //Chests
        if (entity.Type == EntityType.Chest && !entity.IsOpened) return new Chest_MapIcon(entity, Settings);

        //Miscellaneous
        if (entity.Type == EntityType.Player) {
            if (!entity.IsValid ||
                _plugin.GameController.IngameState.Data.LocalPlayer.Address == entity.Address ||
                _plugin.GameController.IngameState.Data.LocalPlayer.GetComponent<Player>().PlayerName == entity.GetComponent<Player>().PlayerName) return null;

            return new Misc_MapIcon(entity, Settings);
        }
        else if (entity.HasComponent<MinimapIcon>()) {
            if (entity.HasComponent<Transitionable>()) {
                if (entity.Path.StartsWith("Metadata/MiscellaneousObjects/MissionMarker") || entity.GetComponent<MinimapIcon>().Name.Equals("MissionTarget"))
                    return new Misc_MapIcon(entity, Settings);
            }
            else if (entity.HasComponent<Targetable>() || entity.Path is "Metadata/Terrain/Leagues/Sanctum/Objects/SanctumMote")
               return new Misc_MapIcon(entity, Settings);
        }    

        return null;
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
