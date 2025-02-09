using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;
using ExileCore2.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
    private List<string> UserSkippedEntityPaths { get; set; }

    private int RunCounter { get; set; }
    private int IconVersion;

    public void RebuildIcons() => IconVersion++;
    public void Initialise() => UpdateUserSkippedEntities();
    public void Tick() {
        RunCounter++;
        if (RunCounter % Settings.RunEveryXTicks != 0) return;

        foreach (var entity in _plugin.GameController.Entities) {
            if (entity.GetHudComponent<MapIcon>() is { Version: var version, } && version >= IconVersion) continue;
            MapIcon icon = CreateIcon(entity);
            if (icon == null) continue;
            icon.UpdateSettings();
            icon.Version = IconVersion;
            entity.SetHudComponent(icon);
        }
    }

    private MapIcon CreateMiscIcon(Entity entity) {
        var icon = new MapIcon(entity);
        icon.IconCategory = IconCategories.Misc;
        icon.Text = icon.RenderName;

        // Player
        if (entity.Type == EntityType.Player) {
            if (!entity.IsValid) return null;
            icon.Text = entity.GetComponent<Player>().PlayerName;
            // Local Player
            if (_plugin.GameController.IngameState.Data.LocalPlayer.Address == entity.Address) {
                //if (plugin.GameController.IngameState.Data.LocalPlayer.GetComponent<Player>().PlayerName == entity.GetComponent<Player>().PlayerName) return; 
                icon.IconRenderType = IconRenderTypes.Player;
                icon.IconType = IconTypes.LocalPlayer;
                icon.UpdateSettingsAction = () => {
                    icon.Draw = Settings.LocalPlayer_Draw;
                    icon.DrawText = Settings.LocalPlayer_DrawText;
                    icon.Size = Settings.LocalPlayer_Size;
                    icon.Index = Settings.LocalPlayer_Index;
                    icon.Tint = Settings.LocalPlayer_Tint;
                };
            }
            // Player
            else {
                icon.IconRenderType = IconRenderTypes.Player;
                icon.IconType = IconTypes.Player;
                icon.UpdateSettingsAction = () => {
                    icon.Draw = Settings.Player_Draw;
                    icon.DrawText = Settings.Player_DrawText;
                    icon.Size = Settings.Player_Size;
                    icon.Index = Settings.Player_Index;
                    icon.Tint = Settings.Player_Tint;
                };
            }
        }
        else if (entity.Path.StartsWith("Metadata/Terrain/Leagues/Sanctum/Objects/SanctumMote")) {
            icon.IconRenderType = IconRenderTypes.Chest;
            icon.IconType = IconTypes.SanctumMote;
            icon.UpdateSettingsAction = () => {
                icon.Draw = Settings.SanctumMote_Draw;
                icon.DrawText = Settings.SanctumMote_DrawText;
                icon.Size = Settings.SanctumMote_Size;
                icon.Index = Settings.SanctumMote_Index;
                icon.Tint = Settings.SanctumMote_Tint;
            };
        }
        else return null;

        DebugMiscIcon(icon);
        return icon;
    }
    private MapIcon CreateChestIcon(Entity entity) {
        var icon = new MapIcon(entity);
        icon.IconRenderType = IconRenderTypes.Chest;
        icon.IconCategory = IconCategories.Chest;
        icon.Text = icon.RenderName;
        icon.Show = () => !entity.IsOpened;

        switch (entity.Path) {
            // Breach Chests
            case string path when path.Contains("BreachChest"):
                if (entity.Path.Contains("Large")) {
                    icon.IconType = IconTypes.BreachChestLarge;
                    icon.UpdateSettingsAction = () => {
                        icon.Draw = Settings.BreachChestBoss_Draw;
                        icon.DrawText = Settings.BreachChestBoss_DrawText;
                        icon.Size = Settings.BreachChestBoss_Size;
                        icon.Index = Settings.BreachChestBoss_Index;
                        icon.Tint = Settings.BreachChestBoss_Tint;
                    };
                }
                else {
                    icon.IconType = IconTypes.BreachChest;
                    icon.UpdateSettingsAction = () => {
                        icon.Draw = Settings.BreachChest_Draw;
                        icon.DrawText = Settings.BreachChest_DrawText;
                        icon.Size = Settings.BreachChest_Size;
                        icon.Index = Settings.BreachChest_Index;
                        icon.Tint = Settings.BreachChest_Tint;
                    };                }
                break;
            // Expedition Chests
            case string path when path.StartsWith("Metadata/Chests/LeaguesExpedition/", StringComparison.Ordinal):
                if (entity.Rarity == MonsterRarity.White) {
                    icon.IconType = IconTypes.ExpeditionChestWhite;
                    icon.UpdateSettingsAction = () => {
                        icon.Draw = Settings.ExpeditionNormalChest_Draw;
                        icon.DrawText = Settings.ExpeditionNormalChest_DrawText;
                        icon.Size = Settings.ExpeditionNormalChest_Size;
                        icon.Index = Settings.ExpeditionNormalChest_Index;
                        icon.Tint = Settings.ExpeditionNormalChest_Tint;
                    };
                }
                else if (entity.Rarity == MonsterRarity.Magic) {
                    icon.IconType = IconTypes.ExpeditionChestMagic;
                    icon.UpdateSettingsAction = () => {
                        icon.Draw = Settings.ExpeditionMagicChest_Draw;
                        icon.DrawText = Settings.ExpeditionMagicChest_DrawText;
                        icon.Size = Settings.ExpeditionMagicChest_Size;
                        icon.Index = Settings.ExpeditionMagicChest_Index;
                        icon.Tint = Settings.ExpeditionMagicChest_Tint;
                    };
                }
                else if (entity.Rarity == MonsterRarity.Rare) {
                    icon.IconType = IconTypes.ExpeditionChestRare;
                    icon.UpdateSettingsAction = () => {
                        icon.Draw = Settings.ExpeditionRareChest_Draw;
                        icon.DrawText = Settings.ExpeditionRareChest_DrawText;
                        icon.Size = Settings.ExpeditionRareChest_Size;
                        icon.Index = Settings.ExpeditionRareChest_Index;
                        icon.Tint = Settings.ExpeditionRareChest_Tint;
                    };
                }
                else return null;
                break;
            // Sanctum Chests
            case string path when path.StartsWith("Metadata/Chests/LeagueSanctum/", StringComparison.Ordinal):
                icon.IconType = IconTypes.SanctumChest;
                icon.UpdateSettingsAction = () => {
                    icon.Draw = Settings.SanctumChest_Draw;
                    icon.DrawText = Settings.SanctumChest_DrawText;
                    icon.Size = Settings.SanctumChest_Size;
                    icon.Index = Settings.SanctumChest_Index;
                    icon.Tint = Settings.SanctumChest_Tint;
                };
                break;
            // Strongboxes
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/ArmourerStrongbox"):
            //    IconType = IconTypes.ArmourerStrongbox;
            //    Setting_Draw = () => Settings.ArmourerStrongbox_Draw;
            //    Setting_DrawText = () => Settings.ArmourerStrongbox_DrawText;
            //    Setting_Size = () => Settings.ArmourerStrongbox_Size;
            //    Setting_Index = () => Settings.ArmourerStrongbox_Index;
            //    Setting_Color = () => Settings.ArmourerStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/MartialStrongbox"):
            //    IconType = IconTypes.BlacksmithStrongbox;
            //    Setting_Draw = () => Settings.BlacksmithStrongbox_Draw;
            //    Setting_DrawText = () => Settings.BlacksmithStrongbox_DrawText;
            //    Setting_Size = () => Settings.BlacksmithStrongbox_Size;
            //    Setting_Index = () => Settings.BlacksmithStrongbox_Index;
            //    Setting_Color = () => Settings.BlacksmithStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Artisan"):
            //    IconType = IconTypes.ArtisanStrongbox;
            //    Setting_Draw = () => Settings.ArtisanStrongbox_Draw;
            //    Setting_DrawText = () => Settings.ArtisanStrongbox_DrawText;
            //    Setting_Size = () => Settings.ArtisanStrongbox_Size;
            //    Setting_Index = () => Settings.ArtisanStrongbox_Index;
            //    Setting_Color = () => Settings.ArtisanStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Cartographer"):
            //    IconType = IconTypes.CartographerStrongbox;
            //    Setting_Draw = () => Settings.CartographerStrongbox_Draw;
            //    Setting_DrawText = () => Settings.CartographerStrongbox_DrawText;
            //    Setting_Size = () => Settings.CartographerStrongbox_Size;
            //    Setting_Index = () => Settings.CartographerStrongbox_Index;
            //    Setting_Color = () => Settings.CartographerStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Chemist"):
            //    IconType = IconTypes.ChemistStrongbox;
            //    Setting_Draw = () => Settings.ChemistStrongbox_Draw;
            //    Setting_DrawText = () => Settings.ChemistStrongbox_DrawText;
            //    Setting_Size = () => Settings.ChemistStrongbox_Size;
            //    Setting_Index = () => Settings.ChemistStrongbox_Index;
            //    Setting_Color = () => Settings.ChemistStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Gemcutter"):
            //    IconType = IconTypes.GemcutterStrongbox;
            //    Setting_Draw = () => Settings.GemcutterStrongbox_Draw;
            //    Setting_DrawText = () => Settings.GemcutterStrongbox_DrawText;
            //    Setting_Size = () => Settings.GemcutterStrongbox_Size;
            //    Setting_Index = () => Settings.GemcutterStrongbox_Index;
            //    Setting_Color = () => Settings.GemcutterStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Jeweller"):
            //    IconType = IconTypes.JewellerStrongbox;
            //    Setting_Draw = () => Settings.JewellerStrongbox_Draw;
            //    Setting_DrawText = () => Settings.JewellerStrongbox_DrawText;
            //    Setting_Size = () => Settings.JewellerStrongbox_Size;
            //    Setting_Index = () => Settings.JewellerStrongbox_Index;
            //    Setting_Color = () => Settings.JewellerStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Large"):
            //    IconType = IconTypes.LargeStrongbox;
            //    Setting_Draw = () => Settings.LargeStrongbox_Draw;
            //    Setting_DrawText = () => Settings.LargeStrongbox_DrawText;
            //    Setting_Size = () => Settings.LargeStrongbox_Size;
            //    Setting_Index = () => Settings.LargeStrongbox_Index;
            //    Setting_Color = () => Settings.LargeStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Ornate"):
            //    IconType = IconTypes.OrnateStrongbox;
            //    Setting_Draw = () => Settings.OrnateStrongbox_Draw;
            //    Setting_DrawText = () => Settings.OrnateStrongbox_DrawText;
            //    Setting_Size = () => Settings.OrnateStrongbox_Size;
            //    Setting_Index = () => Settings.OrnateStrongbox_Index;
            //    Setting_Color = () => Settings.OrnateStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/StrongboxDivination"):
            //    IconType = IconTypes.DivinerStrongbox;
            //    Setting_Draw = () => Settings.DivinerStrongbox_Draw;
            //    Setting_DrawText = () => Settings.DivinerStrongbox_DrawText;
            //    Setting_Size = () => Settings.DivinerStrongbox_Size;
            //    Setting_Index = () => Settings.DivinerStrongbox_Index;
            //    Setting_Color = () => Settings.DivinerStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Operative"):
            //    IconType = IconTypes.OperativeStrongbox;
            //    Setting_Draw = () => Settings.OperativeStrongbox_Draw;
            //    Setting_DrawText = () => Settings.OperativeStrongbox_DrawText;
            //    Setting_Size = () => Settings.OperativeStrongbox_Size;
            //    Setting_Index = () => Settings.OperativeStrongbox_Index;
            //    Setting_Color = () => Settings.OperativeStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Arcane"):
            //    IconType = IconTypes.ArcaneStrongbox;
            //    Setting_Draw = () => Settings.ArcaneStrongbox_Draw;
            //    Setting_DrawText = () => Settings.ArcaneStrongbox_DrawText;
            //    Setting_Size = () => Settings.ArcaneStrongbox_Size;
            //    Setting_Index = () => Settings.ArcaneStrongbox_Index;
            //    Setting_Color = () => Settings.ArcaneStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Researcher"):
            //    IconType = IconTypes.ResearcherStrongbox;
            //    Setting_Draw = () => Settings.ResearcherStrongbox_Draw;
            //    Setting_DrawText = () => Settings.ResearcherStrongbox_DrawText;
            //    Setting_Size = () => Settings.ResearcherStrongbox_Size;
            //    Setting_Index = () => Settings.ResearcherStrongbox_Index;
            //    Setting_Color = () => Settings.ResearcherStrongbox_Tint;
            //    break;
            //case string path when path.StartsWith("Metadata/Chests/StrongBoxes"):
            //    IconType = IconTypes.UnknownStrongbox;
            //    Setting_Draw = () => Settings.UnknownStrongbox_Draw;
            //    Setting_DrawText = () => Settings.UnknownStrongbox_DrawText;
            //    Setting_Size = () => Settings.UnknownStrongbox_Size;
            //    Setting_Index = () => Settings.UnknownStrongbox_Index;
            //    Setting_Color = () => Settings.UnknownStrongbox_Tint;
            //    break;
            // Default
            default:
                if (entity.Rarity == MonsterRarity.White) {
                    icon.IconType = IconTypes.ChestWhite;
                    icon.UpdateSettingsAction = () => {
                        icon.Draw = Settings.NormalChest_Draw;
                        icon.DrawText = Settings.NormalChest_DrawText;
                        icon.Size = Settings.NormalChest_Size;
                        icon.Index = Settings.NormalChest_Index;
                        icon.Tint = Settings.NormalChest_Tint;
                    };
                }
                else if (entity.Rarity == MonsterRarity.Magic) {
                    icon.IconType = IconTypes.ChestMagic;
                    icon.UpdateSettingsAction = () => {
                        icon.Draw = Settings.MagicChest_Draw;
                        icon.DrawText = Settings.MagicChest_DrawText;
                        icon.Size = Settings.MagicChest_Size;
                        icon.Index = Settings.MagicChest_Index;
                        icon.Tint = Settings.MagicChest_Tint;
                    };
                }
                else if (entity.Rarity == MonsterRarity.Rare) {
                    icon.IconType = IconTypes.ChestRare;
                    icon.UpdateSettingsAction = () => {
                        icon.Draw = Settings.RareChest_Draw;
                        icon.DrawText = Settings.RareChest_DrawText;
                        icon.Size = Settings.RareChest_Size;
                        icon.Index = Settings.RareChest_Index;
                        icon.Tint = Settings.RareChest_Tint;
                    };
                }
                else if (entity.Rarity == MonsterRarity.Unique) {
                    icon.IconType = IconTypes.ChestUnique;
                    icon.UpdateSettingsAction = () => {
                        icon.Draw = Settings.UniqueChest_Draw;
                        icon.DrawText = Settings.UniqueChest_DrawText;
                        icon.Size = Settings.UniqueChest_Size;
                        icon.Index = Settings.UniqueChest_Index;
                        icon.Tint = Settings.UniqueChest_Tint;
                    };
                }
                else return null;
                break;
        }
        DebugChestIcon(icon);
        return icon;
    }
    private MapIcon CreateNPCIcon(Entity entity) {
        var icon = new MapIcon(entity);
        icon.IconRenderType = IconRenderTypes.Monster;
        icon.IconCategory = IconCategories.NPC;
        icon.Text = icon.RenderName;
        icon.Show = () => entity.IsAlive;
        
        // NPC
        if (entity.Type == EntityType.Npc) {
            if (!entity.HasComponent<Render>()) return null;
            var component = entity.GetComponent<Render>();
            icon.IconType = IconTypes.NPC;
            icon.Text = component?.Name.Split(',')[0];
            icon.Show = () => entity.IsValid;
            icon.UpdateSettingsAction = () => {
                icon.Draw = Settings.NPC_Draw;
                icon.DrawText = Settings.NPC_DrawText;
                icon.Size = Settings.NPC_Size;
                icon.Index = Settings.NPC_Index;
                icon.Tint = Settings.NPC_Tint;
                icon.HiddenTint = Settings.NPC_HiddenTint;
            };
        }
        // Delirium
        else if (entity.Path.StartsWith("Metadata/Monsters/LeagueDelirium/DoodadDaemons", StringComparison.Ordinal)) {
            if (entity.Path.Contains("ShardPack", StringComparison.OrdinalIgnoreCase)) {
                icon.Priority = IconPriority.Medium;
                icon.Hidden = () => false;
                icon.IconType = IconTypes.FracturingMirror;
                icon.UpdateSettingsAction = () => {
                    icon.Draw = Settings.FracturingMirror_Draw;
                    icon.DrawText = Settings.FracturingMirror_DrawText;
                    icon.Size = Settings.FracturingMirror_Size;
                    icon.Index = Settings.FracturingMirror_Index;
                    icon.Tint = Settings.FracturingMirror_Tint;
                    icon.HiddenTint = Settings.FracturingMirror_HiddenTint;
                };
            }
            else return null;
        }
        //Volatile Core
        else if (entity.Path.Contains("Metadata/Monsters/LeagueDelirium/Volatile")) {
            icon.Priority = IconPriority.Medium;
            icon.IconType = IconTypes.VolatileCore;
            icon.UpdateSettingsAction = () => {
                icon.Draw = Settings.VolatileCore_Draw;
                icon.DrawText = Settings.VolatileCore_DrawText;
                icon.Size = Settings.VolatileCore_Size;
                icon.Index = Settings.VolatileCore_Index;
                icon.Tint = Settings.VolatileCore_Tint;
                icon.HiddenTint = Settings.VolatileCore_HiddenTint;
            };
        }
        // Minion
        else if (!entity.IsHostile) {
            icon.Priority = IconPriority.Low;
            icon.IconType = IconTypes.Minion;
            icon.UpdateSettingsAction = () => {
                icon.Draw = Settings.Minion_Draw;
                icon.DrawText = Settings.Minion_DrawText;
                icon.Size = Settings.Minion_Size;
                icon.Index = Settings.Minion_Index;
                icon.Tint = Settings.Minion_Tint;
                icon.HiddenTint = Settings.Minion_HiddenTint;
            };
        }
        // Spirit
        else if (icon.Rarity == MonsterRarity.Unique && entity.Path.Contains("Metadata/Monsters/Spirit/")) {
            icon.IconType = IconTypes.Spirit;
            icon.UpdateSettingsAction = () => {
                icon.Draw = Settings.Spirit_Draw;
                icon.DrawText = Settings.Spirit_DrawText;
                icon.Size = Settings.Spirit_Size;
                icon.Index = Settings.Spirit_Index;
                icon.Tint = Settings.Spirit_Tint;
                icon.HiddenTint = Settings.Spirit_HiddenTint;
            };
        }
        // Monster
        else {
            // White Monster
            if (icon.Rarity == MonsterRarity.White) {
                icon.IconType = IconTypes.WhiteMonster;
                icon.UpdateSettingsAction = () => {
                    icon.Draw = Settings.WhiteMonster_Draw;
                    icon.DrawText = Settings.WhiteMonster_DrawText;
                    icon.Size = Settings.WhiteMonster_Size;
                    icon.Index = Settings.WhiteMonster_Index;
                    icon.Tint = Settings.WhiteMonster_Tint;
                    icon.HiddenTint = Settings.WhiteMonster_HiddenTint;
                };
            }
            // Magic
            else if (icon.Rarity == MonsterRarity.Magic) {
                icon.IconType = IconTypes.MagicMonster;
                icon.UpdateSettingsAction = () => {
                    icon.Draw = Settings.MagicMonster_Draw;
                    icon.DrawText = Settings.MagicMonster_DrawText;
                    icon.Size = Settings.MagicMonster_Size;
                    icon.Index = Settings.MagicMonster_Index;
                    icon.Tint = Settings.MagicMonster_Tint;
                    icon.HiddenTint = Settings.MagicMonster_HiddenTint;
                };
            }
            // Rare
            else if (icon.Rarity == MonsterRarity.Rare) {
                icon.IconType = IconTypes.RareMonster;
                icon.UpdateSettingsAction = () => {
                    icon.Draw = Settings.RareMonster_Draw;
                    icon.DrawText = Settings.RareMonster_DrawText;
                    icon.Size = Settings.RareMonster_Size;
                    icon.Index = Settings.RareMonster_Index;
                    icon.Tint = Settings.RareMonster_Tint;
                    icon.HiddenTint = Settings.RareMonster_HiddenTint;
                };
            }
            // Unique
            else if (icon.Rarity == MonsterRarity.Unique) {
                icon.IconType = IconTypes.UniqueMonster;
                icon.UpdateSettingsAction = () => {
                    icon.Draw = Settings.UniqueMonster_Draw;
                    icon.DrawText = Settings.UniqueMonster_DrawText;
                    icon.Size = Settings.UniqueMonster_Size;
                    icon.Index = Settings.UniqueMonster_Index;
                    icon.Tint = Settings.UniqueMonster_Tint;
                    icon.HiddenTint = Settings.UniqueMonster_HiddenTint;
                };
            }
            //Converts on death
            if (entity.HasComponent<ObjectMagicProperties>()) {
                var objectMagicProperties = entity.GetComponent<ObjectMagicProperties>();
                var mods = objectMagicProperties.Mods;
                if (mods != null) {
                    if (mods.Contains("MonsterConvertsOnDeath_")) icon.Show = () => entity.IsAlive && entity.IsHostile;
                }
            }
        }

        DebugNPCIcon(icon);
        return icon;
    }
    private MapIcon CreateIngameIcon(Entity entity) {
        var icon = new MapIcon(entity);
        icon.IconRenderType = IconRenderTypes.IngameIcon;
        icon.IconCategory = IconCategories.IngameIcon;
        icon.Text = icon.RenderName;

        var minimapIconComponent = entity.GetComponent<MinimapIcon>();
        if (minimapIconComponent.IsHide) return null;

        var minimapIconName = minimapIconComponent.Name;
        if (string.IsNullOrEmpty(minimapIconName)) return null;

        var iconIndexByName = Extensions.IconIndexByName(minimapIconName);
        if (iconIndexByName == MapIconsIndex.MyPlayer) return null;

        icon.InGameTexture = new HudTexture("Icons.png") { UV = SpriteHelper.GetUV(iconIndexByName), Size = 16 };
        // revisit this later and recheck logic
        var isHidden = false;
        var transitionableFlag1 = 1;
        var shrineIsAvailable = true;
        var isOpened = false;

        T Update<T>(ref T store, Func<T> update) { return entity.IsValid ? store = update() : store; }

        icon.Show = () => !Update(ref isHidden, () => entity.GetComponent<MinimapIcon>()?.IsHide ?? isHidden) &&
                           Update(ref transitionableFlag1, () => entity.GetComponent<Transitionable>()?.Flag1 ?? 1) == 1 &&
                           Update(ref shrineIsAvailable, () => entity.GetComponent<Shrine>()?.IsAvailable ?? shrineIsAvailable) &&
                          !Update(ref isOpened, () => entity.GetComponent<Chest>()?.IsOpened ?? isOpened);

        var iconSizeMultiplier = RemoteMemoryObject.TheGame.Files.MinimapIcons.EntriesList.ElementAtOrDefault((int)iconIndexByName)?.LargeMinimapSize ?? 1;
        icon.InGameTexture.Size = RemoteMemoryObject.TheGame.IngameState.IngameUi.Map.LargeMapZoom * RemoteMemoryObject.TheGame.IngameState.Camera.Height * iconSizeMultiplier / 64;

        // Shrine
        if (entity.HasComponent<Shrine>()) {
            icon.IconType = IconTypes.Shrine;
            icon.UpdateSettingsAction = () => {
                icon.DrawState = Settings.Shrine_State;
                icon.DrawText = Settings.Shrine_DrawText;
            };
        }
        // NPC
        else if (entity.Type == EntityType.Npc) {
            icon.IconType = IconTypes.IngameNPC;
            icon.UpdateSettingsAction = () => {
                icon.DrawState = Settings.IngameNPC_State;
                icon.DrawText = Settings.IngameNPC_DrawText;
            };
        }
        // QuestObject
        else if (minimapIconName == "QuestObject") {
            icon.IconType = IconTypes.QuestObject;
            icon.UpdateSettingsAction = () => {
                icon.DrawState = Settings.QuestObject_State;
                icon.DrawText = Settings.QuestObject_DrawText;
            };
        }
        else {
            switch (entity.Path) {
                case string path when path.StartsWith("Metadata/MiscellaneousObjects/Waypoint"):
                    icon.IconType = IconTypes.Waypoint;
                    icon.UpdateSettingsAction = () => {
                        icon.DrawState = Settings.Waypoint_State;
                        icon.DrawText = Settings.Waypoint_DrawText;
                    };
                    break;
                case string path when path.StartsWith("Metadata/MiscellaneousObjects/Checkpoint"):
                    icon.IconType = IconTypes.Checkpoint;
                    icon.UpdateSettingsAction = () => {
                        icon.DrawState = Settings.Checkpoint_State;
                        icon.DrawText = Settings.Checkpoint_DrawText;
                    };
                    break;
                case string path when path.StartsWith("Metadata/MiscellaneousObjects/AreaTransition"):
                    icon.IconType = IconTypes.AreaTransition;
                    icon.UpdateSettingsAction = () => {
                        icon.DrawState = Settings.AreaTransition_State;
                        icon.DrawText = Settings.AreaTransition_DrawText;
                    };
                    break;
                case string path when path.StartsWith("Metadata/Terrain/Leagues/Ritual/RitualRuneInteractable"):
                    icon.IconType = IconTypes.Ritual;
                    icon.UpdateSettingsAction = () => {
                        icon.DrawState = Settings.Ritual_State;
                        icon.DrawText = Settings.Ritual_DrawText;
                    };
                    break;
                case string path when path.StartsWith("Metadata/MiscellaneousObjects/Breach/BreachObject"):
                    icon.IconType = IconTypes.Breach;
                    icon.UpdateSettingsAction = () => {
                        icon.DrawState = Settings.Breach_State;
                        icon.DrawText = Settings.Breach_DrawText;
                    };
                    break;
                default:
                    icon.IconType = IconTypes.IngameUncategorized;
                    icon.UpdateSettingsAction = () => {
                        icon.DrawState = Settings.IngameUncategorized_State;
                        icon.DrawText = Settings.IngameUncategorized_DrawText;
                    };
                    break;
            }
        }
        DebugIngameIcon(icon);
        return icon;
    }
    private MapIcon CreateIcon(Entity entity) {
        if (entity is null) return null;
        if (string.IsNullOrEmpty(entity.Path)) return null;
        if (!entity.IsValid) return null; // only adding the icon if its valid, to make sure components are available when creating icon... 
        // custom icon by path
        var customIconSettings = Settings.CustomIconSettingsList.FirstOrDefault(setting => entity.Path.StartsWith(setting.Path, StringComparison.Ordinal));
        if (customIconSettings != null) {
            var icon = new MapIcon(entity);
            icon.IconRenderType = IconRenderTypes.Monster;
            icon.IconType = IconTypes.CustomPath;
            icon.Text = entity.RenderName;
            icon.Show = () => true;
            icon.UpdateSettingsAction = () => {
                icon.Draw = customIconSettings.Setting_Draw;
                icon.DrawText = customIconSettings.Setting_DrawText;
                icon.Size = customIconSettings.Setting_Size;
                icon.Index = customIconSettings.Setting_Index;
                icon.Tint = customIconSettings.Setting_Tint;
                icon.HiddenTint = customIconSettings.Setting_HiddenTint;
                icon.Check_IsAlive = customIconSettings.Setting_IsAlive;
                icon.Check_IsOpened = customIconSettings.Setting_IsOpened;
            };
            DebugCustomIcon(icon);
            return icon;
        }
        // skip 
        if (SkippedEntityTypes.Any(type => type == entity.Type)) return null;
        if (SkippedEntityPaths.Any(path => entity.Path?.StartsWith(path) == true)) return null;
        if (UserSkippedEntityPaths.Any(path => entity.Path?.StartsWith(path) == true)) return null;
        if (entity.Type == EntityType.WorldItem) return null; // skip world items
        if (entity.Type == EntityType.MiscellaneousObjects) return null; // skip miscellaneous objects
        // Sanctum Mote
        if (entity.Type == EntityType.Terrain) {
            if (entity.Path.StartsWith("Metadata/Terrain/Leagues/Sanctum/Objects/SanctumMote")) {
                var icon = new MapIcon(entity);
                icon.IconRenderType = IconRenderTypes.Chest;
                icon.IconCategory = IconCategories.Misc;
                icon.IconType = IconTypes.SanctumMote;
                icon.UpdateSettingsAction = () =>
                {
                    icon.Draw = Settings.SanctumMote_Draw;
                    icon.DrawText = Settings.SanctumMote_DrawText;
                    icon.Size = Settings.SanctumMote_Size;
                    icon.Index = Settings.SanctumMote_Index;
                    icon.Tint = Settings.SanctumMote_Tint;
                };
                DebugMiscIcon(icon);
                return icon;
            }
            return null; // skip most of these entity types
        }
        // Ingame Icon
        if (entity.HasComponent<MinimapIcon>()) return CreateIngameIcon(entity);
        // NPC
        if (entity.Type == EntityType.Monster || entity.Type == EntityType.Npc) {
            if (!entity.IsAlive) return null;
            return CreateNPCIcon(entity);
        }
        // Chest
        if (entity.Type == EntityType.Chest && !entity.IsOpened) return CreateChestIcon(entity);
        //Miscellaneous
        return CreateMiscIcon(entity);
    }

    private void DebugIcon(MapIcon icon) {
        if (!Settings.Debug) return;
        Log.Write($"--| IconType: {icon.IconType} | IconCategory: {icon.IconCategory} | IconRenderType: {icon.IconRenderType} | Entity.Type: {icon.Entity.Type} | RenderName: {icon.Entity.RenderName} | Path: {icon.Entity.Path}");
    }
    private void DebugIngameIcon(MapIcon icon) {
        if (Settings.DebugIngameIcon) DebugIcon(icon);
    }
    private void DebugNPCIcon(MapIcon icon) {
        if (Settings.DebugNPCIcon) DebugIcon(icon);
    }
    private void DebugMiscIcon(MapIcon icon) {
        if (Settings.DebugMiscIcon) DebugIcon(icon);
    }
    private void DebugCustomIcon(MapIcon icon) {
        if (Settings.DebugCustomIcon) DebugIcon(icon);
    }
    private void DebugChestIcon(MapIcon icon) {
        if (Settings.DebugChestIcon) DebugIcon(icon);
    }


    public void UpdateUserSkippedEntities() {
        // Split the input by lines and add to the list, ignoring lines starting with #
        UserSkippedEntityPaths = new List<string>();
        var lines = Settings.ignoredEntities.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines) {
            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#")) {
                UserSkippedEntityPaths.Add(line.Trim());
            }
        }
        RebuildIcons(); // Forced update on all icons
    }
}
