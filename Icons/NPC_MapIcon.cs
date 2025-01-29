using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapIcons.Icons;

public enum NPCTypes
{
    Monster,
    Minion,
    FracturingMirror,
    Spirit,
    NPC,
    VolatileCore
}

public class NPC_MapIcon : MapIcon
{
    public NPCTypes NPCType { get; private set; }

    public NPC_MapIcon(Entity entity, MapIconsSettings settings) : base(entity)
    {
        Show = () => entity.IsAlive;

        if (InGameTexture != null && entity.HasComponent<MinimapIcon>() && !entity.GetComponent<MinimapIcon>().Name.Equals("NPC")) return;

        // NPC
        if (entity.Type == EntityType.Npc) {
            NPCType = NPCTypes.NPC;
            var component = entity.GetComponent<Render>();
            Text = component?.Name.Split(',')[0];
            Show = () => entity.IsValid;
        }
        // Delirium
        if (entity.Path.StartsWith("Metadata/Monsters/LeagueDelirium/DoodadDaemons", StringComparison.Ordinal))
        {
            if (entity.Path.Contains("ShardPack", StringComparison.OrdinalIgnoreCase))
            {
                Priority = IconPriority.Medium;
                Hidden = () => false;
                NPCType = NPCTypes.FracturingMirror;
            }
            else
            {
                Show = () => false;
                return;
            }
        }
        //Volatile Core
        else if (entity.Path.Contains("Metadata/Monsters/LeagueDelirium/Volatile"))
        {
            Priority = IconPriority.Medium;
            NPCType = NPCTypes.VolatileCore;
        }
        // Minion
        else if (!entity.IsHostile) {
            if (!HasIngameIcon) {
                Priority = IconPriority.Low;
                NPCType = NPCTypes.Minion;
            }                
        }
        // Spirit
        else if (Rarity == MonsterRarity.Unique && entity.Path.Contains("Metadata/Monsters/Spirit/")) 
            NPCType = NPCTypes.Spirit;
        else { 
            NPCType = NPCTypes.Monster;
            //Converts on death
            if (entity.HasComponent<ObjectMagicProperties>()) {
                var objectMagicProperties = entity.GetComponent<ObjectMagicProperties>();
                var mods = objectMagicProperties.Mods;
                if (mods != null) {
                    if (mods.Contains("MonsterConvertsOnDeath_")) Show = () => entity.IsAlive && entity.IsHostile;
                }
            }
        }

        // Debug
        if (settings.Debug && settings.DebugNPC && Show()) {
            Log.Write($"--| NPC | NPCType:{NPCType} | Rarity:{Rarity} | Text:{Text} | RenderName:{entity.GetComponent<Render>().Name} | Path:{entity.Path}");               
        }
    }
}


