using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;

namespace MapIcons.Icons;
public enum MiscIconTypes
{
    Uncategorized,
    Player,
}
public class Misc_MapIcon : MapIcon {

    public MiscIconTypes MiscIconType { get; }
    public Misc_MapIcon(Entity entity, MapIconsSettings settings) : base(entity) {

        if (Entity.Type == EntityType.Player) {
            MiscIconType = MiscIconTypes.Player;
            Text = entity.GetComponent<Player>().PlayerName;
        }
        else {
            MiscIconType = MiscIconTypes.Uncategorized;
        }


        // Debug
        if (settings.Debug && settings.DebugMiscIcons && Show()) {
            Log.Write($"--| MiscIcon | EntityType:{entity.Type} | MiscIconType:{MiscIconType} | Rarity:{Rarity} | Text:{Text} | RenderName:{entity.GetComponent<Render>().Name} | Path:{entity.Path}");
        }

    }
}
