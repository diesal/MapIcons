using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;
using MapIcons.Icons;

namespace MapIcons.Icons;

public enum ChestTypes
{
    General,
    Breach,
    Strongbox,
    Expedition,
    Sanctum,
}
public enum StrongboxTypes
{
    Unknown,
    Arcanist,
    Armourer,
    Blacksmith,
    Artisan,
    Cartographer,
    Chemist,
    Gemcutter,
    Jeweller,
    Large,
    Ornate,
    Strongbox,
    Diviner,
    Operative,
    Arcane,
    Researcher
}
public enum BreachChestTypes
{
    Normal,
    Boss
}

public class Chest_MapIcon : MapIcon
{
    public ChestTypes ChestType { get; private set; }
    public StrongboxTypes StrongboxType { get; private set; }
    public BreachChestTypes BreachChestType { get; private set; }

    private static readonly Dictionary<string, StrongboxTypes> strongBoxTypeMap = new Dictionary<string, StrongboxTypes>
    {
        { "Metadata/Chests/StrongBoxes/Arcanist", StrongboxTypes.Arcanist },
        { "Metadata/Chests/StrongBoxes/ArmourerStrongbox", StrongboxTypes.Armourer },
        { "Metadata/Chests/StrongBoxes/MartialStrongbox", StrongboxTypes.Blacksmith },
        { "Metadata/Chests/StrongBoxes/Artisan", StrongboxTypes.Artisan },
        { "Metadata/Chests/StrongBoxes/Cartographer", StrongboxTypes.Cartographer },
        { "Metadata/Chests/StrongBoxes/Chemist", StrongboxTypes.Chemist },
        { "Metadata/Chests/StrongBoxes/Gemcutter", StrongboxTypes.Gemcutter },
        { "Metadata/Chests/StrongBoxes/Jeweller", StrongboxTypes.Jeweller },
        { "Metadata/Chests/StrongBoxes/Large", StrongboxTypes.Large },
        { "Metadata/Chests/StrongBoxes/Ornate", StrongboxTypes.Ornate },
        { "Metadata/Chests/StrongBoxes/StrongboxDivination", StrongboxTypes.Diviner },
        { "Metadata/Chests/StrongBoxes/Operative", StrongboxTypes.Operative },
        { "Metadata/Chests/StrongBoxes/Arcane", StrongboxTypes.Arcane },
        { "Metadata/Chests/StrongBoxes/Researcher", StrongboxTypes.Researcher },
    };

    public Chest_MapIcon(Entity entity, MapIconsSettings settings) : base(entity) {
        //categorized chest types
        if (Entity.Path.Contains("BreachChest")) {
            ChestType = ChestTypes.Breach;
            if (Entity.Path.Contains("Large"))
                BreachChestType = BreachChestTypes.Boss;                    
            else                  
                BreachChestType = BreachChestTypes.Normal;
            Text = BreachChestType.ToString();
        }
        else if (Entity.Path.Contains("Metadata/Chests/StrongBoxes")) {
            ChestType = ChestTypes.Strongbox;
            StrongboxType = StrongboxTypes.Unknown;
            foreach (var kvp in strongBoxTypeMap) {
                if (Entity.Path.StartsWith(kvp.Key)) {
                    StrongboxType = kvp.Value;
                    break;
                }
            }
            Text = StrongboxType.ToString();
        }           
        else if (Entity.Path.StartsWith("Metadata/Chests/LeaguesExpedition/", StringComparison.Ordinal)) {
            ChestType = ChestTypes.Expedition;
            Priority = IconPriority.Critical;               
        }
        else if (Entity.Path.StartsWith("Metadata/Chests/LeagueSanctum/", StringComparison.Ordinal)) {
            ChestType = ChestTypes.Sanctum;
            Priority = IconPriority.Critical;
        }
        //else if (Entity.Path.Contains("VaultTreasure"))
        //    ChestType = ChestTypes.VaultTreasure;
        //else if (Entity.Path.Contains("SideAreaChest"))
        //    ChestType = ChestTypes.SideArea;
        else
            ChestType = ChestTypes.General;

        Show = () => !Entity.IsOpened;

        if (HasIngameIcon) {
            Text = Entity.GetComponent<Render>()?.Name;
            return;
        }

        if (settings.Debug && settings.DebugChests && Show()) {
            Log.Write($"--| Chest | ChestType:{ChestType} | Rarity:{Rarity} | Text:{Text} | RenderName:{Entity.GetComponent<Render>().Name} | Path:{Entity.Path}");
            if (ChestType == ChestTypes.Strongbox) {
                Log.Write($"----| StrongBoxType:{StrongboxType}");
            }
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
}
