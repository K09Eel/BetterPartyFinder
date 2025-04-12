using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BetterPartyFinder.Windows;

public static class Helper
{
    /// <summary>
    /// An unformatted version for Helper.TextColored
    /// </summary>
    /// <param name="color">color to be used</param>
    /// <param name="text">text to display</param>
    public static void TextColored(Vector4 color, string text)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(text);
    }

    /// <summary>
    /// An unformatted version for Helper.Tooltip
    /// </summary>
    /// <param name="tooltip">tooltip to display</param>
    public static void Tooltip(string tooltip)
    {
        using (ImRaii.Tooltip())
        using (ImRaii.TextWrapPos(ImGui.GetFontSize() * 35.0f))
            ImGui.TextUnformatted(tooltip);
    }

    internal static bool IconButton(FontAwesomeIcon icon, string? id = null, string? tooltip = null, int width = 0)
    {
        var label = icon.ToIconString();
        if (id != null)
            label += $"##{id}";

        Plugin.Interface.UiBuilder.IconFontFixedWidthHandle.Push();
        var size = new Vector2(0, 0);
        if (width > 0)
        {
            var style = ImGui.GetStyle();
            size.X = width - 2 * style.CellPadding.X;
        }
        var ret = ImGui.Button(label, size);
        Plugin.Interface.UiBuilder.IconFontFixedWidthHandle.Pop();

        if (tooltip != null && ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);

        return ret;
    }
}

public enum Tabs
{
    [Description("任务类型")] Categories,
    [Description("副本名称")] Duties,
    [Description("Item Level")] ILvL,
    [Description("职业")] Jobs,
    [Description("招募类型")] Restrictions,
    [Description("Players")] Players,
    [Description("关键词搜索")] Keywords,
    [Description("关键词屏蔽")] SensitiveWords,
}

public static class TabHelper
{
    public static SortedDictionary<Tabs, (string Name, float Width)> TabSize(Tabs[] tabs)
    {
        var doubleItemSpacingWidth = ImGui.GetStyle().ItemSpacing.X * 2;

        var nameDict = new SortedDictionary<Tabs, (string Name, float Width)>();
        foreach (var tab in tabs)
        {
            var name = tab.GetDescription();
            nameDict[tab] = (name, ImGui.CalcTextSize(name).X + doubleItemSpacingWidth);
        }

        return nameDict;
    }
}