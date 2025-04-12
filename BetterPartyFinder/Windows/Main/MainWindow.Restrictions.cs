using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BetterPartyFinder.Windows.Main;

public partial class MainWindow
{
    private bool Save;

    private void DrawRestrictionsTab(ConfigurationFilter filter)
    {
        using var table = ImRaii.Table("CategoryTable", 2, ImGuiTableFlags.BordersInnerV);
        if (!table.Success)
            return;

        Save = false;

        ImGui.TableSetupColumn("##Show");
        ImGui.TableSetupColumn("##Hide");

        ImGui.TableNextColumn();
        Helper.TextColored(ImGuiColors.HealerGreen, "展示:");
        ImGui.Separator();

        ImGui.TableNextColumn();
        Helper.TextColored(ImGuiColors.ParsedOrange, "隐藏:");
        ImGui.Separator();

        filter[ObjectiveFlags.Practice] = DrawRestrictionEntry("练习", filter[ObjectiveFlags.Practice]);
        filter[ObjectiveFlags.DutyCompletion] = DrawRestrictionEntry("完成任务", filter[ObjectiveFlags.DutyCompletion]);
        filter[ObjectiveFlags.Loot] = DrawRestrictionEntry("反复攻略", filter[ObjectiveFlags.Loot]);

        DrawSeparator();

        filter[ConditionFlags.None] = DrawRestrictionEntry("无", filter[ConditionFlags.None]);
        filter[ConditionFlags.DutyIncomplete] = DrawRestrictionEntry("任务未完成", filter[ConditionFlags.DutyIncomplete]);
        filter[ConditionFlags.DutyComplete] = DrawRestrictionEntry("任务已完成", filter[ConditionFlags.DutyComplete]);
        filter[ConditionFlags.DutyCompleteWeeklyRewardUnclaimed] = DrawRestrictionEntry("任务已完成（本周的周常奖励：未获得）", filter[ConditionFlags.DutyCompleteWeeklyRewardUnclaimed]);

        DrawSeparator();

        filter[DutyFinderSettingsFlags.UndersizedParty] = DrawRestrictionEntry("解除限制", filter[DutyFinderSettingsFlags.UndersizedParty]);
        filter[DutyFinderSettingsFlags.MinimumItemLevel] = DrawRestrictionEntry("最低品级", filter[DutyFinderSettingsFlags.MinimumItemLevel]);
        filter[DutyFinderSettingsFlags.SilenceEcho] = DrawRestrictionEntry("超越之力无效化", filter[DutyFinderSettingsFlags.SilenceEcho]);

        DrawSeparator();

        filter[LootRuleFlags.GreedOnly] = DrawRestrictionEntry("仅限贪婪", filter[LootRuleFlags.GreedOnly]);
        filter[LootRuleFlags.Lootmaster] = DrawRestrictionEntry("队长分配", filter[LootRuleFlags.Lootmaster]);

        DrawSeparator();

        filter[SearchAreaFlags.DataCentre] = DrawRestrictionEntry("跨服小队", filter[SearchAreaFlags.DataCentre]);
        filter[SearchAreaFlags.World] = DrawRestrictionEntry("服务器内招募", filter[SearchAreaFlags.World]);
        filter[SearchAreaFlags.OnePlayerPerJob] = DrawRestrictionEntry("职业不重复", filter[SearchAreaFlags.OnePlayerPerJob]);

        if (Save)
            Plugin.Config.Save();
    }

    private bool DrawRestrictionEntry(string name, bool state)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(state ? 0 : 1);

        if (ImGui.Selectable(name))
        {
            state = !state;
            Save = true;
        }

        return state;
    }

    private void DrawSeparator()
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.Separator();
        ImGui.TableNextColumn();
        ImGui.Separator();
    }
}