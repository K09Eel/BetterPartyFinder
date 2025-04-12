using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace BetterPartyFinder.Windows.Main;

public partial class MainWindow
{
    private string DutySearchQuery = string.Empty;

    private void DrawDutiesTab(ConfigurationFilter filter)
    {
        var listModeStrings = new[]
        {
            "展示已选择",
            "去除已选择",
        };

        Helper.TextColored(ImGuiColors.DalamudOrange, "筛选模式:");
        ImGui.Separator();

        var listModeIdx = filter.DutiesMode == ListMode.Blacklist ? 1 : 0;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 2);
        if (ImGui.Combo("###list-mode", ref listModeIdx, listModeStrings, listModeStrings.Length))
        {
            filter.DutiesMode = listModeIdx == 0 ? ListMode.Whitelist : ListMode.Blacklist;
            Plugin.Config.Save();
        }

        ImGuiComponents.IconButton(FontAwesomeIcon.Search);
        if (DutyAddPopup("DutyAddPopup", out var row, filter))
        {
            filter.Duties.Add(row);
            Plugin.Config.Save();
        }

        if (ImGui.IsItemHovered())
            Helper.Tooltip("搜索副本");

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Eraser))
        {
            filter.Duties.Clear();
            Plugin.Config.Save();
        }

        if (ImGui.IsItemHovered())
            Helper.Tooltip("清空已选择");

        Helper.TextColored(ImGuiColors.HealerGreen, "已选择:");
        ImGui.Separator();

        using var child = ImRaii.Child("duty-selection", Vector2.Zero);
        if (!child.Success)
            return;

        foreach (var cf in filter.Duties.Order().ToArray())
        {
            if (!ImGui.Selectable(Sheets.ContentFinderSheet.GetRow(cf).Name.UpperCaseStr()))
                continue;

            filter.Duties.Remove(cf);
            Plugin.Config.Save();
        }
    }

    private ContentFinderCondition[]? FilteredDuties;


    private void ExcelSheetSearchInput()
    {
        if (ImGui.IsWindowAppearing() && ImGui.IsWindowFocused() && !ImGui.IsAnyItemActive())
        {
            FilteredDuties = null;
            DutySearchQuery = string.Empty;

            ImGui.SetKeyboardFocusHere(0);
        }

        if (ImGui.InputTextWithHint("##DutySheetSearch", "Search", ref DutySearchQuery, 128, ImGuiInputTextFlags.AutoSelectAll))
            FilteredDuties = null;

        FilteredDuties ??= Sheets.DutyCache.Where(duty => duty.Name.ExtractText().ContainsIgnoreCase(DutySearchQuery)).ToArray();
    }

    private bool DutyAddPopup(string id, out uint selectedRow, ConfigurationFilter filter)
    {
        selectedRow = 0;

        ImGui.SetNextWindowSize(new Vector2(0, 250 * ImGuiHelpers.GlobalScale));
        using var popup = ImRaii.ContextPopupItem(id, ImGuiPopupFlags.None);
        if (!popup.Success)
            return false;

        ExcelSheetSearchInput();

        using var child = ImRaii.Child("DutySheetList", Vector2.Zero, true);
        if (!child.Success)
            return false;

        var ret = false;
        foreach (var duty in FilteredDuties!.Where(d => !filter.Duties.Contains(d.RowId)))
        {
            using var pushedId = ImRaii.PushId(id);
            if (!ImGui.Selectable(duty.Name.UpperCaseStr()))
                continue;

            selectedRow = duty.RowId;
            ret = true;
        }

        return ret;
    }
}