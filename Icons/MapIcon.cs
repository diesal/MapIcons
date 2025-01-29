using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ExileCore2.PoEMemory.Components;
using ExileCore2.Shared.Helpers;

namespace MapIcons.Icons;

public abstract class MapIcon {

    public int Version;
    public Entity Entity { get; }
    public RectangleF DrawRect { get; set; }
    public Func<Vector2> GridPosition { get; set; }
    public MonsterRarity Rarity { get; protected set; }
    public IconPriority Priority { get; protected set; }
    public Func<bool> Show { get; set; }
    public Func<bool> Hidden { get; protected set; } = () => false;
    public HudTexture InGameTexture { get; protected set; }
    public bool HasIngameIcon { get; protected set; }
    public string RenderName => Entity.RenderName;
    public string Text { get; protected set; }


    public MapIcon(Entity entity) {
        Entity = entity;
        if (Entity == null) return;

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

        if (Entity.TryGetComponent<MinimapIcon>(out var minimapIconComponent)) {
            var name = minimapIconComponent.Name;
            if (string.IsNullOrEmpty(name)) return;

            var iconIndexByName = Extensions.IconIndexByName(name);
            if (iconIndexByName != MapIconsIndex.MyPlayer) {
                InGameTexture = new HudTexture("Icons.png") { UV = SpriteHelper.GetUV(iconIndexByName), Size = 16 };
                HasIngameIcon = true;
            }

            if (Entity.HasComponent<Portal>() && Entity.TryGetComponent<Transitionable>(out var transitionable)) {
                Text = RenderName;
                Show = () => Entity.IsValid && transitionable.Flag1 == 2;
            }
            else {
                Show = () => Entity.GetComponent<MinimapIcon>() is { IsVisible: true, IsHide: false };
            }
        }


    }

}
