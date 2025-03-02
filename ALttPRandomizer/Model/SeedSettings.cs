namespace ALttPRandomizer.Model {
    using ALttPRandomizer.Settings;
    using System.ComponentModel.DataAnnotations;

    public class SeedSettings {
        public const string Omit = "<null>";

        public Mode Mode { get; set; } = Mode.Open;

        public Weapons Weapons { get; set; } = Weapons.Random;

        public Goal Goal { get; set; } = Goal.Ganon;

        public EntranceShuffle EntranceShuffle { get; set; } = EntranceShuffle.Vanilla;
        public SkullWoodsShuffle SkullWoods { get; set; } = SkullWoodsShuffle.Original;
        public LinkedDrops LinkedDrops { get; set; } = LinkedDrops.Unset;

        public BossShuffle BossShuffle { get; set; } = BossShuffle.Vanilla;

        public EnemyShuffle EnemyShuffle { get; set; } = EnemyShuffle.Vanilla;

        [CommonValue("keyshuffle")]
        public DungeonItemLocations SmallKeys { get; set; } = DungeonItemLocations.Dungeon;

        [CommonValue("bigkeyshuffle")]
        [DeniedValues(DungeonItemLocations.Universal)]
        public DungeonItemLocations BigKeys { get; set; } = DungeonItemLocations.Dungeon;

        [CommonValue("mapshuffle")]
        [DeniedValues(DungeonItemLocations.Universal)]
        public DungeonItemLocations Maps { get; set; } = DungeonItemLocations.Dungeon;

        [CommonValue("compassshuffle")]
        [DeniedValues(DungeonItemLocations.Universal)]
        public DungeonItemLocations Compasses { get; set; } = DungeonItemLocations.Dungeon;

        public ShopShuffle ShopShuffle { get; set; } = ShopShuffle.Vanilla;
        public DropShuffle DropShuffle { get; set; } = DropShuffle.Vanilla;
        public Pottery Pottery { get; set; } = Pottery.Vanilla;

        public PrizeShuffle PrizeShuffle { get; set; } = PrizeShuffle.Vanilla;
    }

    public enum Mode {
        Open,
        Standard,
        Inverted,
    }

    [CommonValue("swords")]
    public enum Weapons {
        Random,
        Assured,
        Vanilla,
        Swordless,
    }

    public enum Goal {
        Ganon,
        [CommonValue("crystals")] FastGanon,
        [CommonValue("dungeons")] AllDungeons,
        Pedestal,
        [CommonValue("triforcehunt")]TriforceHunt,
        GanonHunt,
        Completionist,
    }

    [CommonValue("shuffle")]
    public enum EntranceShuffle {
        Vanilla,
        Full,
        Crossed,
        Swapped,
        [CommonValue("insanity")] Decoupled,
    }

    [CommonValue("skullwoods")]
    public enum SkullWoodsShuffle {
        Original,
        Restricted,
        Loose,
        FollowLinked,
    }

    [CommonValue("linked_drops")]
    public enum LinkedDrops {
        Unset,
        Linked,
        Independent,
    }

    [CommonValue("shufflebosses")]
    public enum BossShuffle {
        [CommonValue("none")] Vanilla,
        Simple,
        Full,
        Random,
        [CommonValue("unique")] PrizeUnique,
    }

    [CommonValue("shuffleenemies")]
    public enum EnemyShuffle {
        [CommonValue("none")] Vanilla,
        Shuffled,
        Mimics,
    }

    public enum DungeonItemLocations {
        [CommonValue("none")] Dungeon,
        Wild,
        Nearby,
        Universal,
    }

    [CommonValue("shopsanity")]
    public enum ShopShuffle {
        [CommonValue(SeedSettings.Omit)] Vanilla,
        [CommonValue("true")] Shuffled,
    }

    public enum DropShuffle {
        [CommonValue("none")] Vanilla,
        Keys,
        Underworld,
    }

    public enum Pottery {
        [CommonValue("none")] Vanilla,
        Keys,
        Cave,
        CaveKeys,
        Reduced,
        Clustered,
        NonEmpty,
        Dungeon,
        Lottery,
    }

    public enum PrizeShuffle {
        [CommonValue("none")] Vanilla,
        Dungeon,
        Nearby,
        Wild,
    }

}
