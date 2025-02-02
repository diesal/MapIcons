using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;
using ExileCore2.Shared.Interfaces;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace MapIcons.Icons;

public enum IconRenderTypes
{ // based on render methods
    Unset,
    Monster,
    NPC,
    IngameIcon,
    Chest,
    Player,
}
public enum IconTypes
{
    UnCategorized,
    IngameUncategorized,

    CustomPath,

    WhiteMonster,
    MagicMonster,
    RareMonster,
    UniqueMonster,
    Minion,
    FracturingMirror,
    Spirit,
    VolatileCore,
    Shrine,
    IngameNPC,
    NPC,
    Player,
    QuestObject,
    Ritual,
    Breach,
    Waypoint,
    Checkpoint,
    AreaTransition,
    ChestWhite,
    ChestMagic,
    ChestRare,
    ChestUnique,
    BreachChest,
    BreachChestLarge,
    ExpeditionChestWhite,
    ExpeditionChestMagic,
    ExpeditionChestRare,
    SanctumChest,
    SanctumMote,
    // Added Strongbox types
    UnknownStrongbox,
    ArcanistStrongbox,
    ArmourerStrongbox,
    BlacksmithStrongbox,
    ArtisanStrongbox,
    CartographerStrongbox,
    ChemistStrongbox,
    GemcutterStrongbox,
    JewellerStrongbox,
    LargeStrongbox,
    OrnateStrongbox,
    DivinerStrongbox,
    OperativeStrongbox,
    ArcaneStrongbox,
    ResearcherStrongbox,
}



public class MapIcon
{
    #region--| Properties |-----------------------------------------------------------------------

    public int Version;
    private MapIconsSettings Settings { get; }
    public bool IsValid { get; private set; } = false;
    public Entity Entity { get; }
    public RectangleF DrawRect { get; set; }
    public Func<Vector2> GridPosition { get; set; }
    public MonsterRarity Rarity { get; protected set; }
    public IconPriority Priority { get; protected set; }
    public Func<bool> Show { get; set; }
    public Func<bool> Hidden { get; protected set; } = () => false;
    public HudTexture InGameTexture { get; protected set; }
    public string RenderName => Entity.RenderName;
    public string Text { get; protected set; }
    public IconRenderTypes IconRenderType { get; private set; }
    public IconTypes IconType { get; private set; }

    public Func<bool> Setting_Draw { get; private set; }
    public Func<bool> Setting_DrawText { get; private set; }
    public Func<int> Setting_DrawState { get; private set; }
    public Func<int> Setting_Size { get; private set; }
    public Func<int> Setting_Index { get; private set; }
    public Func<Vector4> Setting_Color { get; private set; }
    public Func<Vector4> Setting_HiddenColor { get; private set; }

    #endregion-------------------------------------------------------------------------------------

    private void DebugIngameIcon(string invalidReason = null) {
        if (!Settings.Debug || Settings.DebugIngameIcon == 0) return;

        string reason = string.IsNullOrEmpty(invalidReason) ? "valid" : $"Invalid: {invalidReason}";
        string message = $"--| IconRenderType: {IconRenderType} | IconType: {IconType} | {reason} | RenderName: {Entity.RenderName} | EntityType:{Entity.Type} | Path: {Entity.Path}";

        if (Settings.DebugIngameIcon == 1 ||
            (Settings.DebugIngameIcon == 2 && string.IsNullOrEmpty(invalidReason)) ||
            (Settings.DebugIngameIcon == 3 && !string.IsNullOrEmpty(invalidReason))) {
            Log.Write(message);
        }
    }
    private bool IsValidIngameIcon() {
        IconRenderType = IconRenderTypes.IngameIcon;
        Text = RenderName;
        var minimapIconComponent = Entity.GetComponent<MinimapIcon>();
        if (minimapIconComponent.IsHide) {
            DebugIngameIcon("Entity.MinimapIcon.IsHide");
            return false;
        }
        var minimapIconName = minimapIconComponent.Name;
        if (string.IsNullOrEmpty(minimapIconName)) {
            DebugIngameIcon("Entity.MinimapIcon.Name is null or empty");
            return false;
        }
        var iconIndexByName = Extensions.IconIndexByName(minimapIconName);
        if (iconIndexByName == MapIconsIndex.MyPlayer) {
            DebugIngameIcon("MapIconsIndex.MyPlayer");
            return false;
        }

        InGameTexture = new HudTexture("Icons.png") { UV = SpriteHelper.GetUV(iconIndexByName), Size = 16 };
        // revisit this later and recheck logic
        var isHidden = false;
        var transitionableFlag1 = 1;
        var shrineIsAvailable = true;
        var isOpened = false;

        T Update<T>(ref T store, Func<T> update) { return Entity.IsValid ? store = update() : store; }

        Show = () => !Update(ref isHidden, () => Entity.GetComponent<MinimapIcon>()?.IsHide ?? isHidden) &&
                        Update(ref transitionableFlag1, () => Entity.GetComponent<Transitionable>()?.Flag1 ?? 1) == 1 &&
                        Update(ref shrineIsAvailable, () => Entity.GetComponent<Shrine>()?.IsAvailable ?? shrineIsAvailable) &&
                        !Update(ref isOpened, () => Entity.GetComponent<Chest>()?.IsOpened ?? isOpened);

        var iconSizeMultiplier = RemoteMemoryObject.TheGame.Files.MinimapIcons.EntriesList.ElementAtOrDefault((int)iconIndexByName)?.LargeMinimapSize ?? 1;
        InGameTexture.Size = RemoteMemoryObject.TheGame.IngameState.IngameUi.Map.LargeMapZoom * RemoteMemoryObject.TheGame.IngameState.Camera.Height * iconSizeMultiplier / 64;

        // determine icon type
        if (Entity.HasComponent<Shrine>()) {
            IconType = IconTypes.Shrine;
            Setting_DrawState = () => Settings.Shrine_State;
            Setting_DrawText = () => Settings.Shrine_DrawText;
        }
        else if (Entity.Type == EntityType.Npc) {
            IconType = IconTypes.IngameNPC;
            Setting_DrawState = () => Settings.IngameNPC_State;
            Setting_DrawText = () => Settings.IngameNPC_DrawText;
        }
        else if (minimapIconName == "QuestObject") {
            IconType = IconTypes.QuestObject;
            Setting_DrawState = () => Settings.QuestObject_State;
            Setting_DrawText = () => Settings.QuestObject_DrawText;
        }
        else {
            switch (Entity.Path) {
                case string path when path.StartsWith("Metadata/MiscellaneousObjects/Waypoint"):
                    IconType = IconTypes.Waypoint;
                    Setting_DrawState = () => Settings.Waypoint_State;
                    Setting_DrawText = () => Settings.Waypoint_DrawText;
                    break;
                case string path when path.StartsWith("Metadata/MiscellaneousObjects/Checkpoint"):
                    IconType = IconTypes.Checkpoint;
                    Setting_DrawState = () => Settings.Checkpoint_State;
                    Setting_DrawText = () => Settings.Checkpoint_DrawText;
                    break;
                case string path when path.StartsWith("Metadata/MiscellaneousObjects/AreaTransition"):
                    IconType = IconTypes.AreaTransition;
                    Setting_DrawState = () => Settings.AreaTransition_State;
                    Setting_DrawText = () => Settings.AreaTransition_DrawText;
                    break;
                case string path when path.StartsWith("Metadata/Terrain/Leagues/Ritual/RitualRuneInteractable"):
                    IconType = IconTypes.Ritual;
                    Setting_DrawState = () => Settings.Ritual_State;
                    Setting_DrawText = () => Settings.Ritual_DrawText;
                    break;
                case string path when path.StartsWith("Metadata/MiscellaneousObjects/Breach/BreachObject"):
                    IconType = IconTypes.Breach;
                    Setting_DrawState = () => Settings.Breach_State;
                    Setting_DrawText = () => Settings.Breach_DrawText;
                    break;
                default:
                    IconType = IconTypes.IngameUncategorized;
                    Setting_DrawState = () => Settings.IngameUncategorized_State;
                    Setting_DrawText = () => Settings.IngameUncategorized_DrawText;
                    break;
            }
        }
        DebugIngameIcon();
        return true;
    }
    
    private void DebugNPCIcon(string invalidReason = null) {
        if (!Settings.Debug || Settings.DebugNPCIcon == 0) return;

        string reason = string.IsNullOrEmpty(invalidReason) ? "valid" : $"Invalid: {invalidReason}";
        string message = $"--| IconRenderType: {IconRenderType} | IconType: {IconType} | {reason} | RenderName: {Entity.RenderName} | EntityType:{Entity.Type} | Path: {Entity.Path}";

        if (Settings.DebugNPCIcon == 1 ||
            (Settings.DebugNPCIcon == 2 && string.IsNullOrEmpty(invalidReason)) ||
            (Settings.DebugNPCIcon == 3 && !string.IsNullOrEmpty(invalidReason))) {
            Log.Write(message);
        }
    }
    private bool IsValidNPCIcon() {
        IconRenderType = IconRenderTypes.Monster;
        Text = RenderName;
        Show = () => Entity.IsAlive;
        // NPC
        if (Entity.Type == EntityType.Npc) {
            IconType = IconTypes.NPC;
            if (!Entity.HasComponent<Render>()) {
                DebugNPCIcon("missing Render component");
                return false;
            }
            var component = Entity.GetComponent<Render>();
            Text = component?.Name.Split(',')[0];
            Show = () => Entity.IsValid;
            Setting_Draw = () => Settings.NPC_Draw;
            Setting_DrawText = () => Settings.NPC_DrawText;
            Setting_Size = () => Settings.NPC_Size;
            Setting_Index = () => Settings.NPC_Index;
            Setting_Color = () => Settings.NPC_Tint;
            Setting_HiddenColor = () => Settings.NPC_HiddenTint;
        }
        // Delirium
        else if (Entity.Path.StartsWith("Metadata/Monsters/LeagueDelirium/DoodadDaemons", StringComparison.Ordinal)) {
            if (Entity.Path.Contains("ShardPack", StringComparison.OrdinalIgnoreCase)) {
                Priority = IconPriority.Medium;
                Hidden = () => false;
                IconType = IconTypes.FracturingMirror;
                Setting_Draw = () => Settings.FracturingMirror_Draw;
                Setting_DrawText = () => Settings.FracturingMirror_DrawText;
                Setting_Size = () => Settings.FracturingMirror_Size;
                Setting_Index = () => Settings.FracturingMirror_Index;
                Setting_Color = () => Settings.FracturingMirror_Tint;
                Setting_HiddenColor = () => Settings.FracturingMirror_HiddenTint;
            }
            else {
                DebugNPCIcon("LeagueDelirium/DoodadDaemons");
                return false;
            }
        }
        //Volatile Core
        else if (Entity.Path.Contains("Metadata/Monsters/LeagueDelirium/Volatile")) {
            Priority = IconPriority.Medium;
            IconType = IconTypes.VolatileCore;
            Setting_Draw = () => Settings.VolatileCore_Draw;
            Setting_DrawText = () => Settings.VolatileCore_DrawText;
            Setting_Size = () => Settings.VolatileCore_Size;
            Setting_Index = () => Settings.VolatileCore_Index;
            Setting_Color = () => Settings.VolatileCore_Tint;
            Setting_HiddenColor = () => Settings.VolatileCore_HiddenTint;
        }
        // Minion
        else if (!Entity.IsHostile) {
            Priority = IconPriority.Low;
            IconType = IconTypes.Minion;
            Setting_Draw = () => Settings.Minion_Draw;
            Setting_DrawText = () => Settings.Minion_DrawText;
            Setting_Size = () => Settings.Minion_Size;
            Setting_Index = () => Settings.Minion_Index;
            Setting_Color = () => Settings.Minion_Tint;
            Setting_HiddenColor = () => Settings.Minion_HiddenTint;
        }
        // Spirit
        else if (Rarity == MonsterRarity.Unique && Entity.Path.Contains("Metadata/Monsters/Spirit/")) {
            IconType = IconTypes.Spirit;
            Setting_Draw = () => Settings.Spirit_Draw;
            Setting_DrawText = () => Settings.Spirit_DrawText;
            Setting_Size = () => Settings.Spirit_Size;
            Setting_Index = () => Settings.Spirit_Index;
            Setting_Color = () => Settings.Spirit_Tint;
            Setting_HiddenColor = () => Settings.Spirit_HiddenTint;
        }
        // Monster
        else {
            // White Monster
            if (Rarity == MonsterRarity.White) {
                IconType = IconTypes.WhiteMonster;
                Setting_Draw = () => Settings.WhiteMonster_Draw;
                Setting_DrawText = () => Settings.WhiteMonster_DrawText;
                Setting_Size = () => Settings.WhiteMonster_Size;
                Setting_Index = () => Settings.WhiteMonster_Index;
                Setting_Color = () => Settings.WhiteMonster_Tint;
                Setting_HiddenColor = () => Settings.WhiteMonster_HiddenTint;
            }
            // Magic
            else if (Rarity == MonsterRarity.Magic) {               
                IconType = IconTypes.MagicMonster;
                Setting_Draw = () => Settings.MagicMonster_Draw;
                Setting_DrawText = () => Settings.MagicMonster_DrawText;
                Setting_Size = () => Settings.MagicMonster_Size;
                Setting_Index = () => Settings.MagicMonster_Index;
                Setting_Color = () => Settings.MagicMonster_Tint;
                Setting_HiddenColor = () => Settings.MagicMonster_HiddenTint;
            }
            // Rare
            else if (Rarity == MonsterRarity.Rare) {
                IconType = IconTypes.RareMonster;
                Setting_Draw = () => Settings.RareMonster_Draw;
                Setting_DrawText = () => Settings.RareMonster_DrawText;
                Setting_Size = () => Settings.RareMonster_Size;
                Setting_Index = () => Settings.RareMonster_Index;
                Setting_Color = () => Settings.RareMonster_Tint;
                Setting_HiddenColor = () => Settings.RareMonster_HiddenTint;
            }
            // Unique
            else if (Rarity == MonsterRarity.Unique) { 
                IconType = IconTypes.UniqueMonster;
                Setting_Draw = () => Settings.UniqueMonster_Draw;
                Setting_DrawText = () => Settings.UniqueMonster_DrawText;
                Setting_Size = () => Settings.UniqueMonster_Size;
                Setting_Index = () => Settings.UniqueMonster_Index;
                Setting_Color = () => Settings.UniqueMonster_Tint;
                Setting_HiddenColor = () => Settings.UniqueMonster_HiddenTint;
            }
            //Converts on death
            if (Entity.HasComponent<ObjectMagicProperties>()) {
                var objectMagicProperties = Entity.GetComponent<ObjectMagicProperties>();
                var mods = objectMagicProperties.Mods;
                if (mods != null) { 
                    if (mods.Contains("MonsterConvertsOnDeath_")) Show = () => Entity.IsAlive && Entity.IsHostile;
                }
            }
        }

        DebugNPCIcon();
        return true;
    }

    private void DebugChestIcon(string invalidReason = null) {
        if (!Settings.Debug || Settings.DebugChestIcon == 0) return;

        string reason = string.IsNullOrEmpty(invalidReason) ? "valid" : $"Invalid: {invalidReason}";
        string message = $"--| IconRenderType: {IconRenderType} | IconType: {IconType} | {reason} | RenderName: {Entity.RenderName} | EntityType:{Entity.Type} | Path: {Entity.Path}";

        if (Settings.DebugChestIcon == 1 ||
            (Settings.DebugChestIcon == 2 && string.IsNullOrEmpty(invalidReason)) ||
            (Settings.DebugChestIcon == 3 && !string.IsNullOrEmpty(invalidReason))) {
            Log.Write(message);
            if (Entity.GetComponent<Stats>()?.StatDictionary != null) {
                foreach (var i in Entity.GetComponent<Stats>().StatDictionary) {
                    Log.Write($"----| Stat: {i.Key} = {i.Value}");
                }
            }

            if (Entity.GetComponent<ObjectMagicProperties>() != null) {
                foreach (var mod in Entity.GetComponent<ObjectMagicProperties>().Mods) {
                    Log.Write($"----| Mods: {mod}");
                }
            }

        }
    }
    private bool IsValidChestIcon() {
        IconRenderType = IconRenderTypes.Chest;
        Text = RenderName;
        Show = () => !Entity.IsOpened;

        switch (Entity.Path) {
            // Breach Chests
            case string path when path.Contains("BreachChest"):
                if (Entity.Path.Contains("Large")) {
                    IconType = IconTypes.BreachChestLarge;
                    Setting_Draw = () => Settings.BreachChestBoss_Draw;
                    Setting_DrawText = () => Settings.BreachChestBoss_DrawText;
                    Setting_Size = () => Settings.BreachChestBoss_Size;
                    Setting_Index = () => Settings.BreachChestBoss_Index;
                    Setting_Color = () => Settings.BreachChestBoss_Tint;
                }
                else {
                    IconType = IconTypes.BreachChest;
                    Setting_Draw = () => Settings.BreachChest_Draw;
                    Setting_DrawText = () => Settings.BreachChest_DrawText;
                    Setting_Size = () => Settings.BreachChest_Size;
                    Setting_Index = () => Settings.BreachChest_Index;
                    Setting_Color = () => Settings.BreachChest_Tint;
                }
                break;
            // Expedition Chests
            case string path when path.StartsWith("Metadata/Chests/LeaguesExpedition/", StringComparison.Ordinal):
                if (Entity.Rarity == MonsterRarity.White) {
                    IconType = IconTypes.ExpeditionChestWhite;
                    Setting_Draw = () => Settings.ExpeditionNormalChest_Draw;
                    Setting_DrawText = () => Settings.ExpeditionNormalChest_DrawText;
                    Setting_Size = () => Settings.ExpeditionNormalChest_Size;
                    Setting_Index = () => Settings.ExpeditionNormalChest_Index;
                    Setting_Color = () => Settings.ExpeditionNormalChest_Tint;
                }
                else if (Entity.Rarity == MonsterRarity.Magic) {
                    IconType = IconTypes.ExpeditionChestMagic;
                    Setting_Draw = () => Settings.ExpeditionMagicChest_Draw;
                    Setting_DrawText = () => Settings.ExpeditionMagicChest_DrawText;
                    Setting_Size = () => Settings.ExpeditionMagicChest_Size;
                    Setting_Index = () => Settings.ExpeditionMagicChest_Index;
                    Setting_Color = () => Settings.ExpeditionMagicChest_Tint;
                }
                else if (Entity.Rarity == MonsterRarity.Rare) {
                    IconType = IconTypes.ExpeditionChestRare;
                    Setting_Draw = () => Settings.ExpeditionRareChest_Draw;
                    Setting_DrawText = () => Settings.ExpeditionRareChest_DrawText;
                    Setting_Size = () => Settings.ExpeditionRareChest_Size;
                    Setting_Index = () => Settings.ExpeditionRareChest_Index;
                    Setting_Color = () => Settings.ExpeditionRareChest_Tint;
                }
                else {
                    DebugChestIcon("unkown chest");
                    return false;
                }
                break;
            // Sanctum Chests
            case string path when path.StartsWith("Metadata/Chests/LeagueSanctum/", StringComparison.Ordinal):
                IconType = IconTypes.SanctumChest;
                Setting_Draw = () => Settings.SanctumChest_Draw;
                Setting_DrawText = () => Settings.SanctumChest_DrawText;
                Setting_Size = () => Settings.SanctumChest_Size;
                Setting_Index = () => Settings.SanctumChest_Index;
                Setting_Color = () => Settings.SanctumChest_Tint;
                break;
            // Strongboxes
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/ArmourerStrongbox"):
                IconType = IconTypes.ArmourerStrongbox;
                Setting_Draw = () => Settings.ArmourerStrongbox_Draw;
                Setting_DrawText = () => Settings.ArmourerStrongbox_DrawText;
                Setting_Size = () => Settings.ArmourerStrongbox_Size;
                Setting_Index = () => Settings.ArmourerStrongbox_Index;
                Setting_Color = () => Settings.ArmourerStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/MartialStrongbox"):
                IconType = IconTypes.BlacksmithStrongbox;
                Setting_Draw = () => Settings.BlacksmithStrongbox_Draw;
                Setting_DrawText = () => Settings.BlacksmithStrongbox_DrawText;
                Setting_Size = () => Settings.BlacksmithStrongbox_Size;
                Setting_Index = () => Settings.BlacksmithStrongbox_Index;
                Setting_Color = () => Settings.BlacksmithStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Artisan"):
                IconType = IconTypes.ArtisanStrongbox;
                Setting_Draw = () => Settings.ArtisanStrongbox_Draw;
                Setting_DrawText = () => Settings.ArtisanStrongbox_DrawText;
                Setting_Size = () => Settings.ArtisanStrongbox_Size;
                Setting_Index = () => Settings.ArtisanStrongbox_Index;
                Setting_Color = () => Settings.ArtisanStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Cartographer"):
                IconType = IconTypes.CartographerStrongbox;
                Setting_Draw = () => Settings.CartographerStrongbox_Draw;
                Setting_DrawText = () => Settings.CartographerStrongbox_DrawText;
                Setting_Size = () => Settings.CartographerStrongbox_Size;
                Setting_Index = () => Settings.CartographerStrongbox_Index;
                Setting_Color = () => Settings.CartographerStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Chemist"):
                IconType = IconTypes.ChemistStrongbox;
                Setting_Draw = () => Settings.ChemistStrongbox_Draw;
                Setting_DrawText = () => Settings.ChemistStrongbox_DrawText;
                Setting_Size = () => Settings.ChemistStrongbox_Size;
                Setting_Index = () => Settings.ChemistStrongbox_Index;
                Setting_Color = () => Settings.ChemistStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Gemcutter"):
                IconType = IconTypes.GemcutterStrongbox;
                Setting_Draw = () => Settings.GemcutterStrongbox_Draw;
                Setting_DrawText = () => Settings.GemcutterStrongbox_DrawText;
                Setting_Size = () => Settings.GemcutterStrongbox_Size;
                Setting_Index = () => Settings.GemcutterStrongbox_Index;
                Setting_Color = () => Settings.GemcutterStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Jeweller"):
                IconType = IconTypes.JewellerStrongbox;
                Setting_Draw = () => Settings.JewellerStrongbox_Draw;
                Setting_DrawText = () => Settings.JewellerStrongbox_DrawText;
                Setting_Size = () => Settings.JewellerStrongbox_Size;
                Setting_Index = () => Settings.JewellerStrongbox_Index;
                Setting_Color = () => Settings.JewellerStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Large"):
                IconType = IconTypes.LargeStrongbox;
                Setting_Draw = () => Settings.LargeStrongbox_Draw;
                Setting_DrawText = () => Settings.LargeStrongbox_DrawText;
                Setting_Size = () => Settings.LargeStrongbox_Size;
                Setting_Index = () => Settings.LargeStrongbox_Index;
                Setting_Color = () => Settings.LargeStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Ornate"):
                IconType = IconTypes.OrnateStrongbox;
                Setting_Draw = () => Settings.OrnateStrongbox_Draw;
                Setting_DrawText = () => Settings.OrnateStrongbox_DrawText;
                Setting_Size = () => Settings.OrnateStrongbox_Size;
                Setting_Index = () => Settings.OrnateStrongbox_Index;
                Setting_Color = () => Settings.OrnateStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/StrongboxDivination"):
                IconType = IconTypes.DivinerStrongbox;
                Setting_Draw = () => Settings.DivinerStrongbox_Draw;
                Setting_DrawText = () => Settings.DivinerStrongbox_DrawText;
                Setting_Size = () => Settings.DivinerStrongbox_Size;
                Setting_Index = () => Settings.DivinerStrongbox_Index;
                Setting_Color = () => Settings.DivinerStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Operative"):
                IconType = IconTypes.OperativeStrongbox;
                Setting_Draw = () => Settings.OperativeStrongbox_Draw;
                Setting_DrawText = () => Settings.OperativeStrongbox_DrawText;
                Setting_Size = () => Settings.OperativeStrongbox_Size;
                Setting_Index = () => Settings.OperativeStrongbox_Index;
                Setting_Color = () => Settings.OperativeStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Arcane"):
                IconType = IconTypes.ArcaneStrongbox;
                Setting_Draw = () => Settings.ArcaneStrongbox_Draw;
                Setting_DrawText = () => Settings.ArcaneStrongbox_DrawText;
                Setting_Size = () => Settings.ArcaneStrongbox_Size;
                Setting_Index = () => Settings.ArcaneStrongbox_Index;
                Setting_Color = () => Settings.ArcaneStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes/Researcher"):
                IconType = IconTypes.ResearcherStrongbox;
                Setting_Draw = () => Settings.ResearcherStrongbox_Draw;
                Setting_DrawText = () => Settings.ResearcherStrongbox_DrawText;
                Setting_Size = () => Settings.ResearcherStrongbox_Size;
                Setting_Index = () => Settings.ResearcherStrongbox_Index;
                Setting_Color = () => Settings.ResearcherStrongbox_Tint;
                break;
            case string path when path.StartsWith("Metadata/Chests/StrongBoxes"):
                IconType = IconTypes.UnknownStrongbox;
                Setting_Draw = () => Settings.UnknownStrongbox_Draw;
                Setting_DrawText = () => Settings.UnknownStrongbox_DrawText;
                Setting_Size = () => Settings.UnknownStrongbox_Size;
                Setting_Index = () => Settings.UnknownStrongbox_Index;
                Setting_Color = () => Settings.UnknownStrongbox_Tint;
                break;
            // Default
            default:
                if (Entity.Rarity == MonsterRarity.White) {
                    IconType = IconTypes.ChestWhite;
                    Setting_Draw = () => Settings.NormalChest_Draw;
                    Setting_DrawText = () => Settings.NormalChest_DrawText;
                    Setting_Size = () => Settings.NormalChest_Size;
                    Setting_Index = () => Settings.NormalChest_Index;
                    Setting_Color = () => Settings.NormalChest_Tint;
                }
                else if (Entity.Rarity == MonsterRarity.Magic) {
                    IconType = IconTypes.ChestMagic;
                    Setting_Draw = () => Settings.MagicChest_Draw;
                    Setting_DrawText = () => Settings.MagicChest_DrawText;
                    Setting_Size = () => Settings.MagicChest_Size;
                    Setting_Index = () => Settings.MagicChest_Index;
                    Setting_Color = () => Settings.MagicChest_Tint;
                }
                else if (Entity.Rarity == MonsterRarity.Rare) {
                    IconType = IconTypes.ChestRare;
                    Setting_Draw = () => Settings.RareChest_Draw;
                    Setting_DrawText = () => Settings.RareChest_DrawText;
                    Setting_Size = () => Settings.RareChest_Size;
                    Setting_Index = () => Settings.RareChest_Index;
                    Setting_Color = () => Settings.RareChest_Tint;
                }
                else if (Entity.Rarity == MonsterRarity.Unique) {
                    IconType = IconTypes.ChestUnique;
                    Setting_Draw = () => Settings.UniqueChest_Draw;
                    Setting_DrawText = () => Settings.UniqueChest_DrawText;
                    Setting_Size = () => Settings.UniqueChest_Size;
                    Setting_Index = () => Settings.UniqueChest_Index;
                    Setting_Color = () => Settings.UniqueChest_Tint;
                }
                else {
                    DebugChestIcon("Unknown Chest");
                    return false;
                }
                break;
        }

        DebugChestIcon();
        return true;
    }

    private void DebugMiscIcon(string invalidReason = null) {
        if (!Settings.Debug || Settings.DebugMiscIcon == 0) return;

        string reason = string.IsNullOrEmpty(invalidReason) ? "valid" : $"Invalid: {invalidReason}";
        string message = $"--| IconRenderType: {IconRenderType} | IconType: {IconType} | {reason} | Entity.Type: {Entity.Type} | RenderName: {Entity.RenderName} | Path: {Entity.Path}";

        if (Settings.DebugMiscIcon == 1 ||
            (Settings.DebugMiscIcon == 2 && string.IsNullOrEmpty(invalidReason)) ||
            (Settings.DebugMiscIcon == 3 && !string.IsNullOrEmpty(invalidReason))) {
            Log.Write(message);
        }
    }
    private void DebugUnhandled() {
        if (!Settings.Debug || !Settings.DebugUnhandled) return;
        Log.Write($"--| Unhandled entity | Entity.Type: {Entity.Type} | Entity.Rarity: {Entity.Rarity} | RenderName: {Entity.RenderName} | Path: {Entity.Path} ");
    }
    private bool IsValidMiscIcon(MapIcons plugin) {      

        // Player
        if (Entity.Type == EntityType.Player) {
            IconRenderType = IconRenderTypes.Player;
            IconType = IconTypes.Player;
            if (!Entity.IsValid ) {
                DebugMiscIcon("invalid");
                return false;
            }
            Text = Entity.GetComponent<Player>().PlayerName;
            Setting_Draw = () => Settings.Player_Draw;
            Setting_DrawText = () => Settings.Player_DrawText;
            Setting_Size = () => Settings.Player_Size;
            Setting_Index = () => Settings.Player_Index;
            Setting_Color = () => Settings.Player_Tint;
        }
        else if (Entity.Path.StartsWith("Metadata/Terrain/Leagues/Sanctum/Objects/SanctumMote")) {
            IconRenderType = IconRenderTypes.Chest;
            IconType = IconTypes.SanctumMote;
            Setting_Draw = () => Settings.SanctumMote_Draw;
            Setting_DrawText = () => Settings.SanctumMote_DrawText;
            Setting_Size = () => Settings.SanctumMote_Size;
            Setting_Index = () => Settings.SanctumMote_Index;
            Setting_Color = () => Settings.SanctumMote_Tint;
        }
        else {
            DebugUnhandled();
            return false;
        }

        DebugMiscIcon();
        return true;
    }
    private void DebugCustomIcon() {
        if (!Settings.Debug) return;
        Log.Write($"--| Custom Icon | Entity.Type: {Entity.Type} | Entity.Rarity: {Entity.Rarity} | RenderName: {Entity.RenderName} | Path: {Entity.Path} ");
    }

    public MapIcon(Entity entity, MapIconsSettings settings, MapIcons plugin) {
        if (entity == null) return;
        if (plugin.GameController.IngameState.Data.LocalPlayer.Address == entity.Address) return; // skip local player
        //if (plugin.GameController.IngameState.Data.LocalPlayer.GetComponent<Player>().PlayerName == entity.GetComponent<Player>().PlayerName) return; 

        // Initialise MapIcon
        Settings = settings;
        Entity = entity;
        Rarity = Entity.Rarity;
        Priority = Rarity switch {
            MonsterRarity.White => IconPriority.Low,
            MonsterRarity.Magic => IconPriority.Medium,
            MonsterRarity.Rare => IconPriority.High,
            MonsterRarity.Unique => IconPriority.Critical,
            _ => IconPriority.Critical
        };
        Show = () => Entity.IsValid;
        Hidden = () => entity.IsHidden;
        GridPosition = () => Entity.GridPos;

        // custom icons by path
        var customIconSetting = Settings.CustomIconSettingsList.FirstOrDefault(setting => Entity.Path.StartsWith(setting.Path, StringComparison.Ordinal));
        if (customIconSetting != null) {
            IconRenderType = IconRenderTypes.Monster;
            IconType = IconTypes.CustomPath;
            Show = () => true;
            Setting_Draw = () => customIconSetting.Setting_Draw;
            Setting_DrawText = () => customIconSetting.Setting_DrawText;
            Setting_Size = () => customIconSetting.Setting_Size;
            Setting_Index = () => customIconSetting.Setting_Index;
            Setting_Color = () => customIconSetting.Setting_Tint;
            Setting_HiddenColor = () => customIconSetting.Setting_HiddenTint;
            IsValid = true;
            DebugCustomIcon();
            return;
        }
        //var metadata = entity.Metadata;
        //if (Settings.CustomIcons.Content
        //        .FirstOrDefault(x => _regexes.GetValue(x.MetadataRegex.Value, p => new Regex(p))!.IsMatch(metadata)) is { } customIconConfig) {
        //    return new CustomIcon(entity, Settings, customIconConfig);
        //}

        // WorldItem, MiscellaneousObjects, Terrain
        if (entity.Type == EntityType.WorldItem || entity.Type == EntityType.MiscellaneousObjects || entity.Type == EntityType.Terrain) {
            if (entity.HasComponent<Targetable>()) IsValid = IsValidMiscIcon(plugin);
            return; // skip most of these entity types
        }  

        // Ingame Icon
        if (entity.HasComponent<MinimapIcon>()) {
            IsValid = IsValidIngameIcon();
            return;
        }
        //NPC
        if (entity.Type == EntityType.Monster || entity.Type == EntityType.Npc) {
            if (!entity.IsAlive) {
                IsValid = false;
                return;
            }
            IsValid = IsValidNPCIcon();
            return;
        }
        //Chests
        if (entity.Type == EntityType.Chest && !entity.IsOpened) {
            IsValid = IsValidChestIcon();
            return;
        }
        //Miscellaneous
        IsValid = IsValidMiscIcon(plugin);
        return;

        //else if (entity.HasComponent<MinimapIcon>()) {
        //    if (entity.HasComponent<Transitionable>()) {
        //        if (entity.Path.StartsWith("Metadata/MiscellaneousObjects/MissionMarker") || entity.GetComponent<MinimapIcon>().Name.Equals("MissionTarget"))
        //            return new Misc_MapIcon(entity, Settings);
        //    }
        //    else if (entity.HasComponent<Targetable>() || entity.Path is "Metadata/Terrain/Leagues/Sanctum/Objects/SanctumMote")
        //       return new Misc_MapIcon(entity, Settings);
        //}   
    }
}
