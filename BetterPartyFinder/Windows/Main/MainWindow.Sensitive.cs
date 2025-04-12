using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BetterPartyFinder.Windows.Main;

public partial class MainWindow
{
    private string ssWord = string.Empty;

    private void DrawSensitiveTab(ConfigurationFilter filter)
    {
        var player = Plugin.ClientState.LocalPlayer;
        if (player == null)
            return;

        ImGui.PushItemWidth(ImGui.GetWindowWidth() / 3f);
        ImGui.InputText("###ss-words", ref ssWord, 64);

        ImGui.PopItemWidth();

        ImGui.SameLine();

        if (Helper.IconButton(FontAwesomeIcon.Plus, "add-ssWords"))
        {
            var word = ssWord.Trim();
            if (word.Length != 0)
            {
                filter.SensitiveWords.Add(word);
                Plugin.Config.Save();
            }
            ssWord = string.Empty;
        }

        string? deleting = null;
        foreach (var word in filter.SensitiveWords)
        {
            ImGui.TextUnformatted($"{word}");
            ImGui.SameLine();
            if (Helper.IconButton(FontAwesomeIcon.Trash, $"delete-ssWords-{word.GetHashCode()}"))
                deleting = word;
        }

        if (deleting != null)
        {
            filter.SensitiveWords.Remove(deleting);
            Plugin.Config.Save();
        }
    }
}