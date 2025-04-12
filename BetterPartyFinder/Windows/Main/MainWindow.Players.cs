using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BetterPartyFinder.Windows.Main;

public partial class MainWindow
{
    private int SelectedWorld;
    private string PlayerName = string.Empty;

    private void DrawPlayersTab(ConfigurationFilter filter)
    {
        var player = Plugin.ClientState.LocalPlayer;
        if (player == null)
            return;

        ImGui.PushItemWidth(ImGui.GetWindowWidth() / 3f);
        ImGui.InputText("###player-name", ref PlayerName, 64);

        ImGui.SameLine();

        var worlds = Util.WorldsOnDataCentre(player).OrderByDescending(world => world.DataCenter.RowId).ThenBy(world => world.Name.ExtractText()).ToList();

        using (var combo = ImRaii.Combo("###player-world", worlds[SelectedWorld].Name.ExtractText()))
        {
            if (combo.Success)
            {
                var lastDc = worlds.First().DataCenter.RowId;
                foreach (var (world, idx) in worlds.WithIndex())
                {
                    if (ImGui.Selectable(world.Name.ExtractText(), SelectedWorld == idx))
                        SelectedWorld = idx;

                    if (lastDc != world.DataCenter.RowId)
                    {
                        lastDc = world.DataCenter.RowId;
                        ImGui.Separator();
                    }
                }
            }
        }

        ImGui.PopItemWidth();

        ImGui.SameLine();

        if (Helper.IconButton(FontAwesomeIcon.Plus, "add-player"))
        {
            var name = PlayerName.Trim();
            if (name.Length != 0)
            {
                filter.Players.Add(new PlayerInfo(name, worlds[SelectedWorld].RowId));
                Plugin.Config.Save();
            }
        }

        PlayerInfo? deleting = null;
        foreach (var info in filter.Players)
        {
            var world = Sheets.WorldSheet.GetRow(info.World);
            ImGui.TextUnformatted($"{info.Name}@{world.Name.ExtractText()}");
            ImGui.SameLine();
            if (Helper.IconButton(FontAwesomeIcon.Trash, $"delete-player-{info.GetHashCode()}"))
                deleting = info;
        }

        if (deleting != null)
        {
            filter.Players.Remove(deleting);
            Plugin.Config.Save();
        }
    }
}