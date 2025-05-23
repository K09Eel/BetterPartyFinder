using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Game.Gui.PartyFinder.Types;

namespace BetterPartyFinder;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public Dictionary<Guid, ConfigurationFilter> Presets { get; } = new();
    public Guid? SelectedPreset { get; set; }

    public bool ShowWhenPfOpen { get; set; }
    public WindowSide WindowSide { get; set; } = WindowSide.Left;

    internal static Configuration? Load()
    {
        return (Configuration?) Plugin.Interface.GetPluginConfig();
    }

    internal void Save()
    {
        Plugin.Interface.SavePluginConfig(this);
    }
}

public class ConfigurationFilter
{
    public string Name { get; set; } = "<unnamed preset>";

    public ListMode DutiesMode { get; set; } = ListMode.Blacklist;
    public HashSet<uint> Duties { get; set; } = [];

    public HashSet<UiCategory> Categories { get; set; } = [];

    public List<JobFlags> Jobs { get; set; } = [];
    // default to true because that's the PF's default
    // use nosol if trying to avoid spam

    public SearchAreaFlags SearchArea { get; set; } = (SearchAreaFlags) ~(uint) 0;
    public LootRuleFlags LootRule { get; set; } = ~LootRuleFlags.None;
    public DutyFinderSettingsFlags DutyFinderSettings { get; set; } = ~DutyFinderSettingsFlags.None;
    public ConditionFlags Conditions { get; set; } = (ConditionFlags) ~(uint) 0;
    public ObjectiveFlags Objectives { get; set; } = ~ObjectiveFlags.None;

    public bool AllowHugeItemLevel { get; set; } = true;
    public uint? MinItemLevel { get; set; }
    public uint? MaxItemLevel { get; set; }

    public HashSet<PlayerInfo> Players { get; set; } = [];
    public HashSet<string> Keywords { get; set; } = [];
    public HashSet<string> SensitiveWords { get; set; } = [];

    internal bool this[SearchAreaFlags flags]
    {
        get => (SearchArea & flags) > 0;
        set
        {
            if (value)
                SearchArea |= flags;
            else
                SearchArea &= ~flags;
        }
    }

    internal bool this[LootRuleFlags flags]
    {
        get => (LootRule & flags) > 0;
        set
        {
            if (value)
                LootRule |= flags;
            else
                LootRule &= ~flags;
        }
    }

    internal bool this[DutyFinderSettingsFlags flags]
    {
        get => (DutyFinderSettings & flags) > 0;
        set
        {
            if (value)
                DutyFinderSettings |= flags;
            else
                DutyFinderSettings &= ~flags;
        }
    }

    internal bool this[ConditionFlags flags]
    {
        get => (Conditions & flags) > 0;
        set
        {
            if (value)
                Conditions |= flags;
            else
                Conditions &= ~flags;
        }
    }

    internal bool this[ObjectiveFlags flags]
    {
        get => (Objectives & flags) > 0;
        set
        {
            if (value)
                Objectives |= flags;
            else
                Objectives &= ~flags;
        }
    }

    internal ConfigurationFilter Clone()
    {
        var categories = Categories.ToHashSet();
        var duties = Duties.ToHashSet();
        var jobs = Jobs.ToList();
        var players = Players.Select(info => info.Clone()).ToHashSet();

        return new ConfigurationFilter
        {
            Categories = categories,
            Conditions = Conditions,
            Duties = duties,
            Jobs = jobs,
            Name = new string(Name),
            Objectives = Objectives,
            DutiesMode = DutiesMode,
            LootRule = LootRule,
            SearchArea = SearchArea,
            DutyFinderSettings = DutyFinderSettings,
            MaxItemLevel = MaxItemLevel,
            MinItemLevel = MinItemLevel,
            AllowHugeItemLevel = AllowHugeItemLevel,
            Players = players,
        };
    }

    internal static ConfigurationFilter Create()
    {
        return new ConfigurationFilter { Categories = Enum.GetValues<UiCategory>().ToHashSet(), };
    }
}

public class PlayerInfo
{
    public string Name { get; }
    public uint World { get; }

    public PlayerInfo(string name, uint world)
    {
        Name = name;
        World = world;
    }

    internal PlayerInfo Clone()
    {
        return new PlayerInfo(Name, World);
    }

    private bool Equals(PlayerInfo other)
    {
        return Name == other.Name && World == other.World;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return obj.GetType() == GetType() && Equals((PlayerInfo) obj);
    }

    public override int GetHashCode()
    {
        unchecked { return (Name.GetHashCode() * 397) ^ (int) World; }
    }
}

public enum ListMode
{
    Whitelist,
    Blacklist,
}

public enum WindowSide
{
    Left,
    Right,
}