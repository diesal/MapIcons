using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MapIcons;

/// <summary>
/// Provides helper methods for ImGui controls.
/// </summary>
public static class ImGuiUtils
{
    /// <summary>
    /// Renders a checkbox with an optional tooltip.
    /// </summary>
    /// <param name="label">The label for the checkbox.</param>
    /// <param name="tooltip">\Ttooltip text to display when the checkbox is hovered.</param>
    /// <param name="value">The boolean value of the checkbox.</param>
    public static void Checkbox(string label, string tooltip, ref bool value) {
        ImGui.Checkbox(label, ref value);
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.Text(tooltip);
            ImGui.EndTooltip();
        }
    }
    private static Vector2 _popupOffset = new(10, 10);
    public static void ColorSwatch(string desc_id, ref Vector4 color) {
        if (ImGui.ColorButton(desc_id, color)) {
            ImGui.SetNextWindowPos(ImGui.GetMousePos() + _popupOffset, ImGuiCond.Always);
            ImGui.OpenPopup(desc_id);
        }
        if (ImGui.BeginPopup(desc_id)) {
            ImGui.ColorPicker4(desc_id, ref color);
            ImGui.EndPopup();
        }
    }

    public static bool CollapsingHeader(string label, ref bool open) {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None;
        if (open) flags |= ImGuiTreeNodeFlags.DefaultOpen;

        open = ImGui.CollapsingHeader(label, flags);

        return open;
    }

    private static readonly Vector4 _defaultTint = new Vector4(1f, 1f, 1f, 1f);

    /// <summary>
    /// Shows an icon picker window.
    /// </summary>
    /// <param name="name">The name of the window.</param>
    /// <param name="selectedIcon">The currently selected icon index.</param>
    /// <param name="iconAtlas">The icon atlas to use for the picker.</param>
    /// <param name="tint">The optional tint to apply to the icons.</param>
    /// <returns>True if the window is open </returns>
    public static bool IconPickerWindow(string name, ref int selectedIcon, IconAtlas iconAtlas, Vector4? tint = null) {
        if (iconAtlas.TextureId == IntPtr.Zero) return false;

        var show = true;
        int iconsPerRow = (int)(iconAtlas.AtlasSize.X / iconAtlas.IconSize.X); // calculate the number of icons per row
        int iconsPerColumn = (int)(iconAtlas.AtlasSize.Y / iconAtlas.IconSize.Y); // calculate the number of icons per column

        uint childBgColor = ImGui.GetColorU32(ImGuiCol.ChildBg);
        uint buttonBgColor = ImGui.GetColorU32(ImGuiCol.Button);

        Vector2 windowPadding = ImGui.GetStyle().WindowPadding; // get the default window padding

        // Temporarily set frame padding to zero
        Vector2 originalFramePadding = ImGui.GetStyle().FramePadding;
        ImGui.GetStyle().FramePadding = Vector2.Zero;

        float titleBarHeight = ImGui.GetFrameHeight();

        Vector2 windowSize = new Vector2(
            iconAtlas.AtlasSize.X + windowPadding.X * 2,
            iconAtlas.AtlasSize.Y + windowPadding.Y * 2 + titleBarHeight
        );  // calculate the window size

        ImGui.Begin($"{name}###IconPickerWindow", ref show, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar);

        Vector2 cursorPos = ImGui.GetCursorPos();

        for (int y = 0; y < iconsPerColumn; y++) {
            for (int x = 0; x < iconsPerRow; x++) {
                int iconIndex = y * iconsPerRow + x; // Calculate UV coordinates for the current icon
                (Vector2 uv0, Vector2 uv1) = iconAtlas.GetIconUVs(iconIndex);

                ImGui.PushStyleColor(ImGuiCol.Button, iconIndex == selectedIcon ? buttonBgColor : childBgColor); // Set button background color
                ImGui.SetCursorPos(new Vector2(cursorPos.X + x * iconAtlas.IconSize.X, cursorPos.Y + y * iconAtlas.IconSize.Y));
                if (ImGui.ImageButton($"##textureID_{y}_{x}", iconAtlas.TextureId, iconAtlas.IconSize, uv0, uv1, Vector4.Zero, tint ?? _defaultTint )) {
                    selectedIcon = iconIndex;
                    show = false;
                }
                ImGui.PopStyleColor();

                if (x < iconsPerRow - 1) ImGui.SameLine(0, 0);
            }
        }

        // Restore original frame padding
        ImGui.GetStyle().FramePadding = originalFramePadding;
        ImGui.End();
        return show;
    }



    public static System.Drawing.Color Vector4ToColor(Vector4 vector) {
        // Ensure the vector components are in the range [0, 1]
        vector = Vector4.Clamp(vector, Vector4.Zero, Vector4.One);

        // Convert the vector components to a Color
        int alpha = (int)(vector.W * 255);
        int red = (int)(vector.X * 255);
        int green = (int)(vector.Y * 255);
        int blue = (int)(vector.Z * 255);

        return System.Drawing.Color.FromArgb(alpha, red, green, blue);
    }

}
