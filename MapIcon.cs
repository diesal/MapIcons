using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;
using ExileCore2.Shared.Interfaces;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace MapIcons;

public enum IconRenderTypes
{ // based on render methods
    Unset,
    Monster,
    IngameIcon,
    Chest,
    Player,
}
public enum IconCategories {
    Unset,
    NPC,
    Chest,
    IngameIcon,
    Misc,
    Custom,
}
public enum IconTypes
{
    Unset,
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
    LocalPlayer,
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
    public Entity Entity { get; }
    public RectangleF DrawRect { get; set; }
    public Func<Vector2> GridPosition { get; set; }
    public MonsterRarity Rarity { get; protected set; }
    public IconPriority Priority { get; set; }
    public Func<bool> Show { get; set; }
    public Func<bool> Hidden { get; set; } = () => false;
    public HudTexture InGameTexture { get; set; }
    public string RenderName => Entity.RenderName;
    public string Text { get; set; }
    public IconRenderTypes IconRenderType { get; set; }
    public IconCategories IconCategory { get; set; }
    public IconTypes IconType { get; set; }
    // Settings
    public int DrawState { get; set; } = 0;
    public bool Draw { get; set; } = false;
    public bool DrawText { get; set; } = false;
    public int Size { get; set; } = 0;
    public int Index { get; set; } = 0;
    public Vector4 Tint { get; set; } = Vector4.Zero;
    public Vector4 HiddenTint { get; set; } = Vector4.Zero;
    public bool Check_IsAlive { get; set; } = false;
    public bool Check_IsOpened { get; set; } = false;

    public Action UpdateSettingsAction { get; set; }
    public void UpdateSettings() => UpdateSettingsAction?.Invoke();

    #endregion-------------------------------------------------------------------------------------

    public MapIcon(Entity entity)   {

        Entity = entity;
        Rarity = Entity.Rarity;
        Priority = Rarity switch
        {
            MonsterRarity.White => IconPriority.Low,
            MonsterRarity.Magic => IconPriority.Medium,
            MonsterRarity.Rare => IconPriority.High,
            MonsterRarity.Unique => IconPriority.Critical,
            _ => IconPriority.Critical
        };
        Show = () => Entity.IsValid;
        Hidden = () => entity.IsHidden;
        GridPosition = () => Entity.GridPos;

        return;
    }
}
