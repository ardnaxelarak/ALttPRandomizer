using System.ComponentModel.DataAnnotations;

namespace ALttPRandomizer.Model {
    public class SeedSettings {
        public Mode Mode { get; set; } = Mode.Open;

        public Weapons Weapons { get; set; } = Weapons.Randomized;

        public Goal Goal { get; set; } = Goal.Ganon;

        public EntranceShuffle EntranceShuffle { get; set; } = EntranceShuffle.None;

        public BossShuffle BossShuffle { get; set; } = BossShuffle.None;

        public DungeonItemLocations SmallKeys { get; set; } = DungeonItemLocations.Dungeon;

        [DeniedValues(DungeonItemLocations.Universal)]
        public DungeonItemLocations BigKeys { get; set; } = DungeonItemLocations.Dungeon;

        [DeniedValues(DungeonItemLocations.Universal)]
        public DungeonItemLocations Maps { get; set; } = DungeonItemLocations.Dungeon;

        [DeniedValues(DungeonItemLocations.Universal)]
        public DungeonItemLocations Compasses { get; set; } = DungeonItemLocations.Dungeon;
    }

    public enum Mode {
        Open,
        Standard,
        Inverted,
    }

    public enum Weapons {
        Randomized,
        Assured,
        Vanilla,
        Swordless,
    }

    public enum Goal {
        Ganon,
        FastGanon,
        AllDungeons,
        Pedestal,
        TriforceHunt,
        GanonHunt,
        Completionist,
    }

    public enum EntranceShuffle {
        None,
        Full,
        Crossed,
        Decoupled,
    }

    public enum BossShuffle {
        None,
        Simple,
        Full,
        Random,
        PrizeUnique,
    }

    public enum DungeonItemLocations {
        Dungeon,
        Wild,
        Nearby,
        Universal,
    }
}
