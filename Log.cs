using ImGuiNET;
using System.Numerics;
using System.Text;

namespace MapIcons;

public static class Log
{
    private static readonly Queue<(DateTime Date, string Description)> _logEntries = new();
    private static string _logText = string.Empty;
    private static int _maxLogEntries = 100; // Limit to the most recent 100 entries
    private static int _bufferSize = 65536; // 64 KB buffer size

    private static Action _customControlsAction;

    public static void Write(string entry) {
        _logEntries.Enqueue((DateTime.Now, entry));
        if (_logEntries.Count > _maxLogEntries) {
            _logEntries.Dequeue(); // Remove the oldest entry
        }
        UpdateLogText();
    }

    public static void Render(ref bool showLogWindow) {
        if (showLogWindow) {
            ImGui.Begin("MapIcons Debug Log", ref showLogWindow);

            if (ImGui.Button("Clear History")) {
                _logEntries.Clear();
                UpdateLogText();
            }
            _customControlsAction?.Invoke();

            ImGui.BeginChild("Log");
            ImGui.InputTextMultiline("##logText", ref _logText, (uint)_bufferSize, new Vector2(-1, -1), ImGuiInputTextFlags.ReadOnly);

            ImGui.EndChild();
            ImGui.End();
        }
    }

    public static void SetCustomHeaderControls(Action customControlsAction) {
        _customControlsAction = customControlsAction;
    }

    private static void UpdateLogText() {
        var logText = new StringBuilder();
        foreach (var (dateTime, description) in _logEntries) {
            logText.AppendLine($"{dateTime:HH:mm:ss.fff}: {description}");
        }
        _logText = logText.ToString();
    }
}