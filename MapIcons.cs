using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using ExileCore2.Shared.Cache;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;
using ImGuiNET;
using MapIcons.Icons;
using System.Numerics;

namespace MapIcons;

public class MapIcons : BaseSettingsPlugin<MapIconsSettings>
{
    private CachedValue<List<MapIcon>> _iconListCache;
    private IconAtlas _iconAtlas;
    private IconBuilder _iconBuilder;       
    private IconBuilder IconBuilder => _iconBuilder ??= new IconBuilder(this); // private IconBuilder IconBuilder { get { if (_iconBuilder == null) { _iconBuilder = new IconBuilder(this); } return _iconsBuilder; } }

    private IngameUIElements _ingameUi;
    private bool? _largeMap;
    private float _mapScale;
    private Vector2 _mapCenter;
    private const float CameraAngle = 38.7f * MathF.PI / 180;
    private static readonly float CameraAngleCos = MathF.Cos(CameraAngle);
    private static readonly float CameraAngleSin = MathF.Sin(CameraAngle);
    private SubMap LargeMapWindow => GameController.Game.IngameState.IngameUi.Map.LargeMap;

    private bool _showIconPicker = false;
    private string _selectedIconButton = "";
    //--| Initialise |--------------------------------------------------------------------------------------------------
    public override bool Initialise() {
        CanUseMultiThreading = true;

        Log.SetCustomHeaderControls(AddLogHeaderControls);
        IconBuilder.Initialise();
        Graphics.InitImage("Icons.png");
        _iconAtlas = new(Graphics, "MapIcons", Path.Combine(Path.GetDirectoryName(typeof(MapIcons).Assembly.Location), "MapIcons.png"), new Vector2(32, 32));
        _iconListCache = CreateIconListCache();

        return base.Initialise();
    }
    //--| Draw Settings |-----------------------------------------------------------------------------------------------
    private void IconButton(string id_name, string tooltip, ref int iconIndex, Vector4 tint) {
        (Vector2 uv0, Vector2 uv1) = _iconAtlas.GetIconUVs(iconIndex);
        if (ImGui.ImageButton($"##{id_name}", _iconAtlas.TextureId, new Vector2(16, 16), uv0, uv1, new Vector4(0, 0, 0, 0), tint)) {
            _showIconPicker = true;
            _selectedIconButton = id_name;
        }
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.Text("Icon");
            ImGui.EndTooltip();
        }
        if (_showIconPicker && _selectedIconButton == id_name) {
            _showIconPicker = ImGuiUtils.IconPickerWindow(id_name, ref iconIndex, _iconAtlas, tint);
        }
    }
    private static void IconSizeSliderInt(string id, ref int v, int v_min, int v_max) {
        ImGui.PushItemWidth(100);
        ImGui.SliderInt($"##{id}", ref v, v_min, v_max);
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.Text("Icon Size");
            ImGui.EndTooltip();
        }
        ImGui.PopItemWidth();
    }

    private string[] ingameIconStates = { "Off", "Ranged", "Always" };
    private bool IngameIconComboBox(string label, ref int selectedItem) {
        bool itemChanged = false;
        ImGui.PushItemWidth(100);
        if (ImGui.BeginCombo(label, ingameIconStates[selectedItem])) {
            for (int i = 0; i < ingameIconStates.Length; i++) {
                bool isSelected = (selectedItem == i);
                if (ImGui.Selectable(ingameIconStates[i], isSelected)) {
                    selectedItem = i;
                    itemChanged = true;
                }
                if (isSelected) {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
        ImGui.PopItemWidth();
        return itemChanged;
    }
    private void DrawNpcIconSettings() {
        // Vector4 childBgColor = ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ChildBg));

        // normal monsters 
        ImGuiUtils.Checkbox("##NormalMonsters", "Draw Normal Monsters", ref Settings.NormalMonsterDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Normal", ref Settings.NormalMonsterTint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Normal", ref Settings.NormalMonsterHiddenTint); ImGui.SameLine();
        IconButton("Normal Monster Icon", "Icon", ref Settings.NormalMonsterIconIndex, Settings.NormalMonsterTint); ImGui.SameLine();
        IconSizeSliderInt("##NormalMonsters", ref Settings.NormalMonsterSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Normal Monster");

        // magic monsters
        ImGuiUtils.Checkbox("##MagicMonsters", "Draw Magic Monsters", ref Settings.MagicMonsterDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Magic", ref Settings.MagicMonsterTint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Magic", ref Settings.MagicMonsterHiddenTint); ImGui.SameLine();
        IconButton("Magic Monster Icon", "Icon", ref Settings.MagicMonsterIconIndex, Settings.MagicMonsterTint); ImGui.SameLine();
        IconSizeSliderInt("##MagicMonsters", ref Settings.MagicMonsterSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Magic Monster");

        // rare monsters
        ImGuiUtils.Checkbox("##RareMonsters", "Draw Rare Monsters", ref Settings.RareMonsterDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Rare", ref Settings.RareMonsterTint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Rare", ref Settings.RareMonsterHiddenTint); ImGui.SameLine();
        IconButton("Rare Monster Icon", "Icon", ref Settings.RareMonsterIconIndex, Settings.RareMonsterTint); ImGui.SameLine();
        IconSizeSliderInt("##RareMonsters", ref Settings.RareMonsterSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Rare Monster");

        // unique monsters
        ImGuiUtils.Checkbox("##UniqueMonsters", "Draw Unique Monsters", ref Settings.UniqueMonsterDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Unique", ref Settings.UniqueMonsterTint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Unique", ref Settings.UniqueMonsterHiddenTint); ImGui.SameLine();
        IconButton("Unique Monster Icon", "Icon", ref Settings.UniqueMonsterIconIndex, Settings.UniqueMonsterTint); ImGui.SameLine();
        IconSizeSliderInt("##UniqueMonsters", ref Settings.UniqueMonsterSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Unique Monster");

        // spirits
        ImGuiUtils.Checkbox("##Spirits", "Draw Spirits", ref Settings.SpiritDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Spirits", ref Settings.SpiritTint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Spirits", ref Settings.SpiritHiddenTint); ImGui.SameLine();
        IconButton("Spirit Icon", "Icon", ref Settings.SpiritIconIndex, Settings.SpiritTint); ImGui.SameLine();
        IconSizeSliderInt("##Spirits", ref Settings.SpiritSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Spirit");

        // volatile cores
        ImGuiUtils.Checkbox("##VolatileCores", "Draw Volatile Cores", ref Settings.VolatileDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##VolatileCores", ref Settings.VolatileTint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##VolatileCores", ref Settings.VolatileHiddenTint); ImGui.SameLine();
        IconButton("Volatile Core Icon", "Icon", ref Settings.VolatileIconIndex, Settings.VolatileTint); ImGui.SameLine();
        IconSizeSliderInt("##VolatileCores", ref Settings.VolatileSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Volatile Core");

        // fracturing mirrors
        ImGuiUtils.Checkbox("##FracturingMirrors", "Draw Fracturing Mirrors", ref Settings.FracturingMirrorDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##FracturingMirrors", ref Settings.FracturingMirrorTint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##FracturingMirrors", ref Settings.FracturingMirrorHiddenTint); ImGui.SameLine();
        IconButton("Fracturing Mirror Icon", "Icon", ref Settings.FracturingMirrorIconIndex, Settings.FracturingMirrorTint); ImGui.SameLine();
        IconSizeSliderInt("##FracturingMirrors", ref Settings.FracturingMirrorSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Fracturing Mirror");

        // minions
        ImGuiUtils.Checkbox("##Minions", "Draw Minions", ref Settings.MinionDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Minions", ref Settings.MinionTint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Minions", ref Settings.MinionHiddenTint); ImGui.SameLine();
        IconButton("Minion Icon", "Icon", ref Settings.MinionIconIndex, Settings.MinionTint); ImGui.SameLine();
        IconSizeSliderInt("##Minions", ref Settings.MinionSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Minion");

        // NPCs
        ImGuiUtils.Checkbox("##NPCs", "Draw NPCs", ref Settings.NPCDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##NPCs", ref Settings.NPCTint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##NPCs", ref Settings.NPCHiddenTint); ImGui.SameLine();
        IconButton("NPC Icon", "Icon", ref Settings.NPCIconIndex, Settings.NPCTint); ImGui.SameLine();
        IconSizeSliderInt("##NPCs", ref Settings.NPCSize, 0, 32); ImGui.SameLine();
        ImGuiUtils.Checkbox("##NPCText", "Draw NPC Name", ref Settings.NPCTextShow); ImGui.SameLine();
        ImGui.Text("NPC");

    }
    private void DrawMiscIconSettings() {
        ImGuiUtils.Checkbox("##Players", "Draw Players", ref Settings.PlayerDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Players", ref Settings.PlayerTint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Players", ref Settings.NPCHiddenTint); ImGui.SameLine();
        IconButton("Player Icon", "Icon", ref Settings.PlayerIconIndex, Settings.PlayerTint); ImGui.SameLine();
        IconSizeSliderInt("##Players", ref Settings.PlayerSize, 0, 32); ImGui.SameLine();
        ImGuiUtils.Checkbox("##PlayerText", "Draw Player Names", ref Settings.PlayerTextShow); ImGui.SameLine();
        ImGui.Text("Players");
    }
    private void DrawChestSettings() {

        // normal chest
        ImGuiUtils.Checkbox("##NormalChest", "Draw Normal Chest", ref Settings.NormalChestDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##NormalChest", ref Settings.NormalChestTint); ImGui.SameLine();
        IconButton("Normal Chest Icon", "Icon", ref Settings.NormalChestIconIndex, Settings.NormalChestTint); ImGui.SameLine();
        IconSizeSliderInt("##NormalChest", ref Settings.NormalChestSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Normal Chest");

        // magic chest
        ImGuiUtils.Checkbox("##MagicChest", "Draw Magic Chest", ref Settings.MagicChestDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##MagicChest", ref Settings.MagicChestTint); ImGui.SameLine();
        IconButton("Magic Chest Icon", "Icon", ref Settings.MagicChestIconIndex, Settings.MagicChestTint); ImGui.SameLine();
        IconSizeSliderInt("##MagicChest", ref Settings.MagicChestSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Magic Chest");

        // rare chest
        ImGuiUtils.Checkbox("##RareChest", "Draw Rare Chest", ref Settings.RareChestDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##RareChest", ref Settings.RareChestTint); ImGui.SameLine();
        IconButton("Rare Chest Icon", "Icon", ref Settings.RareChestIconIndex, Settings.RareChestTint); ImGui.SameLine();
        IconSizeSliderInt("##RareChest", ref Settings.RareChestSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Rare Chest");

        // Breach Hand
        ImGuiUtils.Checkbox("##BreachHand", "Draw Breach Hand", ref Settings.BreachChestNormalDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##BreachHand", ref Settings.BreachChestNormalTint); ImGui.SameLine();
        IconButton("Breach Hand Icon", "Icon", ref Settings.BreachChestNormalIconIndex, Settings.BreachChestNormalTint); ImGui.SameLine();
        IconSizeSliderInt("##BreachHand", ref Settings.BreachChestNormalSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Breach Hand");

        // Breach Boss Hand
        ImGuiUtils.Checkbox("##BreachBossHand", "Draw Breach Boss Hand", ref Settings.BreachChestBossDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##BreachBossHand", ref Settings.BreachChestBossTint); ImGui.SameLine();
        IconButton("Breach Boss Hand Icon", "Icon", ref Settings.BreachChestBossIconIndex, Settings.BreachChestBossTint); ImGui.SameLine();
        IconSizeSliderInt("##BreachBossHand", ref Settings.BreachChestBossSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Breach Boss Hand");

        //expedition chest normal
        ImGuiUtils.Checkbox("##ExpeditionNormalChest", "Draw Expedition Normal Chest", ref Settings.ExpeditionNormalChestDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ExpeditionNormalChest", ref Settings.ExpeditionNormalChestTint); ImGui.SameLine();
        IconButton("Expedition Normal Chest Icon", "Icon", ref Settings.ExpeditionNormalChestIconIndex, Settings.ExpeditionNormalChestTint); ImGui.SameLine();
        IconSizeSliderInt("##ExpeditionNormalChest", ref Settings.ExpeditionNormalChestSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Expedition Normal Chest");

        //expedition chest magic
        ImGuiUtils.Checkbox("##ExpeditionMagicChest", "Draw Expedition Magic Chest", ref Settings.ExpeditionMagicChestDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ExpeditionMagicChest", ref Settings.ExpeditionMagicChestTint); ImGui.SameLine();
        IconButton("Expedition Magic Chest Icon", "Icon", ref Settings.ExpeditionMagicChestIconIndex, Settings.ExpeditionMagicChestTint); ImGui.SameLine();
        IconSizeSliderInt("##ExpeditionMagicChest", ref Settings.ExpeditionMagicChestSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Expedition Magic Chest");

        //expedition chest rare
        ImGuiUtils.Checkbox("##ExpeditionRareChest", "Draw Expedition Rare Chest", ref Settings.ExpeditionRareChestDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ExpeditionRareChest", ref Settings.ExpeditionRareChestTint); ImGui.SameLine();
        IconButton("Expedition Rare Chest Icon", "Icon", ref Settings.ExpeditionRareChestIconIndex, Settings.ExpeditionRareChestTint); ImGui.SameLine();
        IconSizeSliderInt("##ExpeditionRareChest", ref Settings.ExpeditionRareChestSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Expedition Rare Chest");

    }
    private void DrawStrongboxSettings() {

        ImGuiUtils.Checkbox("##UnknownStrongbox", "Draw Unknown Strongbox", ref Settings.UnknownStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##UnknownStrongbox", ref Settings.UnknownStrongboxTint); ImGui.SameLine();
        IconButton("Unknown Strongbox Icon", "Icon", ref Settings.UnknownStrongboxIconIndex, Settings.UnknownStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##UnknownStrongbox", ref Settings.UnknownStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Unknown Strongbox");

        ImGuiUtils.Checkbox("##ArcanistStrongbox", "Draw Arcanist Strongbox", ref Settings.ArcanistStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ArcanistStrongbox", ref Settings.ArcanistStrongboxTint); ImGui.SameLine();
        IconButton("Arcanist Strongbox Icon", "Icon", ref Settings.ArcanistStrongboxIconIndex, Settings.ArcanistStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##ArcanistStrongbox", ref Settings.ArcanistStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Arcanist Strongbox");

        ImGuiUtils.Checkbox("##ArmourerStrongbox", "Draw Armourer Strongbox", ref Settings.ArmourerStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ArmourerStrongbox", ref Settings.ArmourerStrongboxTint); ImGui.SameLine();
        IconButton("Armourer Strongbox Icon", "Icon", ref Settings.ArmourerStrongboxIconIndex, Settings.ArmourerStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##ArmourerStrongbox", ref Settings.ArmourerStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Armourer Strongbox");

        ImGuiUtils.Checkbox("##BlacksmithStrongbox", "Draw Blacksmith Strongbox", ref Settings.BlacksmithStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##BlacksmithStrongbox", ref Settings.BlacksmithStrongboxTint); ImGui.SameLine();
        IconButton("Blacksmith Strongbox Icon", "Icon", ref Settings.BlacksmithStrongboxIconIndex, Settings.BlacksmithStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##BlacksmithStrongbox", ref Settings.BlacksmithStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Blacksmith Strongbox");

        ImGuiUtils.Checkbox("##ArtisanStrongbox", "Draw Artisan Strongbox", ref Settings.ArtisanStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ArtisanStrongbox", ref Settings.ArtisanStrongboxTint); ImGui.SameLine();
        IconButton("Artisan Strongbox Icon", "Icon", ref Settings.ArtisanStrongboxIconIndex, Settings.ArtisanStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##ArtisanStrongbox", ref Settings.ArtisanStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Artisan Strongbox");

        ImGuiUtils.Checkbox("##CartographerStrongbox", "Draw Cartographer Strongbox", ref Settings.CartographerStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##CartographerStrongbox", ref Settings.CartographerStrongboxTint); ImGui.SameLine();
        IconButton("Cartographer Strongbox Icon", "Icon", ref Settings.CartographerStrongboxIconIndex, Settings.CartographerStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##CartographerStrongbox", ref Settings.CartographerStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Cartographer Strongbox");

        ImGuiUtils.Checkbox("##ChemistStrongbox", "Draw Chemist Strongbox", ref Settings.ChemistStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ChemistStrongbox", ref Settings.ChemistStrongboxTint); ImGui.SameLine();
        IconButton("Chemist Strongbox Icon", "Icon", ref Settings.ChemistStrongboxIconIndex, Settings.ChemistStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##ChemistStrongbox", ref Settings.ChemistStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Chemist Strongbox");

        ImGuiUtils.Checkbox("##GemcutterStrongbox", "Draw Gemcutter Strongbox", ref Settings.GemcutterStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##GemcutterStrongbox", ref Settings.GemcutterStrongboxTint); ImGui.SameLine();
        IconButton("Gemcutter Strongbox Icon", "Icon", ref Settings.GemcutterStrongboxIconIndex, Settings.GemcutterStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##GemcutterStrongbox", ref Settings.GemcutterStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Gemcutter Strongbox");

        ImGuiUtils.Checkbox("##JewellerStrongbox", "Draw Jeweller Strongbox", ref Settings.JewellerStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##JewellerStrongbox", ref Settings.JewellerStrongboxTint); ImGui.SameLine();
        IconButton("Jeweller Strongbox Icon", "Icon", ref Settings.JewellerStrongboxIconIndex, Settings.JewellerStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##JewellerStrongbox", ref Settings.JewellerStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Jeweller Strongbox");

        ImGuiUtils.Checkbox("##LargeStrongbox", "Draw Large Strongbox", ref Settings.LargeStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##LargeStrongbox", ref Settings.LargeStrongboxTint); ImGui.SameLine();
        IconButton("Large Strongbox Icon", "Icon", ref Settings.LargeStrongboxIconIndex, Settings.LargeStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##LargeStrongbox", ref Settings.LargeStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Large Strongbox");

        ImGuiUtils.Checkbox("##OrnateStrongbox", "Draw Ornate Strongbox", ref Settings.OrnateStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##OrnateStrongbox", ref Settings.OrnateStrongboxTint); ImGui.SameLine();
        IconButton("Ornate Strongbox Icon", "Icon", ref Settings.OrnateStrongboxIconIndex, Settings.OrnateStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##OrnateStrongbox", ref Settings.OrnateStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Ornate Strongbox");

        ImGuiUtils.Checkbox("##Strongbox", "Draw Strongbox", ref Settings.StrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Strongbox", ref Settings.StrongboxTint); ImGui.SameLine();
        IconButton("Strongbox Icon", "Icon", ref Settings.StrongboxIconIndex, Settings.StrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##Strongbox", ref Settings.StrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Strongbox");

        ImGuiUtils.Checkbox("##DivinerStrongbox", "Draw Diviner Strongbox", ref Settings.DivinerStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##DivinerStrongbox", ref Settings.DivinerStrongboxTint); ImGui.SameLine();
        IconButton("Diviner Strongbox Icon", "Icon", ref Settings.DivinerStrongboxIconIndex, Settings.DivinerStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##DivinerStrongbox", ref Settings.DivinerStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Diviner Strongbox");

        ImGuiUtils.Checkbox("##OperativeStrongbox", "Draw Operative Strongbox", ref Settings.OperativeStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##OperativeStrongbox", ref Settings.OperativeStrongboxTint); ImGui.SameLine();
        IconButton("Operative Strongbox Icon", "Icon", ref Settings.OperativeStrongboxIconIndex, Settings.OperativeStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##OperativeStrongbox", ref Settings.OperativeStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Operative Strongbox");

        ImGuiUtils.Checkbox("##ArcaneStrongbox", "Draw Arcane Strongbox", ref Settings.ArcaneStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ArcaneStrongbox", ref Settings.ArcaneStrongboxTint); ImGui.SameLine();
        IconButton("Arcane Strongbox Icon", "Icon", ref Settings.ArcaneStrongboxIconIndex, Settings.ArcaneStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##ArcaneStrongbox", ref Settings.ArcaneStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Arcane Strongbox");

        ImGuiUtils.Checkbox("##ResearcherStrongbox", "Draw Researcher Strongbox", ref Settings.ResearcherStrongboxDraw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ResearcherStrongbox", ref Settings.ResearcherStrongboxTint); ImGui.SameLine();
        IconButton("Researcher Strongbox Icon", "Icon", ref Settings.ResearcherStrongboxIconIndex, Settings.ResearcherStrongboxTint); ImGui.SameLine();
        IconSizeSliderInt("##ResearcherStrongbox", ref Settings.ResearcherStrongboxSize, 0, 32); ImGui.SameLine();
        ImGui.Text("Researcher Strongbox");
    }
    private void DrawIngameIconsSettings() {
        IngameIconComboBox("##AreaTransition", ref Settings.AreaTransitionState); ImGui.SameLine();
        ImGui.Text("Area Transition");

        IngameIconComboBox("##Breach", ref Settings.BreachState); ImGui.SameLine();
        ImGui.Text("Breach");

        IngameIconComboBox("##Checkpoint", ref Settings.CheckpointState); ImGui.SameLine();
        ImGui.Text("Checkpoint");

        IngameIconComboBox("##QuestObject", ref Settings.QuestObjectState); ImGui.SameLine();
        ImGui.Text("Quest Object");

        IngameIconComboBox("##NPC", ref Settings.NPCState); ImGui.SameLine();
        ImGui.Text("NPC");

        IngameIconComboBox("##Ritual", ref Settings.RitualState); ImGui.SameLine();
        ImGui.Text("Ritual");

        IngameIconComboBox("##Shrine", ref Settings.ShrineState); ImGui.SameLine();
        ImGuiUtils.Checkbox("##ShrineText","Show Shrine Name", ref Settings.ShrineTextShow); ImGui.SameLine();
        ImGui.Text("Shrine");

        IngameIconComboBox("##Waypoint", ref Settings.WaypointState); ImGui.SameLine();
        ImGui.Text("Waypoint");

        IngameIconComboBox("##Uncategorized", ref Settings.UncategorizedState); ImGui.SameLine();
        ImGui.Text("Uncategorized");
    }
    public override void DrawSettings() {

        ImGui.PushItemWidth(100); // Set slider width
        ImGui.SliderInt("Rebuild", ref Settings.RunEveryXTicks, 1, 20);
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.Text("Set the interval (in ticks) for rebuilding the icons");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        ImGui.SliderInt("ReCache", ref Settings.IconListUpdatePeriod, 10, 1000);
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.Text("Set the interval (in milliseconds) for refreshing the icon cache");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        ImGui.PopItemWidth(); // Reset slider width
        ImGui.Checkbox("Debug", ref Settings.Debug);

        if (ImGuiUtils.CollapsingHeader("Draw Settings", ref Settings.DrawSetingsOpen)) {
            ImGui.Indent();
            ImGuiUtils.Checkbox("Draw on Minimap", "Draw Monsters on the minimap", ref Settings.DrawOnMinimap);
            ImGuiUtils.Checkbox("Draw cached Entities", "Draw entities that are cached but no longer in proximity", ref Settings.DrawCachedEntities);
            ImGuiUtils.Checkbox("Draw Over Large Panels", "Enable drawing over large panels", ref Settings.DrawOverLargePanels);
            ImGuiUtils.Checkbox("Draw Over Fullscreen Panels", "Enable drawing over fullscreen panels", ref Settings.DrawOverFullscreenPanels);
            ImGui.Unindent();
        }
        if (ImGuiUtils.CollapsingHeader("Ignored Entities", ref Settings.IgnoderEntitesOpen)) {
            ImGui.Indent();
            if (ImGui.Button("Update")) { IconBuilder.UpdateIgnoredEntities(); }
            ImGui.SameLine();
            ImGui.SliderInt("Height", ref Settings.IgnoredEntitiesHeight, 100, 1000);
            ImGui.InputTextMultiline("##ignoredEntitiesInput", ref Settings.ignoredEntities, 1000, new Vector2(ImGui.GetContentRegionAvail().X, Settings.IgnoredEntitiesHeight));
            ImGui.Unindent();
        };
        if (ImGuiUtils.CollapsingHeader("Ingame Icons", ref Settings.RangeIconsOpen)) {
            ImGui.Indent();
            DrawIngameIconsSettings();
            ImGui.Unindent();
        };
        if (ImGuiUtils.CollapsingHeader("NPC Icons", ref Settings.NPCIconsOpen)) {
            ImGui.Indent();
            DrawNpcIconSettings();
            ImGui.Unindent();
        };
        if (ImGuiUtils.CollapsingHeader("Misc Icons", ref Settings.MiscIconsOpen)) {
            ImGui.Indent();
            DrawMiscIconSettings();
            ImGui.Unindent();
        };
        if (ImGuiUtils.CollapsingHeader("Chest Icons", ref Settings.ChestIconsOpen)) {
            ImGui.Indent();
            DrawChestSettings();
            ImGui.Unindent();
        };
        if (ImGuiUtils.CollapsingHeader("Strongbox Icons", ref Settings.StrongboxIconsOpen)) {
            ImGui.Indent();
            DrawStrongboxSettings();
            ImGui.Unindent();
        };
    }
    //--| Tick |-------------------------------------------------------------------------------------------------------
    public override void Tick() {
        IconBuilder.Tick();
        _ingameUi = GameController.Game.IngameState.IngameUi;

        var smallMiniMap = _ingameUi.Map.SmallMiniMap;
        if (smallMiniMap.IsValid && smallMiniMap.IsVisibleLocal) {
            var mapRect = smallMiniMap.GetClientRectCache;
            _mapCenter = mapRect.Center;
            _largeMap = false;
            _mapScale = smallMiniMap.MapScale;
        }
        else if (_ingameUi.Map.LargeMap.IsVisibleLocal) {
            var largeMapWindow = LargeMapWindow;
            _mapCenter = largeMapWindow.MapCenter;
            _largeMap = true;
            _mapScale = largeMapWindow.MapScale;
        }
        else {
            _largeMap = null;
        }
    }
    //--| Render |-----------------------------------------------------------------------------------------------------
        private bool GetIconProperties(MapIcon icon, out string iconFileName, out int iconSize, out System.Drawing.Color iconColor, out RectangleF iconUV, out bool showText) {
        iconFileName = null;
        iconSize = 0;
        iconColor = System.Drawing.Color.White;
        iconUV = new RectangleF();
        showText = false;

        //if (icon.HasIngameIcon &&
        //    icon is not CustomIcon &&
        //    (!Settings.DrawReplacementsForGameIconsWhenOutOfRange || icon.Entity.IsValid) &&
        //    !Settings.AlwaysShownIngameIcons.Content.Any(x => x.Value.Equals(icon.Entity.Path)))
        //    continue; 

        if (icon is NPC_MapIcon npc_icon) {
            iconFileName = _iconAtlas.Name;

            switch(npc_icon.NPCType) {
                case NPCTypes.NPC:
                    if (!Settings.NPCDraw) return false;
                    iconSize = Settings.NPCSize;
                    iconUV = _iconAtlas.GetIconUV(Settings.NPCIconIndex);
                    iconColor = ImGuiUtils.Vector4ToColor(Settings.NPCTint);
                    showText = Settings.NPCTextShow;
                    break;
                case NPCTypes.Spirit:
                    iconSize = Settings.SpiritSize;
                    iconUV = _iconAtlas.GetIconUV(Settings.SpiritIconIndex);
                    iconColor = ImGuiUtils.Vector4ToColor(Settings.SpiritTint);
                    break;
                case NPCTypes.VolatileCore:
                    iconSize = Settings.VolatileSize;
                    iconUV = _iconAtlas.GetIconUV(Settings.VolatileIconIndex);
                    iconColor = ImGuiUtils.Vector4ToColor(Settings.VolatileTint);
                    break;
                case NPCTypes.Minion:
                    iconSize = Settings.MinionSize;
                    iconUV = _iconAtlas.GetIconUV(Settings.MinionIconIndex);
                    iconColor = ImGuiUtils.Vector4ToColor(Settings.MinionTint);
                    break;
                case NPCTypes.FracturingMirror:
                    iconSize = Settings.FracturingMirrorSize;
                    iconUV = _iconAtlas.GetIconUV(Settings.FracturingMirrorIconIndex);
                    iconColor = ImGuiUtils.Vector4ToColor(Settings.FracturingMirrorTint);
                    break;
                case NPCTypes.Monster:
                    switch (icon.Rarity) {
                        case MonsterRarity.White:
                            if (!Settings.NormalMonsterDraw) return false;
                            iconSize = Settings.NormalMonsterSize;
                            iconUV = _iconAtlas.GetIconUV(Settings.NormalMonsterIconIndex);
                            iconColor = icon.Hidden() ? ImGuiUtils.Vector4ToColor(Settings.NormalMonsterHiddenTint) : ImGuiUtils.Vector4ToColor(Settings.NormalMonsterTint);
                            break;
                        case MonsterRarity.Magic:
                            if (!Settings.MagicMonsterDraw) return false;
                            iconSize = Settings.MagicMonsterSize;
                            iconUV = _iconAtlas.GetIconUV(Settings.MagicMonsterIconIndex);
                            iconColor = icon.Hidden() ? ImGuiUtils.Vector4ToColor(Settings.MagicMonsterHiddenTint) : ImGuiUtils.Vector4ToColor(Settings.MagicMonsterTint);
                            break;
                        case MonsterRarity.Rare:
                            if (!Settings.RareMonsterDraw) return false;
                            iconSize = Settings.RareMonsterSize;
                            iconUV = _iconAtlas.GetIconUV(Settings.RareMonsterIconIndex);
                            iconColor = icon.Hidden() ? ImGuiUtils.Vector4ToColor(Settings.RareMonsterHiddenTint) : ImGuiUtils.Vector4ToColor(Settings.RareMonsterTint);
                            break;
                        case MonsterRarity.Unique:
                            if (!Settings.UniqueMonsterDraw) return false;
                            iconSize = Settings.UniqueMonsterSize;
                            iconUV = _iconAtlas.GetIconUV(Settings.UniqueMonsterIconIndex);
                            iconColor = icon.Hidden() ? ImGuiUtils.Vector4ToColor(Settings.UniqueMonsterHiddenTint) : ImGuiUtils.Vector4ToColor(Settings.UniqueMonsterTint);
                            break;
                        default:
                            return false;
                    }
                    break;
                default:
                    return false;                    
            }            
        }
        else if (icon is Chest_MapIcon chest_icon) {
            if (chest_icon.ChestType == ChestTypes.General) {
                iconFileName = _iconAtlas.Name;
                switch (chest_icon.Rarity) {

                    case MonsterRarity.Magic:
                        if (!Settings.MagicChestDraw) return false;
                        iconSize = Settings.MagicChestSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.MagicChestIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.MagicChestTint);
                        break;

                    case MonsterRarity.Rare:
                        if (!Settings.RareChestDraw) return false;
                        iconSize = Settings.RareChestSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.RareChestIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.RareChestTint);
                        break;

                    default:
                        if (!Settings.NormalChestDraw) return false;
                        iconSize = Settings.NormalChestSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.NormalChestIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.NormalChestTint);
                        break;

                }
            }
            else if (chest_icon.ChestType == ChestTypes.Expedition) {
                iconFileName = _iconAtlas.Name;
                switch (chest_icon.Rarity) {

                    case MonsterRarity.Magic:
                        if (!Settings.ExpeditionMagicChestDraw) return false;
                        iconSize = Settings.ExpeditionMagicChestSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.ExpeditionMagicChestIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.ExpeditionMagicChestTint);
                        break;

                    case MonsterRarity.Rare:
                        if (!Settings.ExpeditionRareChestDraw) return false;
                        iconSize = Settings.ExpeditionRareChestSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.ExpeditionRareChestIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.ExpeditionRareChestTint);
                        break;

                    default:
                        if (!Settings.ExpeditionNormalChestDraw) return false;
                        iconSize = Settings.ExpeditionNormalChestSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.ExpeditionNormalChestIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.ExpeditionNormalChestTint);
                        break;
                }
            }
            else if (chest_icon.ChestType == ChestTypes.Breach) {
                iconFileName = _iconAtlas.Name;
                if (chest_icon.BreachChestType == BreachChestTypes.Boss) {
                    if (!Settings.BreachChestBossDraw) return false;
                    iconSize = Settings.BreachChestBossSize;
                    iconUV = _iconAtlas.GetIconUV(Settings.BreachChestBossIconIndex);
                    iconColor = ImGuiUtils.Vector4ToColor(Settings.BreachChestBossTint);
                }
                else {
                    if (!Settings.BreachChestNormalDraw) return false;
                    iconSize = Settings.BreachChestNormalSize;
                    iconUV = _iconAtlas.GetIconUV(Settings.BreachChestNormalIconIndex);
                    iconColor = ImGuiUtils.Vector4ToColor(Settings.BreachChestNormalTint);
                }
            }
            else if (chest_icon.ChestType == ChestTypes.Strongbox) {
                iconFileName = _iconAtlas.Name;
                switch (chest_icon.StrongboxType) {

                    case StrongboxTypes.Arcanist:
                        if (!Settings.ArcanistStrongboxDraw) return false;
                        iconSize = Settings.ArcanistStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.ArcanistStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.ArcanistStrongboxTint);
                        break;

                    case StrongboxTypes.Armourer:
                        if (!Settings.ArmourerStrongboxDraw) return false;
                        iconSize = Settings.ArmourerStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.ArmourerStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.ArmourerStrongboxTint);
                        break;

                    case StrongboxTypes.Blacksmith:
                        if (!Settings.BlacksmithStrongboxDraw) return false;
                        iconSize = Settings.BlacksmithStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.BlacksmithStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.BlacksmithStrongboxTint);
                        break;

                    case StrongboxTypes.Artisan:
                        if (!Settings.ArtisanStrongboxDraw) return false;
                        iconSize = Settings.ArtisanStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.ArtisanStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.ArtisanStrongboxTint);
                        break;

                    case StrongboxTypes.Cartographer:
                        if (!Settings.CartographerStrongboxDraw) return false;
                        iconSize = Settings.CartographerStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.CartographerStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.CartographerStrongboxTint);
                        break;

                    case StrongboxTypes.Chemist:
                        if (!Settings.ChemistStrongboxDraw) return false;
                        iconSize = Settings.ChemistStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.ChemistStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.ChemistStrongboxTint);
                        break;

                    case StrongboxTypes.Gemcutter:
                        if (!Settings.GemcutterStrongboxDraw) return false;
                        iconSize = Settings.GemcutterStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.GemcutterStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.GemcutterStrongboxTint);
                        break;

                    case StrongboxTypes.Jeweller:
                        if (!Settings.JewellerStrongboxDraw) return false;
                        iconSize = Settings.JewellerStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.JewellerStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.JewellerStrongboxTint);
                        break;

                    case StrongboxTypes.Large:
                        if (!Settings.LargeStrongboxDraw) return false;
                        iconSize = Settings.LargeStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.LargeStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.LargeStrongboxTint);
                        break;

                    case StrongboxTypes.Ornate:
                        if (!Settings.OrnateStrongboxDraw) return false;
                        iconSize = Settings.OrnateStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.OrnateStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.OrnateStrongboxTint);
                        break;

                    case StrongboxTypes.Strongbox:
                        if (!Settings.StrongboxDraw) return false;
                        iconSize = Settings.StrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.StrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.StrongboxTint);
                        break;

                    case StrongboxTypes.Diviner:
                        if (!Settings.DivinerStrongboxDraw) return false;
                        iconSize = Settings.DivinerStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.DivinerStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.DivinerStrongboxTint);
                        break;

                    case StrongboxTypes.Operative:
                        if (!Settings.OperativeStrongboxDraw) return false;
                        iconSize = Settings.OperativeStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.OperativeStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.OperativeStrongboxTint);
                        break;

                    case StrongboxTypes.Arcane:
                        if (!Settings.ArcaneStrongboxDraw) return false;
                        iconSize = Settings.ArcaneStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.ArcaneStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.ArcaneStrongboxTint);
                        break;

                    case StrongboxTypes.Researcher:
                        if (!Settings.ResearcherStrongboxDraw) return false;
                        iconSize = Settings.ResearcherStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.ResearcherStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.ResearcherStrongboxTint);
                        break;

                    case StrongboxTypes.Unknown:
                        if (!Settings.UnknownStrongboxDraw) return false;
                        iconSize = Settings.UnknownStrongboxSize;
                        iconUV = _iconAtlas.GetIconUV(Settings.UnknownStrongboxIconIndex);
                        iconColor = ImGuiUtils.Vector4ToColor(Settings.UnknownStrongboxTint);
                        break;

                    default:
                        return false;

                }
            }
        }
        else if (icon is Ingame_MapIcon ingame_icon) {
            iconFileName = ingame_icon.InGameTexture.FileName;
            switch (ingame_icon.IngameIconType) {
                case IngameIconTypes.AreaTransition:
                    if (Settings.AreaTransitionState == 0 || (Settings.AreaTransitionState == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                case IngameIconTypes.Breach:
                    if (Settings.BreachState == 0 || (Settings.BreachState == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                case IngameIconTypes.Checkpoint: 
                    if (Settings.CheckpointState == 0 || (Settings.CheckpointState == 1 && ingame_icon.Entity.IsValid)) return false;                    
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                case IngameIconTypes.QuestObject:
                    if (Settings.QuestObjectState == 0 || (Settings.QuestObjectState == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                case IngameIconTypes.NPC:
                    if (Settings.NPCState == 0 || (Settings.NPCState == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                case IngameIconTypes.Ritual:
                    if (Settings.RitualState == 0 || (Settings.RitualState == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    if (ingame_icon.Entity.GetComponent<MinimapIcon>()?.Name == "RitualRuneFinished")
                        iconColor = ImGuiUtils.Vector4ToColor(new Vector4(.8f,.8f,.8f,1));
                    break;
                case IngameIconTypes.Shrine:
                    if (Settings.ShrineState == 0 || (Settings.ShrineState == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    showText = Settings.ShrineTextShow;
                    break;
                case IngameIconTypes.Waypoint:
                    if (Settings.WaypointState == 0 || (Settings.WaypointState == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                case IngameIconTypes.Uncategorized:
                    if (Settings.UncategorizedState == 0 || (Settings.UncategorizedState == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                default:
                    return false;                
            }    
        }        
        else if (icon is Misc_MapIcon misc_icon) {
            iconFileName = _iconAtlas.Name;
            if (misc_icon.MiscIconType == MiscIconTypes.Player) {
                if (!Settings.PlayerDraw) return false;
                iconSize = Settings.PlayerSize;
                iconUV = _iconAtlas.GetIconUV(Settings.PlayerIconIndex);
                iconColor = ImGuiUtils.Vector4ToColor(Settings.PlayerTint);
                showText = Settings.PlayerTextShow;
            }
        }
        else {
            return false;
        }
        return iconFileName != null && iconSize != 0 && iconUV != new RectangleF();
    }
    private Vector2 DeltaInWorldToMinimapDelta(Vector2 delta, float deltaZ) {
        return _mapScale * Vector2.Multiply(new Vector2(delta.X - delta.Y, deltaZ - (delta.X + delta.Y)), new Vector2(CameraAngleCos, CameraAngleSin));
    }
    public override void Render() {
        // ui rendering unbound by DrawSettings
        Log.Render(ref Settings.Debug);

        if (_largeMap == null || !GameController.InGame || !Settings.DrawOnMinimap && _largeMap != true) return;
        if (!Settings.DrawOverFullscreenPanels && _ingameUi.FullscreenPanels.Any(x => x.IsVisible) || Settings.DrawOverLargePanels && _ingameUi.LargePanels.Any(x => x.IsVisible)) return;
        if (LargeMapWindow == null) return;

        var playerRender = GameController?.Player?.GetComponent<Render>();
        if (playerRender == null) return;
        var playerPos = playerRender.Pos.WorldToGrid();
        var playerHeight = -playerRender.UnclampedHeight;

        var overlayIcons = _iconListCache.Value;
        if (overlayIcons == null) return;

        foreach (var icon in overlayIcons) {

            if (icon?.Entity == null) continue;
            if (!icon.Show()) continue;

            //if (icon.HasIngameIcon) continue; // TODO

            if (!GetIconProperties(icon, out var iconFileName, out var iconSize, out var iconColor, out var iconUV, out var showText)) continue;
            var iconGridPos = icon.GridPosition();
            var position = _mapCenter + DeltaInWorldToMinimapDelta(iconGridPos - playerPos, (playerHeight + GameController.IngameState.Data.GetTerrainHeightAt(iconGridPos)) * PoeMapExtension.WorldToGridConversion);

            float halfSize = iconSize / 2;
            icon.DrawRect = new RectangleF(position.X - halfSize, position.Y - halfSize, iconSize, iconSize);
            var drawRect = icon.DrawRect;
            if (_largeMap == false && !_ingameUi.Map.SmallMiniMap.GetClientRectCache.Contains(drawRect)) continue;

            Graphics.DrawImage(iconFileName, drawRect, iconUV, iconColor);
            if (showText) Graphics.DrawText(icon.Text, position.Translate(0, 0), FontAlign.Center);
            //Graphics.DrawText($"{icon.Show()} {icon.Hidden()} {icon.Rarity} {icon.Entity}", position.Translate(0, 0), FontAlign.Center);
        }
    }
    //--| Misc |-------------------------------------------------------------------------------------------------------
    private TimeCache<List<MapIcon>> CreateIconListCache() {
        return new TimeCache<List<MapIcon>>(() => {
            var entitySource = Settings.DrawCachedEntities
                ? GameController?.EntityListWrapper.Entities
                : GameController?.EntityListWrapper?.OnlyValidEntities;
            var baseIcons = entitySource?.Select(x => x.GetHudComponent<MapIcon>())
                .Where(icon => icon != null)
                .OrderBy(x => x.Priority)
                .ToList();
            return baseIcons ?? [];
        }, Settings.IconListUpdatePeriod);
    }
    private void AddLogHeaderControls() {
        ImGui.SameLine();
        ImGui.Checkbox("NPC", ref Settings.DebugNPC); ImGui.SameLine();
        ImGui.Checkbox("Misc", ref Settings.DebugMiscIcons); ImGui.SameLine();
        ImGui.Checkbox("Chest", ref Settings.DebugChests); ImGui.SameLine();
        ImGui.Checkbox("Ingame", ref Settings.DebugIngameIcons); ImGui.SameLine();
        if (ImGui.Button("Rebuild Icons")) {
            IconBuilder.RebuildIcons();
        }
    }

}