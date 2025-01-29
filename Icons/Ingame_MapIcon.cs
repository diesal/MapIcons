using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapIcons.Icons;
public enum IngameIconTypes
{
    Uncategorized,

    AreaTransition,
    Breach,
    Checkpoint,
    QuestObject,
    Ritual,
    Shrine,
    Waypoint,
    NPC,
    NPCHideout
}
public class Ingame_MapIcon : MapIcon
{
    public IngameIconTypes IngameIconType { get; }

    private static readonly Dictionary<string, IngameIconTypes> NameDictionary = new()
    {
        { "QuestObject", IngameIconTypes.QuestObject }
    };
    private static readonly List<KeyValuePair<string, IngameIconTypes>> PathList = new()
    {
        new KeyValuePair<string, IngameIconTypes>("Metadata/MiscellaneousObjects/Waypoint", IngameIconTypes.Waypoint),
        new KeyValuePair<string, IngameIconTypes>("Metadata/MiscellaneousObjects/Checkpoint", IngameIconTypes.Checkpoint),
        new KeyValuePair<string, IngameIconTypes>("Metadata/MiscellaneousObjects/AreaTransition", IngameIconTypes.AreaTransition),
        new KeyValuePair<string, IngameIconTypes>("Metadata/Terrain/Leagues/Ritual/RitualRuneInteractable", IngameIconTypes.Ritual),
        new KeyValuePair<string, IngameIconTypes>("Metadata/MiscellaneousObjects/Breach/BreachObject", IngameIconTypes.Breach)
    };


    public Ingame_MapIcon(Entity entity, MapIconsSettings settings) : base(entity)
    {
        var isHidden = false;
        var transitionableFlag1 = 1;
        var shrineIsAvailable = true;
        var isOpened = false;

        T Update<T>(ref T store, Func<T> update) { return entity.IsValid ? store = update() : store; }

        Show = () => !Update(ref isHidden, () => entity.GetComponent<MinimapIcon>()?.IsHide ?? isHidden) &&
                      Update(ref transitionableFlag1, () => entity.GetComponent<Transitionable>()?.Flag1 ?? 1) == 1 &&
                      Update(ref shrineIsAvailable, () => entity.GetComponent<Shrine>()?.IsAvailable ?? shrineIsAvailable) &&
                     !Update(ref isOpened, () => entity.GetComponent<Chest>()?.IsOpened ?? isOpened);

        var name = entity.GetComponent<MinimapIcon>()?.Name ?? "";
        var iconIndexByName = ExileCore2.Shared.Helpers.Extensions.IconIndexByName(name);
        var iconSizeMultiplier = RemoteMemoryObject.TheGame.Files.MinimapIcons.EntriesList.ElementAtOrDefault((int)iconIndexByName)?.LargeMinimapSize ?? 1;
        InGameTexture.Size = RemoteMemoryObject.TheGame.IngameState.IngameUi.Map.LargeMapZoom * RemoteMemoryObject.TheGame.IngameState.Camera.Height * iconSizeMultiplier / 64;



        if (entity.HasComponent<Shrine>()) {
            IngameIconType = IngameIconTypes.Shrine;
            Text = RenderName;
        }
        else if (entity.Type == EntityType.Npc) {
            if (entity.Path.StartsWith("Metadata/NPC/Hideout/"))
                IngameIconType = IngameIconTypes.NPCHideout;
            else                
                IngameIconType = IngameIconTypes.NPC;
        }
        else if (NameDictionary.TryGetValue(name, out var iconTypeByName))
            IngameIconType = iconTypeByName;        
        else {
            IngameIconType = IngameIconTypes.Uncategorized;
            foreach (var path in PathList) {
                if (entity.Path.StartsWith(path.Key)) {
                    IngameIconType = path.Value;
                    break;
                }
            }
        }


        // Debug
        if (settings.Debug && settings.DebugIngameIcons && Show()) {
            Log.Write($"--| IngameIcon | EntityType:{entity.Type} | IngameIconType:{IngameIconType} | Rarity:{Rarity} | MinimapIcon.Name:{name} | RenderName:{entity.GetComponent<Render>().Name} | Path:{entity.Path}");
        }
    }


}





