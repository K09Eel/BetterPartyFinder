using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BetterPartyFinder.Windows.Main;

public partial class MainWindow
{
    private string KeyWords = string.Empty;

    private void DrawKeywordsTab(ConfigurationFilter filter)
    {
        var player = Plugin.ClientState.LocalPlayer;
        if (player == null)
            return;

        ImGui.PushItemWidth(ImGui.GetWindowWidth() / 3f);
        ImGui.InputText("###key-words", ref KeyWords, 64);

        ImGui.PopItemWidth();

        ImGui.SameLine();

        if (Helper.IconButton(FontAwesomeIcon.Plus, "add-keywords"))
        {
            var word = KeyWords.Trim();
            if (word.Length != 0)
            {
                filter.Keywords.Add(word);
                Plugin.Config.Save();
            }
            KeyWords = string.Empty;
        }

        string? deleting = null;
        foreach (var word in filter.Keywords)
        {
            ImGui.TextUnformatted($"{word}");
            ImGui.SameLine();
            if (Helper.IconButton(FontAwesomeIcon.Trash, $"delete-keyword-{word.GetHashCode()}"))
                deleting = word;
        }

        if (deleting != null)
        {
            filter.Keywords.Remove(deleting);
            Plugin.Config.Save();
        }
    }
}