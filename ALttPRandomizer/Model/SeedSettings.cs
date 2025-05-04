namespace ALttPRandomizer.Model {
    using ALttPRandomizer.Randomizers;
    using ALttPRandomizer.Settings;
    using System.Text.Json.Serialization;

    using static ALttPRandomizer.Model.RandomizerInstance;

    public class SeedSettings {
        [NoSettingName]
        public RandomizerInstance Randomizer { get; set; } = RandomizerInstance.Base;

        [NoSettingName]
        public string PlayerName { get; set; } = string.Empty;

        [NoSettingName]
        public RaceMode Race { get; set; } = RaceMode.Normal;

        [ForbiddenSetting([Apr2025], Mode.Inverted)]
        public Mode Mode { get; set; } = Mode.Open;

        [SettingName("swords")]
        [ForbiddenSetting([Apr2025], Weapons.Swordless, Weapons.AssuredMaster)]
        public Weapons Weapons { get; set; } = Weapons.Random;

        [RequiredSetting([Apr2025], Goal.Ganon)]
        public Goal Goal { get; set; } = Goal.Ganon;

        [SettingName("crystals_ganon")]
        public EntryRequirement CrystalsGanon { get; set; } = EntryRequirement.Crystals7;

        [SettingName("crystals_gt")]
        [JsonPropertyName("crystals_gt")]
        [NoSettingName([Apr2025])]
        public EntryRequirement CrystalsGT { get; set; } = EntryRequirement.Crystals7;

        [SettingName("shuffle")]
        [ForbiddenSetting([Apr2025], EntranceShuffle.Swapped)]
        public EntranceShuffle EntranceShuffle { get; set; } = EntranceShuffle.Vanilla;

        [SettingName("skullwoods")]
        [RequiredSetting([Apr2025], SkullWoodsShuffle.Original)]
        [NoSettingName([Apr2025])]
        public SkullWoodsShuffle SkullWoods { get; set; } = SkullWoodsShuffle.Original;

        [SettingName("linked_drops")]
        [RequiredSetting([Apr2025], LinkedDrops.Unset)]
        [NoSettingName([Apr2025])]
        public LinkedDrops LinkedDrops { get; set; } = LinkedDrops.Unset;

        [SettingName("shufflebosses")]
        [RequiredSetting([Apr2025], BossShuffle.Vanilla)]
        [NoSettingName([Apr2025])]
        public BossShuffle BossShuffle { get; set; } = BossShuffle.Vanilla;

        [SettingName("shuffleenemies")]
        [RequiredSetting([Apr2025], EnemyShuffle.Vanilla)]
        [NoSettingName([Apr2025])]
        public EnemyShuffle EnemyShuffle { get; set; } = EnemyShuffle.Vanilla;

        [SettingName("keyshuffle")]
        [RequiredSetting([Apr2025], KeyLocations.Dungeon, KeyLocations.Wild)]
        [NoSettingName([Apr2025])]
        public KeyLocations SmallKeys { get; set; } = KeyLocations.Dungeon;

        [SettingName("bigkeyshuffle")]
        [RequiredSetting([Apr2025], DungeonItemLocations.Dungeon)]
        [NoSettingName([Apr2025])]
        public DungeonItemLocations BigKeys { get; set; } = DungeonItemLocations.Dungeon;

        [SettingName("mapshuffle")]
        [RequiredSetting([Apr2025], DungeonItemLocations.Dungeon)]
        [NoSettingName([Apr2025])]
        public DungeonItemLocations Maps { get; set; } = DungeonItemLocations.Dungeon;

        [SettingName("compassshuffle")]
        [RequiredSetting([Apr2025], DungeonItemLocations.Dungeon)]
        [NoSettingName([Apr2025])]
        public DungeonItemLocations Compasses { get; set; } = DungeonItemLocations.Dungeon;

        [NoSettingName]
        [RequiredSetting([Apr2025], ShopShuffle.Vanilla)]
        public ShopShuffle ShopShuffle { get; set; } = ShopShuffle.Vanilla;

        [RequiredSetting([Apr2025], DropShuffle.Vanilla)]
        [NoSettingName([Apr2025])]
        public DropShuffle DropShuffle { get; set; } = DropShuffle.Vanilla;

        [RequiredSetting([Apr2025], Pottery.Vanilla)]
        [NoSettingName([Apr2025])]
        public Pottery Pottery { get; set; } = Pottery.Vanilla;

        [RequiredSetting([Apr2025], PrizeShuffle.Vanilla)]
        [NoSettingName([Apr2025])]
        public PrizeShuffle PrizeShuffle { get; set; } = PrizeShuffle.Vanilla;

        [NoSettingName]
        [ForbiddenSetting([Apr2025], BootsSettings.Starting)]
        public BootsSettings Boots { get; set; } = BootsSettings.Normal;

        [NoSettingName]
        [RequiredSetting([Apr2025], FluteSettings.Normal)]
        public FluteSettings Flute { get; set; } = FluteSettings.Normal;

        [SettingName("dark_rooms")]
        [RequiredSetting([Apr2025], DarkRoomSettings.RequireLamp)]
        [NoSettingName([Apr2025])]
        public DarkRoomSettings DarkRooms { get; set; } = DarkRoomSettings.RequireLamp;

        [NoSettingName]
        [RequiredSetting([Apr2025], BombSettings.Normal)]
        public BombSettings Bombs { get; set; } = BombSettings.Normal;

        [NoSettingName]
        [RequiredSetting([Apr2025], BookSettings.Normal)]
        public BookSettings Book { get; set; } = BookSettings.Normal;

        [SettingName("door_shuffle")]
        [RequiredSetting([Apr2025], DoorShuffle.Vanilla)]
        [NoSettingName([Apr2025])]
        public DoorShuffle DoorShuffle { get; set; } = DoorShuffle.Vanilla;

        [SettingName("intensity")]
        [NoSettingName([Apr2025])]
        public DoorLobbies Lobbies { get; set; } = DoorLobbies.Vanilla;

        [SettingName("door_type_mode")]
        [NoSettingName([Apr2025])]
        public DoorTypeMode DoorTypeMode { get; set; } = DoorTypeMode.Big;

        [SettingName("trap_door_mode")]
        [NoSettingName([Apr2025])]
        public TrapDoorMode TrapDoorMode { get; set; } = TrapDoorMode.Optional;

        [NoSettingName]
        [RequiredSetting([Apr2025], FollowerShuffle.Vanilla)]
        public FollowerShuffle FollowerShuffle { get; set; } = FollowerShuffle.Vanilla;

        [NoSettingName]
        public Hints Hints { get; set; } = Hints.Off;
    }

    public enum RandomizerInstance {
        [RandomizerName(BaseRandomizer.Name)] Base,
        [RandomizerName(Apr2025Randomizer.Name)] Apr2025,
    }

    public enum RaceMode {
        Normal,
        [AdditionalSetting("--securerandom")] Race,
    }

    public enum Mode {
        Open,
        Standard,
        Inverted,
    }

    public enum Weapons {
        Random,
        Assured,
        Vanilla,
        Swordless,
        [SettingName("assured")] [AddStartingItems("Progressive_Sword")] AssuredMaster,
    }

    public enum Goal {
        Ganon,
        [SettingName("crystals")] FastGanon,
        [SettingName("dungeons")] AllDungeons,
        Pedestal,
        [SettingName("triforcehunt")]TriforceHunt,
        GanonHunt,
        Completionist,
        Sanctuary,
    }

    public enum EntryRequirement {
        [JsonStringEnumMemberName("0")] [SettingName("0")] Crystals0 = 0,
        [JsonStringEnumMemberName("1")] [SettingName("1")] Crystals1 = 1,
        [JsonStringEnumMemberName("2")] [SettingName("2")] Crystals2 = 2,
        [JsonStringEnumMemberName("3")] [SettingName("3")] Crystals3 = 3,
        [JsonStringEnumMemberName("4")] [SettingName("4")] Crystals4 = 4,
        [JsonStringEnumMemberName("5")] [SettingName("5")] Crystals5 = 5,
        [JsonStringEnumMemberName("6")] [SettingName("6")] Crystals6 = 6,
        [JsonStringEnumMemberName("7")] [SettingName("7")] Crystals7 = 7,
        Random,
    }

    public enum EntranceShuffle {
        Vanilla,
        Full,
        Crossed,
        Swapped,
        [SettingName("insanity")] Decoupled,
    }

    public enum SkullWoodsShuffle {
        Original,
        Restricted,
        Loose,
        FollowLinked,
    }

    public enum LinkedDrops {
        Unset,
        Linked,
        Independent,
    }

    public enum BossShuffle {
        [SettingName("none")] Vanilla,
        Simple,
        Full,
        Random,
        [SettingName("unique")] PrizeUnique,
    }

    public enum EnemyShuffle {
        [SettingName("none")] Vanilla,
        Shuffled,
        Mimics,
    }

    public enum KeyLocations {
        [SettingName("none")] Dungeon,
        [AdditionalSetting([Apr2025], "--keysanity")] Wild,
        Nearby,
        Universal,
    }

    public enum DungeonItemLocations {
        [SettingName("none")] Dungeon,
        Wild,
        Nearby,
    }

    public enum ShopShuffle {
        Vanilla,
        [AdditionalSetting("--shopsanity")] Shuffled,
    }

    public enum DropShuffle {
        [SettingName("none")] Vanilla,
        Keys,
        Underworld,
    }

    public enum Pottery {
        [SettingName("none")] Vanilla,
        [AdditionalSetting("--colorizepots")] Keys,
        [AdditionalSetting("--colorizepots")] Cave,
        [AdditionalSetting("--colorizepots")] CaveKeys,
        [AdditionalSetting("--colorizepots")] Reduced,
        [AdditionalSetting("--colorizepots")] Clustered,
        [AdditionalSetting("--colorizepots")] NonEmpty,
        [AdditionalSetting("--colorizepots")] Dungeon,
        Lottery,
    }

    public enum PrizeShuffle {
        [SettingName("none")] Vanilla,
        Dungeon,
        Nearby,
        Wild,
    }

    public enum BootsSettings {
        Normal,
        [AdditionalSetting("--pseudoboots")] Pseudoboots,
        [AddStartingItems("Pegasus_Boots")] Starting,
    }

    public enum FluteSettings {
        Normal,
        [AdditionalSetting("--flute_mode=active")] Preactivated,
        [AddStartingItems("Ocarina_(Activated)")] Starting,
    }

    public enum DarkRoomSettings {
        [SettingName("require_lamp")] RequireLamp,
        [SettingName("always_light_cone")] AlwaysLightCone,
        [SettingName("no_dark_rooms")] NoDarkRooms,
        [SettingName("require_lamp")] [AddStartingItems("Lamp")] StartingLamp,
    }

    public enum BombSettings {
        Normal,
        [AdditionalSetting("--bombbag")] BombBagRequired,
    }

    public enum BookSettings {
        Normal,
        [AdditionalSetting("--crystal_book")] CrystalSwitches,
    }

    public enum DoorShuffle {
        Vanilla,
        Basic,
        Paired,
        Partitioned,
        Crossed,
    }

    public enum DoorLobbies {
        [SettingName("2")] Vanilla,
        [SettingName("3")] Shuffled,
    }

    public enum DoorTypeMode {
        Original,
        Big,
        All,
        Chaos,
    }

    public enum TrapDoorMode {
        Vanilla,
        Optional,
        Boss,
        [SettingName("oneway")] RemoveAll,
    }

    public enum FollowerShuffle {
        Vanilla,
        [AdditionalSetting("--shuffle_followers")] Shuffled,
    }

    public enum Hints {
        Off,
        [AdditionalSetting("--hints")] On,
    }
}
