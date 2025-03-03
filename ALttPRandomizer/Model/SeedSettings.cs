namespace ALttPRandomizer.Model {
    using ALttPRandomizer.Settings;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    public class SeedSettings {
        [NoSettingName]
        public RaceMode Race { get; set; } = RaceMode.Normal;

        public Mode Mode { get; set; } = Mode.Open;

        [SettingName("swords")]
        public Weapons Weapons { get; set; } = Weapons.Random;

        public Goal Goal { get; set; } = Goal.Ganon;

        [SettingName("crystals_ganon")]
        public EntryRequirement CrystalsGanon { get; set; } = EntryRequirement.Crystals7;

        [SettingName("crystals_gt")]
        [JsonPropertyName("crystals_gt")]
        public EntryRequirement CrystalsGT { get; set; } = EntryRequirement.Crystals7;

        [SettingName("shuffle")]
        public EntranceShuffle EntranceShuffle { get; set; } = EntranceShuffle.Vanilla;
        [SettingName("skullwoods")]
        public SkullWoodsShuffle SkullWoods { get; set; } = SkullWoodsShuffle.Original;
        [SettingName("linked_drops")]
        public LinkedDrops LinkedDrops { get; set; } = LinkedDrops.Unset;

        [SettingName("shufflebosses")]
        public BossShuffle BossShuffle { get; set; } = BossShuffle.Vanilla;

        [SettingName("shuffleenemies")]
        public EnemyShuffle EnemyShuffle { get; set; } = EnemyShuffle.Vanilla;

        [SettingName("keyshuffle")]
        public DungeonItemLocations SmallKeys { get; set; } = DungeonItemLocations.Dungeon;

        [SettingName("bigkeyshuffle")]
        [DeniedValues(DungeonItemLocations.Universal)]
        public DungeonItemLocations BigKeys { get; set; } = DungeonItemLocations.Dungeon;

        [SettingName("mapshuffle")]
        [DeniedValues(DungeonItemLocations.Universal)]
        public DungeonItemLocations Maps { get; set; } = DungeonItemLocations.Dungeon;

        [SettingName("compassshuffle")]
        [DeniedValues(DungeonItemLocations.Universal)]
        public DungeonItemLocations Compasses { get; set; } = DungeonItemLocations.Dungeon;

        [NoSettingName]
        public ShopShuffle ShopShuffle { get; set; } = ShopShuffle.Vanilla;
        public DropShuffle DropShuffle { get; set; } = DropShuffle.Vanilla;
        public Pottery Pottery { get; set; } = Pottery.Vanilla;

        public PrizeShuffle PrizeShuffle { get; set; } = PrizeShuffle.Vanilla;
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
    }

    public enum Goal {
        Ganon,
        [SettingName("crystals")] FastGanon,
        [SettingName("dungeons")] AllDungeons,
        Pedestal,
        [SettingName("triforcehunt")]TriforceHunt,
        GanonHunt,
        Completionist,
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

    public enum DungeonItemLocations {
        [SettingName("none")] Dungeon,
        Wild,
        Nearby,
        Universal,
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

}
