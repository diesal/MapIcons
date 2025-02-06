using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using ExileCore2.Shared.Cache;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;
using ImGuiNET;
using System.Numerics;
using System.Runtime.CompilerServices;

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

        Settings.InitCustomIconSettings();

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

    private string[] debugIconStates = { "Off", "All", "Valid", "Invalid" };
    private bool DebugIconComboBox(string label, ref int selectedItem) {
        bool itemChanged = false;
        ImGui.PushItemWidth(80);
        if (ImGui.BeginCombo(label, debugIconStates[selectedItem])) {
            for (int i = 0; i < debugIconStates.Length; i++) {
                bool isSelected = (selectedItem == i);
                if (ImGui.Selectable(debugIconStates[i], isSelected)) {
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
    private void DrawCustomIconPathSettings() {       

        for (int i = 0; i < Settings.CustomIconSettingsList.Count; i++) {
            var setting = Settings.CustomIconSettingsList[i];

            ImGuiUtils.Checkbox($"##CustomPath{i}", "Draw Custom Path", ref setting.Setting_Draw); ImGui.SameLine();
            ImGuiUtils.ColorSwatch($"Icon Tint ##CustomPath{i}", ref setting.Setting_Tint); ImGui.SameLine();
            ImGuiUtils.ColorSwatch($"Hidden Icon Tint ##CustomPath{i}", ref setting.Setting_HiddenTint); ImGui.SameLine();
            IconButton($"Custom Path Icon {i}", "Icon", ref setting.Setting_Index, setting.Setting_Tint); ImGui.SameLine();
            IconSizeSliderInt($"##CustomPath{i}", ref setting.Setting_Size, 0, 32); ImGui.SameLine();
            ImGuiUtils.Checkbox($"##CustomPathText{i}", "Draw Text", ref setting.Setting_DrawText); ImGui.SameLine();
            ImGuiUtils.Checkbox($"##CustomPathAlive{i}", "Check if Entity is Alive", ref setting.Setting_IsAlive); ImGui.SameLine();
            ImGuiUtils.Checkbox($"##CustomPathOpened{i}", "Check if Entity is Opened", ref setting.Setting_IsOpened); ImGui.SameLine();  
            float inputTextWidth = ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize("Remove").X - ImGui.GetStyle().ItemSpacing.X;
            ImGui.SetNextItemWidth(inputTextWidth);
            ImGui.InputText($"##Path{i}", ref setting.Path, 100); ImGui.SameLine();
            ImGui.PopItemWidth();
            if (ImGui.Button($"Remove##Path{i}")) Settings.RemoveCustomIconSettings(i);
        }

        if (ImGui.Button("Add Icon")) Settings.NewCustomIconSettings(); ImGui.SameLine();
        if (ImGui.Button("Rebuild Icons")) IconBuilder.RebuildIcons(); 
    }
    private void DrawNpcIconSettings() {
        // Vector4 childBgColor = ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ChildBg));

        // normal monsters 
        ImGuiUtils.Checkbox("##NormalMonsters", "Draw Normal Monsters", ref Settings.WhiteMonster_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Normal", ref Settings.WhiteMonster_Tint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Normal", ref Settings.WhiteMonster_HiddenTint); ImGui.SameLine();
        IconButton("Normal Monster Icon", "Icon", ref Settings.WhiteMonster_Index, Settings.WhiteMonster_Tint); ImGui.SameLine();
        IconSizeSliderInt("##NormalMonsters", ref Settings.WhiteMonster_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Normal Monster");

        // magic monsters
        ImGuiUtils.Checkbox("##MagicMonsters", "Draw Magic Monsters", ref Settings.MagicMonster_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Magic", ref Settings.MagicMonster_Tint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Magic", ref Settings.MagicMonster_HiddenTint); ImGui.SameLine();
        IconButton("Magic Monster Icon", "Icon", ref Settings.MagicMonster_Index, Settings.MagicMonster_Tint); ImGui.SameLine();
        IconSizeSliderInt("##MagicMonsters", ref Settings.MagicMonster_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Magic Monster");

        // rare monsters
        ImGuiUtils.Checkbox("##RareMonsters", "Draw Rare Monsters", ref Settings.RareMonster_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Rare", ref Settings.RareMonster_Tint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Rare", ref Settings.RareMonster_HiddenTint); ImGui.SameLine();
        IconButton("Rare Monster Icon", "Icon", ref Settings.RareMonster_Index, Settings.RareMonster_Tint); ImGui.SameLine();
        IconSizeSliderInt("##RareMonsters", ref Settings.RareMonster_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Rare Monster");

        // unique monsters
        ImGuiUtils.Checkbox("##UniqueMonsters", "Draw Unique Monsters", ref Settings.UniqueMonster_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Unique", ref Settings.UniqueMonster_Tint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Unique", ref Settings.UniqueMonster_HiddenTint); ImGui.SameLine();
        IconButton("Unique Monster Icon", "Icon", ref Settings.UniqueMonster_Index, Settings.UniqueMonster_Tint); ImGui.SameLine();
        IconSizeSliderInt("##UniqueMonsters", ref Settings.UniqueMonster_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Unique Monster");

        // spirits
        ImGuiUtils.Checkbox("##Spirits", "Draw Spirits", ref Settings.Spirit_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Spirits", ref Settings.Spirit_Tint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Spirits", ref Settings.Spirit_HiddenTint); ImGui.SameLine();
        IconButton("Spirit Icon", "Icon", ref Settings.Spirit_Index, Settings.Spirit_Tint); ImGui.SameLine();
        IconSizeSliderInt("##Spirits", ref Settings.Spirit_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Spirit");

        // volatile cores
        ImGuiUtils.Checkbox("##VolatileCores", "Draw Volatile Cores", ref Settings.VolatileCore_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##VolatileCores", ref Settings.VolatileCore_Tint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##VolatileCores", ref Settings.VolatileCore_HiddenTint); ImGui.SameLine();
        IconButton("Volatile Core Icon", "Icon", ref Settings.VolatileCore_Index, Settings.VolatileCore_Tint); ImGui.SameLine();
        IconSizeSliderInt("##VolatileCores", ref Settings.VolatileCore_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Volatile Core");

        // fracturing mirrors
        ImGuiUtils.Checkbox("##FracturingMirrors", "Draw Fracturing Mirrors", ref Settings.FracturingMirror_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##FracturingMirrors", ref Settings.FracturingMirror_Tint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##FracturingMirrors", ref Settings.FracturingMirror_HiddenTint); ImGui.SameLine();
        IconButton("Fracturing Mirror Icon", "Icon", ref Settings.FracturingMirror_Index, Settings.FracturingMirror_Tint); ImGui.SameLine();
        IconSizeSliderInt("##FracturingMirrors", ref Settings.FracturingMirror_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Fracturing Mirror");

        // minions
        ImGuiUtils.Checkbox("##Minions", "Draw Minions", ref Settings.Minion_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Minions", ref Settings.Minion_Tint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Minions", ref Settings.Minion_HiddenTint); ImGui.SameLine();
        IconButton("Minion Icon", "Icon", ref Settings.Minion_Index, Settings.Minion_Tint); ImGui.SameLine();
        IconSizeSliderInt("##Minions", ref Settings.Minion_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Minion");

        // NPCs
        ImGuiUtils.Checkbox("##NPCs", "Draw NPCs", ref Settings.NPC_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##NPCs", ref Settings.NPC_Tint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##NPCs", ref Settings.NPC_HiddenTint); ImGui.SameLine();
        IconButton("NPC Icon", "Icon", ref Settings.NPC_Index, Settings.NPC_Tint); ImGui.SameLine();
        IconSizeSliderInt("##NPCs", ref Settings.NPC_Size, 0, 32); ImGui.SameLine();
        ImGuiUtils.Checkbox("##NPCText", "Draw NPC Name", ref Settings.NPC_DrawText); ImGui.SameLine();
        ImGui.Text("NPC");

    }
    private void DrawMiscIconSettings() {

        ImGuiUtils.Checkbox("##Players", "Draw Local Player", ref Settings.LocalPlayer_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Players", ref Settings.LocalPlayer_Tint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Players", ref Settings.NPC_HiddenTint); ImGui.SameLine();
        IconButton("Local Player Icon", "Icon", ref Settings.LocalPlayer_Index, Settings.LocalPlayer_Tint); ImGui.SameLine();
        IconSizeSliderInt("##Players", ref Settings.LocalPlayer_Size, 0, 32); ImGui.SameLine();
        ImGuiUtils.Checkbox("##PlayerText", "Draw Local Player Name", ref Settings.LocalPlayer_DrawText); ImGui.SameLine();
        ImGui.Text("Local Player");

        ImGuiUtils.Checkbox("##Players", "Draw Players", ref Settings.Player_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Players", ref Settings.Player_Tint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##Players", ref Settings.NPC_HiddenTint); ImGui.SameLine();
        IconButton("Player Icon", "Icon", ref Settings.Player_Index, Settings.Player_Tint); ImGui.SameLine();
        IconSizeSliderInt("##Players", ref Settings.Player_Size, 0, 32); ImGui.SameLine();
        ImGuiUtils.Checkbox("##PlayerText", "Draw Player Names", ref Settings.Player_DrawText); ImGui.SameLine();
        ImGui.Text("Players");

        ImGuiUtils.Checkbox("##SanctumMote", "Draw Sacred Water", ref Settings.SanctumMote_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##SanctumMote", ref Settings.SanctumMote_Tint); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Hidden Icon Tint ##SanctumMote", ref Settings.SanctumMote_HiddenTint); ImGui.SameLine();
        IconButton("Sacred Water Icon", "Icon", ref Settings.SanctumMote_Index, Settings.SanctumMote_Tint); ImGui.SameLine();
        IconSizeSliderInt("##SanctumMote", ref Settings.SanctumMote_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Sacred Water");

    }
    private void DrawChestSettings() {

        // normal chest
        ImGuiUtils.Checkbox("##NormalChest", "Draw Normal Chest", ref Settings.NormalChest_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##NormalChest", ref Settings.NormalChest_Tint); ImGui.SameLine();
        IconButton("Normal Chest Icon", "Icon", ref Settings.NormalChest_Index, Settings.NormalChest_Tint); ImGui.SameLine();
        IconSizeSliderInt("##NormalChest", ref Settings.NormalChest_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Normal Chest");

        // magic chest
        ImGuiUtils.Checkbox("##MagicChest", "Draw Magic Chest", ref Settings.MagicChest_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##MagicChest", ref Settings.MagicChest_Tint); ImGui.SameLine();
        IconButton("Magic Chest Icon", "Icon", ref Settings.MagicChest_Index, Settings.MagicChest_Tint); ImGui.SameLine();
        IconSizeSliderInt("##MagicChest", ref Settings.MagicChest_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Magic Chest");

        // rare chest
        ImGuiUtils.Checkbox("##RareChest", "Draw Rare Chest", ref Settings.RareChest_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##RareChest", ref Settings.RareChest_Tint); ImGui.SameLine();
        IconButton("Rare Chest Icon", "Icon", ref Settings.RareChest_Index, Settings.RareChest_Tint); ImGui.SameLine();
        IconSizeSliderInt("##RareChest", ref Settings.RareChest_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Rare Chest");

        // Breach Hand
        ImGuiUtils.Checkbox("##BreachHand", "Draw Breach Hand", ref Settings.BreachChest_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##BreachHand", ref Settings.BreachChest_Tint); ImGui.SameLine();
        IconButton("Breach Hand Icon", "Icon", ref Settings.BreachChest_Index, Settings.BreachChest_Tint); ImGui.SameLine();
        IconSizeSliderInt("##BreachHand", ref Settings.BreachChest_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Breach Hand");

        // Breach Boss Hand
        ImGuiUtils.Checkbox("##BreachBossHand", "Draw Breach Boss Hand", ref Settings.BreachChestBoss_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##BreachBossHand", ref Settings.BreachChestBoss_Tint); ImGui.SameLine();
        IconButton("Breach Boss Hand Icon", "Icon", ref Settings.BreachChestBoss_Index, Settings.BreachChestBoss_Tint); ImGui.SameLine();
        IconSizeSliderInt("##BreachBossHand", ref Settings.BreachChestBoss_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Breach Boss Hand");

        //expedition chest normal
        ImGuiUtils.Checkbox("##ExpeditionNormalChest", "Draw Expedition Normal Chest", ref Settings.ExpeditionNormalChest_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ExpeditionNormalChest", ref Settings.ExpeditionNormalChest_Tint); ImGui.SameLine();
        IconButton("Expedition Normal Chest Icon", "Icon", ref Settings.ExpeditionNormalChest_Index, Settings.ExpeditionNormalChest_Tint); ImGui.SameLine();
        IconSizeSliderInt("##ExpeditionNormalChest", ref Settings.ExpeditionNormalChest_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Expedition Normal Chest");

        //expedition chest magic
        ImGuiUtils.Checkbox("##ExpeditionMagicChest", "Draw Expedition Magic Chest", ref Settings.ExpeditionMagicChest_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ExpeditionMagicChest", ref Settings.ExpeditionMagicChest_Tint); ImGui.SameLine();
        IconButton("Expedition Magic Chest Icon", "Icon", ref Settings.ExpeditionMagicChest_Index, Settings.ExpeditionMagicChest_Tint); ImGui.SameLine();
        IconSizeSliderInt("##ExpeditionMagicChest", ref Settings.ExpeditionMagicChest_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Expedition Magic Chest");

        //expedition chest rare
        ImGuiUtils.Checkbox("##ExpeditionRareChest", "Draw Expedition Rare Chest", ref Settings.ExpeditionRareChest_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ExpeditionRareChest", ref Settings.ExpeditionRareChest_Tint); ImGui.SameLine();
        IconButton("Expedition Rare Chest Icon", "Icon", ref Settings.ExpeditionRareChest_Index, Settings.ExpeditionRareChest_Tint); ImGui.SameLine();
        IconSizeSliderInt("##ExpeditionRareChest", ref Settings.ExpeditionRareChest_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Expedition Rare Chest");

        // snactum chest
        ImGuiUtils.Checkbox("##SanctumChest", "Draw Sanctum Chest", ref Settings.SanctumChest_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##SanctumChest", ref Settings.SanctumChest_Tint); ImGui.SameLine();
        IconButton("Sanctum Chest Icon", "Icon", ref Settings.SanctumChest_Index, Settings.SanctumChest_Tint); ImGui.SameLine();
        IconSizeSliderInt("##SanctumChest", ref Settings.SanctumChest_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Sanctum Chest");

    }
    private void DrawStrongboxSettings() {

        ImGuiUtils.Checkbox("##UnknownStrongbox", "Draw Unknown Strongbox", ref Settings.UnknownStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##UnknownStrongbox", ref Settings.UnknownStrongbox_Tint); ImGui.SameLine();
        IconButton("Unknown Strongbox Icon", "Icon", ref Settings.UnknownStrongbox_Index, Settings.UnknownStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##UnknownStrongbox", ref Settings.UnknownStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Unknown Strongbox");

        ImGuiUtils.Checkbox("##ArcanistStrongbox", "Draw Arcanist Strongbox", ref Settings.ArcanistStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ArcanistStrongbox", ref Settings.ArcanistStrongbox_Tint); ImGui.SameLine();
        IconButton("Arcanist Strongbox Icon", "Icon", ref Settings.ArcanistStrongbox_Index, Settings.ArcanistStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##ArcanistStrongbox", ref Settings.ArcanistStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Arcanist Strongbox");

        ImGuiUtils.Checkbox("##ArmourerStrongbox", "Draw Armourer Strongbox", ref Settings.ArmourerStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ArmourerStrongbox", ref Settings.ArmourerStrongbox_Tint); ImGui.SameLine();
        IconButton("Armourer Strongbox Icon", "Icon", ref Settings.ArmourerStrongbox_Index, Settings.ArmourerStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##ArmourerStrongbox", ref Settings.ArmourerStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Armourer Strongbox");

        ImGuiUtils.Checkbox("##BlacksmithStrongbox", "Draw Blacksmith Strongbox", ref Settings.BlacksmithStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##BlacksmithStrongbox", ref Settings.BlacksmithStrongbox_Tint); ImGui.SameLine();
        IconButton("Blacksmith Strongbox Icon", "Icon", ref Settings.BlacksmithStrongbox_Index, Settings.BlacksmithStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##BlacksmithStrongbox", ref Settings.BlacksmithStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Blacksmith Strongbox");

        ImGuiUtils.Checkbox("##ArtisanStrongbox", "Draw Artisan Strongbox", ref Settings.ArtisanStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ArtisanStrongbox", ref Settings.ArtisanStrongbox_Tint); ImGui.SameLine();
        IconButton("Artisan Strongbox Icon", "Icon", ref Settings.ArtisanStrongbox_Index, Settings.ArtisanStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##ArtisanStrongbox", ref Settings.ArtisanStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Artisan Strongbox");

        ImGuiUtils.Checkbox("##CartographerStrongbox", "Draw Cartographer Strongbox", ref Settings.CartographerStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##CartographerStrongbox", ref Settings.CartographerStrongbox_Tint); ImGui.SameLine();
        IconButton("Cartographer Strongbox Icon", "Icon", ref Settings.CartographerStrongbox_Index, Settings.CartographerStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##CartographerStrongbox", ref Settings.CartographerStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Cartographer Strongbox");

        ImGuiUtils.Checkbox("##ChemistStrongbox", "Draw Chemist Strongbox", ref Settings.ChemistStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ChemistStrongbox", ref Settings.ChemistStrongbox_Tint); ImGui.SameLine();
        IconButton("Chemist Strongbox Icon", "Icon", ref Settings.ChemistStrongbox_Index, Settings.ChemistStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##ChemistStrongbox", ref Settings.ChemistStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Chemist Strongbox");

        ImGuiUtils.Checkbox("##GemcutterStrongbox", "Draw Gemcutter Strongbox", ref Settings.GemcutterStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##GemcutterStrongbox", ref Settings.GemcutterStrongbox_Tint); ImGui.SameLine();
        IconButton("Gemcutter Strongbox Icon", "Icon", ref Settings.GemcutterStrongbox_Index, Settings.GemcutterStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##GemcutterStrongbox", ref Settings.GemcutterStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Gemcutter Strongbox");

        ImGuiUtils.Checkbox("##JewellerStrongbox", "Draw Jeweller Strongbox", ref Settings.JewellerStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##JewellerStrongbox", ref Settings.JewellerStrongbox_Tint); ImGui.SameLine();
        IconButton("Jeweller Strongbox Icon", "Icon", ref Settings.JewellerStrongbox_Index, Settings.JewellerStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##JewellerStrongbox", ref Settings.JewellerStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Jeweller Strongbox");

        ImGuiUtils.Checkbox("##LargeStrongbox", "Draw Large Strongbox", ref Settings.LargeStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##LargeStrongbox", ref Settings.LargeStrongbox_Tint); ImGui.SameLine();
        IconButton("Large Strongbox Icon", "Icon", ref Settings.LargeStrongbox_Index, Settings.LargeStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##LargeStrongbox", ref Settings.LargeStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Large Strongbox");

        ImGuiUtils.Checkbox("##OrnateStrongbox", "Draw Ornate Strongbox", ref Settings.OrnateStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##OrnateStrongbox", ref Settings.OrnateStrongbox_Tint); ImGui.SameLine();
        IconButton("Ornate Strongbox Icon", "Icon", ref Settings.OrnateStrongbox_Index, Settings.OrnateStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##OrnateStrongbox", ref Settings.OrnateStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Ornate Strongbox");

        ImGuiUtils.Checkbox("##Strongbox", "Draw Strongbox", ref Settings.Strongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##Strongbox", ref Settings.Strongbox_Tint); ImGui.SameLine();
        IconButton("Strongbox Icon", "Icon", ref Settings.Strongbox_Index, Settings.Strongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##Strongbox", ref Settings.Strongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Strongbox");

        ImGuiUtils.Checkbox("##DivinerStrongbox", "Draw Diviner Strongbox", ref Settings.DivinerStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##DivinerStrongbox", ref Settings.DivinerStrongbox_Tint); ImGui.SameLine();
        IconButton("Diviner Strongbox Icon", "Icon", ref Settings.DivinerStrongbox_Index, Settings.DivinerStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##DivinerStrongbox", ref Settings.DivinerStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Diviner Strongbox");

        ImGuiUtils.Checkbox("##OperativeStrongbox", "Draw Operative Strongbox", ref Settings.OperativeStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##OperativeStrongbox", ref Settings.OperativeStrongbox_Tint); ImGui.SameLine();
        IconButton("Operative Strongbox Icon", "Icon", ref Settings.OperativeStrongbox_Index, Settings.OperativeStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##OperativeStrongbox", ref Settings.OperativeStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Operative Strongbox");

        ImGuiUtils.Checkbox("##ArcaneStrongbox", "Draw Arcane Strongbox", ref Settings.ArcaneStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ArcaneStrongbox", ref Settings.ArcaneStrongbox_Tint); ImGui.SameLine();
        IconButton("Arcane Strongbox Icon", "Icon", ref Settings.ArcaneStrongbox_Index, Settings.ArcaneStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##ArcaneStrongbox", ref Settings.ArcaneStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Arcane Strongbox");

        ImGuiUtils.Checkbox("##ResearcherStrongbox", "Draw Researcher Strongbox", ref Settings.ResearcherStrongbox_Draw); ImGui.SameLine();
        ImGuiUtils.ColorSwatch("Icon Tint ##ResearcherStrongbox", ref Settings.ResearcherStrongbox_Tint); ImGui.SameLine();
        IconButton("Researcher Strongbox Icon", "Icon", ref Settings.ResearcherStrongbox_Index, Settings.ResearcherStrongbox_Tint); ImGui.SameLine();
        IconSizeSliderInt("##ResearcherStrongbox", ref Settings.ResearcherStrongbox_Size, 0, 32); ImGui.SameLine();
        ImGui.Text("Researcher Strongbox");
    }
    private void DrawIngameIconsSettings() {
        IngameIconComboBox("##AreaTransition", ref Settings.AreaTransition_State); ImGui.SameLine();
        ImGui.Text("Area Transition");

        IngameIconComboBox("##Breach", ref Settings.Breach_State); ImGui.SameLine();
        ImGui.Text("Breach");

        IngameIconComboBox("##Checkpoint", ref Settings.Checkpoint_State); ImGui.SameLine();
        ImGui.Text("Checkpoint");

        IngameIconComboBox("##QuestObject", ref Settings.QuestObject_State); ImGui.SameLine();
        ImGui.Text("Quest Object");

        IngameIconComboBox("##NPC", ref Settings.IngameNPC_State); ImGui.SameLine();
        ImGui.Text("NPC");

        IngameIconComboBox("##Ritual", ref Settings.Ritual_State); ImGui.SameLine();
        ImGui.Text("Ritual");

        IngameIconComboBox("##Shrine", ref Settings.Shrine_State); ImGui.SameLine();
        ImGuiUtils.Checkbox("##ShrineText","Show Shrine Name", ref Settings.Shrine_DrawText); ImGui.SameLine();
        ImGui.Text("Shrine");

        IngameIconComboBox("##Waypoint", ref Settings.Waypoint_State); ImGui.SameLine();
        ImGui.Text("Waypoint");

        IngameIconComboBox("##Uncategorized", ref Settings.IngameUncategorized_State); ImGui.SameLine();
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
        ImGui.Checkbox("Realtime Icon Settings", ref Settings.RealtimeIconSettings);
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("When enabled, any changes made to icons through the UI\n"
                             + "will be immediately reflected in the rendered icons\n"
                             + "without needing to rebuild.\n"
                             + "Disable this after setup to improve performance.");
        }
        if (ImGuiUtils.CollapsingHeader("Draw Settings", ref Settings.DrawSetingsOpen)) {
            ImGui.Indent();
            ImGuiUtils.Checkbox("Draw on Minimap", "Draw Monsters on the minimap", ref Settings.DrawOnMinimap);
            ImGuiUtils.Checkbox("Draw cached Entities", "Draw entities that are cached but no longer in proximity", ref Settings.DrawCachedEntities);
            ImGuiUtils.Checkbox("Draw Over Large Panels", "Enable drawing over large panels", ref Settings.DrawOverLargePanels);
            ImGuiUtils.Checkbox("Draw Over Fullscreen Panels", "Enable drawing over fullscreen panels", ref Settings.DrawOverFullscreenPanels);
            ImGui.Unindent();
        }
        if (ImGuiUtils.CollapsingHeader("Ignored Entities", ref Settings.IgnoredEntitesOpen)) {
            ImGui.Indent();
            if (ImGui.Button("Update")) { IconBuilder.UpdateUserSkippedEntities(); }
            ImGui.SameLine();
            ImGui.SliderInt("Height", ref Settings.IgnoredEntitiesHeight, 100, 1000);
            ImGui.InputTextMultiline("##ignoredEntitiesInput", ref Settings.ignoredEntities, 1000, new Vector2(ImGui.GetContentRegionAvail().X, Settings.IgnoredEntitiesHeight));
            ImGui.Unindent();
        };
        if (ImGuiUtils.CollapsingHeader("Custom Path Icons", ref Settings.CustomIconsOpen)) {
            ImGui.Indent();
            DrawCustomIconPathSettings();
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
    /*

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
                    if (Settings.AreaTransition_State == 0 || (Settings.AreaTransition_State == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                case IngameIconTypes.Breach:
                    if (Settings.Breach_State == 0 || (Settings.Breach_State == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                case IngameIconTypes.Checkpoint: 
                    if (Settings.Checkpoint_State == 0 || (Settings.Checkpoint_State == 1 && ingame_icon.Entity.IsValid)) return false;                    
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                case IngameIconTypes.QuestObject:
                    if (Settings.QuestObject_State == 0 || (Settings.QuestObject_State == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                case IngameIconTypes.NPC:
                    if (Settings.NPC_State == 0 || (Settings.NPC_State == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                case IngameIconTypes.Ritual:
                    if (Settings.Ritual_State == 0 || (Settings.Ritual_State == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    if (ingame_icon.Entity.GetComponent<MinimapIcon>()?.Name == "RitualRuneFinished")
                        iconColor = ImGuiUtils.Vector4ToColor(new Vector4(.8f,.8f,.8f,1));
                    break;
                case IngameIconTypes.Shrine:
                    if (Settings.Shrine_State == 0 || (Settings.Shrine_State == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    showText = Settings.Shrine_DrawText;
                    break;
                case IngameIconTypes.Waypoint:
                    if (Settings.Waypoint_State == 0 || (Settings.Waypoint_State == 1 && ingame_icon.Entity.IsValid)) return false;
                    iconSize = (int)ingame_icon.InGameTexture.Size;
                    iconUV = ingame_icon.InGameTexture.UV;
                    break;
                case IngameIconTypes.Uncategorized:
                    if (Settings.Uncategorized_State == 0 || (Settings.Uncategorized_State == 1 && ingame_icon.Entity.IsValid)) return false;
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
    */
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

        var mapIcons = _iconListCache.Value;
        if (mapIcons == null) return;

        foreach (var icon in mapIcons) {
            //if (!icon.IsValid) continue;
            if (icon?.Entity == null) continue;
            if (!icon.Show()) continue;

            //if (!GetIconProperties(icon, out var iconFileName, out var iconSize, out var iconColor, out var iconUV, out var showText)) continue;

            string iconFileName = null;
            var iconSize = 0;
            var iconColor = System.Drawing.Color.White;
            var iconUV = new RectangleF();
            var showText = false;

            if (Settings.RealtimeIconSettings) { icon.UpdateSettings();}

            if (icon.Check_IsAlive && !icon.Entity.IsAlive) continue;
            if (icon.Check_IsOpened && icon.Entity.IsOpened) continue;

            if (icon.IconRenderType == IconRenderTypes.IngameIcon) {
                if (icon.DrawState == 0 || (icon.DrawState == 1 && icon.Entity.IsValid)) continue;
                iconFileName = icon.InGameTexture.FileName;
                iconSize = (int)icon.InGameTexture.Size;
                iconColor = System.Drawing.Color.White;
                iconUV = icon.InGameTexture.UV;
                showText = icon.DrawText;
            }
            else if (icon.IconRenderType == IconRenderTypes.Monster) {
                if (!icon.Draw) continue;
                iconFileName = _iconAtlas.Name;
                iconSize = icon.Size;
                iconUV = _iconAtlas.GetIconUV(icon.Index);
                iconColor = icon.Hidden() ? ImGuiUtils.Vector4ToColor(icon.HiddenTint) : ImGuiUtils.Vector4ToColor(icon.Tint);
                showText = icon.DrawText;
            }
            else if (icon.IconRenderType == IconRenderTypes.Chest) {
                if (!icon.Draw) continue;
                iconFileName = _iconAtlas.Name;
                iconSize = icon.Size;
                iconUV = _iconAtlas.GetIconUV(icon.Index);
                iconColor = ImGuiUtils.Vector4ToColor(icon.Tint);
                showText = icon.DrawText;
            }
            else if (icon.IconRenderType == IconRenderTypes.Player) {
                if (!icon.Draw) continue;
                iconFileName = _iconAtlas.Name;
                iconSize = icon.Size;
                iconUV = _iconAtlas.GetIconUV(icon.Index);
                iconColor = ImGuiUtils.Vector4ToColor(icon.Tint);
                showText = icon.DrawText;
            }
            else {
                continue;
            }

            if (iconFileName == null) continue;

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
        ImGui.Checkbox("NPC", ref Settings.DebugNPCIcon); ImGui.SameLine();
        ImGui.Checkbox("Chest", ref Settings.DebugChestIcon); ImGui.SameLine();
        ImGui.Checkbox("Ingame", ref Settings.DebugIngameIcon); ImGui.SameLine();
        ImGui.Checkbox("Misc", ref Settings.DebugMiscIcon); ImGui.SameLine();
        if (ImGui.Button("Rebuild Icons")) IconBuilder.RebuildIcons();
    }

}