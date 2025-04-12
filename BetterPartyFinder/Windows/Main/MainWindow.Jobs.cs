using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BetterPartyFinder.Windows.Main;

public partial class MainWindow
{
    private void DrawJobsTab(ConfigurationFilter filter)
    {
        if (ImGui.Button("添加职业栏位"))
        {
            filter.Jobs.Add(0);
            Plugin.Config.Save();
        }

        var toRemove = new HashSet<int>();
        for (var i = 0; i < filter.Jobs.Count; i++)
        {
            var slot = filter.Jobs[i];

            if (!ImGui.CollapsingHeader($"栏位 {i + 1}"))
                continue;

            if (ImGui.Button("全选"))
            {
                filter.Jobs[i] = Enum.GetValues<JobFlags>().Aggregate(slot, (current, job) => current | job);
                Plugin.Config.Save();
            }

            ImGui.SameLine();

            if (ImGui.Button("清空"))
            {
                filter.Jobs[i] = 0;
                Plugin.Config.Save();
            }

            ImGui.SameLine();

            if (ImGui.Button("删除"))
                toRemove.Add(i);

            using var table = ImRaii.Table($"JobTable{i}", 2, ImGuiTableFlags.BordersInnerV);
            if (!table.Success)
                return;

            ImGui.TableSetupColumn($"##Selected{i}");
            ImGui.TableSetupColumn($"##Add{i}");

            ImGui.TableNextColumn();
            Helper.TextColored(ImGuiColors.HealerGreen, "已选择:");
            ImGui.Separator();

            ImGui.TableNextColumn();
            Helper.TextColored(ImGuiColors.ParsedOrange, "可用:");
            ImGui.Separator();

            ImGui.TableNextColumn();
            using var id = ImRaii.PushId(i);
            foreach (var job in Enum.GetValues<JobFlags>().Where(j => (slot & j) > 0))
            {
                if (!ImGui.Selectable(job.ClassJob(Plugin.Data)?.Name.UpperCaseStr() ?? "???"))
                    continue;

                slot &= ~job;

                filter.Jobs[i] = slot;
                Plugin.Config.Save();
            }

            ImGui.TableNextColumn();
            foreach (var job in Enum.GetValues<JobFlags>().Where(j => (slot & j) == 0))
            {
                if (!ImGui.Selectable(job.ClassJob(Plugin.Data)?.Name.UpperCaseStr() ?? "???"))
                    continue;

                slot |= job;

                filter.Jobs[i] = slot;
                Plugin.Config.Save();
            }
        }

        foreach (var idx in toRemove)
            filter.Jobs.RemoveAt(idx);

        if (toRemove.Count > 0)
            Plugin.Config.Save();
    }
}