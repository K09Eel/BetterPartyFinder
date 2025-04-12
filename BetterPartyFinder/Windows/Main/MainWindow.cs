using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace BetterPartyFinder.Windows.Main;

public unsafe partial class MainWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    private bool IsCollapsed;
    private AtkUnitBase* Addon = null;

    private string PresetName { get; set; } = string.Empty;

    private Tabs SelectedTab;
    private static readonly Tabs[] WindowTabs = [Tabs.Categories, Tabs.Duties, Tabs.Jobs, Tabs.Restrictions, Tabs.Keywords, Tabs.SensitiveWords];

    public MainWindow(Plugin plugin) : base(Plugin.Name)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 520),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        Flags = ImGuiWindowFlags.NoDocking;

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        Addon = null;
        var addonPtr = Plugin.GameGui.GetAddonByName("LookingForGroup");
        if (Plugin.Config.ShowWhenPfOpen && addonPtr != nint.Zero)
            Addon = (AtkUnitBase*) addonPtr;

        IsCollapsed = true;

        if (Position.HasValue)
            ImGui.SetNextWindowPos(Position.Value, ImGuiCond.Always);

        Position = null;
        base.PreDraw();
    }

    public override void PostDraw()
    {
        if (IsCollapsed && Addon != null && Addon->IsVisible)
        {
            // wait until addon is initialised to show
            var rootNode = Addon->RootNode;
            if (rootNode == null)
                return;

            Position = ImGuiHelpers.MainViewport.Pos + new Vector2(Addon->X, Addon->Y - ImGui.GetFrameHeight());
        }
        base.PostDraw();
    }

    public override void Draw()
    {
        IsCollapsed = false;

        if (Addon != null && Plugin.Config.WindowSide == WindowSide.Right)
        {
            var rootNode = Addon->RootNode;
            if (rootNode != null)
                ImGui.SetWindowPos(ImGuiHelpers.MainViewport.Pos + new Vector2(Addon->X + rootNode->Width, Addon->Y));
        }

        var selected = Plugin.Config.SelectedPreset;

        string selectedName;
        if (selected == null)
        {
            selectedName = "<none>";
        }
        else
        {
            if (Plugin.Config.Presets.TryGetValue(selected.Value, out var preset))
            {
                selectedName = preset.Name;
            }
            else
            {
                Plugin.Config.SelectedPreset = null;
                selectedName = "<invalid preset>";
            }
        }

        using (var combo = ImRaii.Combo("###preset", selectedName))
        {
            if (combo.Success)
            {
                if (ImGui.Selectable("<none>"))
                {
                    Plugin.Config.SelectedPreset = null;
                    Plugin.Config.Save();

                    Plugin.HookManager.RefreshListings();
                }

                foreach (var preset in Plugin.Config.Presets)
                {
                    if (!ImGui.Selectable(preset.Value.Name))
                        continue;

                    Plugin.Config.SelectedPreset = preset.Key;
                    Plugin.Config.Save();

                    Plugin.HookManager.RefreshListings();
                }
            }
        }

        ImGui.SameLine();

        if (Helper.IconButton(FontAwesomeIcon.Plus, "add-preset"))
        {
            var id = Guid.NewGuid();

            Plugin.Config.Presets.Add(id, ConfigurationFilter.Create());
            Plugin.Config.SelectedPreset = id;
            Plugin.Config.Save();
        }

        if (ImGui.IsItemHovered())
            Helper.Tooltip("添加预设");

        ImGui.SameLine();

        if (Helper.IconButton(FontAwesomeIcon.Trash, "delete-preset") && selected != null)
        {
            Plugin.Config.Presets.Remove(selected.Value);
            Plugin.Config.Save();
        }

        if (ImGui.IsItemHovered())
            Helper.Tooltip("删除当前预设");

        ImGui.SameLine();

        if (Helper.IconButton(FontAwesomeIcon.PencilAlt, "edit-preset") && selected != null)
        {
            if (Plugin.Config.Presets.TryGetValue(selected.Value, out var editPreset))
            {
                PresetName = editPreset.Name;

                ImGui.OpenPopup("###rename-preset");
            }
        }

        using (var modal = ImRaii.PopupModal("重命名###rename-preset"))
        {
            if (modal.Success && selected != null)
            {
                if (Plugin.Config.Presets.TryGetValue(selected.Value, out var editPreset))
                {
                    ImGui.TextUnformatted("预设名称");
                    ImGui.PushItemWidth(-1f);
                    var name = PresetName;
                    if (ImGui.InputText("###preset-name", ref name, 1_000))
                        PresetName = name;
                    ImGui.PopItemWidth();

                    if (ImGui.Button("保存") && PresetName.Trim().Length > 0)
                    {
                        editPreset.Name = PresetName;
                        Plugin.Config.Save();
                        ImGui.CloseCurrentPopup();
                    }
                }
            }
        }

        if (ImGui.IsItemHovered())
            Helper.Tooltip("重命名");

        ImGui.SameLine();

        if (Helper.IconButton(FontAwesomeIcon.Copy, "copy") && selected != null)
        {
            if (Plugin.Config.Presets.TryGetValue(selected.Value, out var copyFilter))
            {
                var guid = Guid.NewGuid();

                var copied = copyFilter.Clone();
                copied.Name += " (copy)";
                Plugin.Config.Presets.Add(guid, copied);
                Plugin.Config.SelectedPreset = guid;
                Plugin.Config.Save();
            }
        }

        if (ImGui.IsItemHovered())
            Helper.Tooltip("复制预设");

        ImGui.Separator();

        var pos = ImGui.GetCursorPos();

        var nameDict = TabHelper.TabSize(WindowTabs);
        var childSize = new Vector2(nameDict.Select(pair => pair.Value.Width).Max(), 0);
        using (var tabChild = ImRaii.Child("Tabs", childSize, true))
        {
            if (tabChild.Success)
            {
                foreach (var (id, (name, _)) in nameDict)
                    if (ImGui.Selectable(name, SelectedTab == id))
                        SelectedTab = id;
            }
        }

        ImGui.SetCursorPos(pos with {X = pos.X + childSize.X});
        using (var contentChild = ImRaii.Child("Content", Vector2.Zero, true))
        {
            if (contentChild.Success)
            {
                if (selected == null)
                    return;

                if (!Plugin.Config.Presets.TryGetValue(selected.Value, out var filter))
                    return;

                switch (SelectedTab)
                {
                    case Tabs.Categories:
                        DrawCategoriesTab(filter);
                        break;
                    case Tabs.Duties:
                        DrawDutiesTab(filter);
                        break;
                    case Tabs.ILvL:
                        DrawItemLevelTab(filter);
                        break;
                    case Tabs.Jobs:
                        DrawJobsTab(filter);
                        break;
                    case Tabs.Restrictions:
                        DrawRestrictionsTab(filter);
                        break;
                    case Tabs.Players:
                        DrawPlayersTab(filter);
                        break;
                    case Tabs.Keywords:
                        DrawKeywordsTab(filter);
                        break;
                    case Tabs.SensitiveWords:
                        DrawSensitiveTab(filter);
                        break;
                }
            }
        }

        if (Addon != null && Plugin.Config.WindowSide == WindowSide.Left)
        {
            var rootNode = Addon->RootNode;
            if (rootNode != null)
            {
                var currentWidth = ImGui.GetWindowWidth();
                ImGui.SetWindowPos(ImGuiHelpers.MainViewport.Pos + new Vector2(Addon->X - currentWidth, Addon->Y));
            }
        }
    }
}
