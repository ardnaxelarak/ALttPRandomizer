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

        [SettingName("bosses_ganon")]
        [NoSettingName([Apr2025, Base])]
        public BossRequirement BossesGanon { get; set; } = BossRequirement.Bosses8of10;

        [SettingName("crystals_gt")]
        [JsonPropertyName("crystals_gt")]
        [NoSettingName([Apr2025])]
        public EntryRequirement CrystalsGT { get; set; } = EntryRequirement.Crystals7;

        [SettingName("ganon_item")]
        [RequiredSetting([Apr2025], GanonItem.Silver)]
        [NoSettingName([Apr2025])]
        public GanonItem GanonItem { get; set; } = GanonItem.Silver;

        [SettingName("shuffle")]
        [ForbiddenSetting([Apr2025], EntranceShuffle.Swapped)]
        public EntranceShuffle EntranceShuffle { get; set; } = EntranceShuffle.Vanilla;

        [NoSettingName]
        [RequiredSetting([Apr2025], LinksHouse.Vanilla)]
        public LinksHouse LinksHouse { get; set; } = LinksHouse.Vanilla;

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

        [SettingName("shuffle_damage_table")]
        [RequiredSetting([Apr2025], DamageTableShuffle.Vanilla)]
        [NoSettingName([Apr2025])]
        public DamageTableShuffle DamageTableShuffle { get; set; } = DamageTableShuffle.Vanilla;

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

        [SettingName("showloot")]
        [RequiredSetting([Apr2025, Base], ShowLoot.Never)]
        [NoSettingName([Apr2025, Base])]
        public ShowLoot ShowLoot { get; set; } = ShowLoot.Never;

        [SettingName("showmap")]
        [RequiredSetting([Apr2025, Base], ShowMap.Map)]
        [NoSettingName([Apr2025, Base])]
        public ShowMap ShowMap { get; set; } = ShowMap.Map;

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

        [NoSettingName]
        [RequiredSetting([Apr2025], MirrorSettings.Normal)]
        public MirrorSettings Mirror { get; set; } = MirrorSettings.Normal;

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

        [SettingName("ow_fluteshuffle")]
        [NoSettingName([Apr2025])]
        public FluteShuffle FluteShuffle { get; set; } = FluteShuffle.Vanilla;

        [NoSettingName]
        [RequiredSetting([Apr2025], TileSwap.Vanilla)]
        public TileSwap TileSwap { get; set; } = TileSwap.Vanilla;

        [SettingName("damage_challenge")]
        [NoSettingName([Apr2025])]
        public DamageChallengeMode DamageChallenge { get; set; } = DamageChallengeMode.Normal;

        [NoSettingName]
        public Hints Hints { get; set; } = Hints.Off;
    }

    public enum RandomizerInstance
    {
        [RandomizerName(BaseRandomizer.Name)] Base,
        [RandomizerName(Apr2025Randomizer.Name)] Apr2025,
        [RandomizerName(BaseRandomizer.DungeonMapName)] DungeonMap,
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
        [SettingName("triforcehunt")] TriforceHunt,
        [SettingName("bosshunt")] BossHunt,
        GanonHunt,
        Trinity,
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

    public enum BossRequirement {
        [JsonStringEnumMemberName("0of10")] [SettingName("0")] Bosses0of10,
        [JsonStringEnumMemberName("1of10")] [SettingName("1")] Bosses1of10,
        [JsonStringEnumMemberName("2of10")] [SettingName("2")] Bosses2of10,
        [JsonStringEnumMemberName("3of10")] [SettingName("3")] Bosses3of10,
        [JsonStringEnumMemberName("4of10")] [SettingName("4")] Bosses4of10,
        [JsonStringEnumMemberName("5of10")] [SettingName("5")] Bosses5of10,
        [JsonStringEnumMemberName("6of10")] [SettingName("6")] Bosses6of10,
        [JsonStringEnumMemberName("7of10")] [SettingName("7")] Bosses7of10,
        [JsonStringEnumMemberName("8of10")] [SettingName("8")] Bosses8of10,
        [JsonStringEnumMemberName("9of10")] [SettingName("9")] Bosses9of10,
        [JsonStringEnumMemberName("10of10")] [SettingName("10")] Bosses10of10,

        [JsonStringEnumMemberName("0of12")] [SettingName("0")] [AdditionalSetting("--bosshunt_include_agas")] Bosses0of12,
        [JsonStringEnumMemberName("1of12")] [SettingName("1")] [AdditionalSetting("--bosshunt_include_agas")] Bosses1of12,
        [JsonStringEnumMemberName("2of12")] [SettingName("2")] [AdditionalSetting("--bosshunt_include_agas")] Bosses2of12,
        [JsonStringEnumMemberName("3of12")] [SettingName("3")] [AdditionalSetting("--bosshunt_include_agas")] Bosses3of12,
        [JsonStringEnumMemberName("4of12")] [SettingName("4")] [AdditionalSetting("--bosshunt_include_agas")] Bosses4of12,
        [JsonStringEnumMemberName("5of12")] [SettingName("5")] [AdditionalSetting("--bosshunt_include_agas")] Bosses5of12,
        [JsonStringEnumMemberName("6of12")] [SettingName("6")] [AdditionalSetting("--bosshunt_include_agas")] Bosses6of12,
        [JsonStringEnumMemberName("7of12")] [SettingName("7")] [AdditionalSetting("--bosshunt_include_agas")] Bosses7of12,
        [JsonStringEnumMemberName("8of12")] [SettingName("8")] [AdditionalSetting("--bosshunt_include_agas")] Bosses8of12,
        [JsonStringEnumMemberName("9of12")] [SettingName("9")] [AdditionalSetting("--bosshunt_include_agas")] Bosses9of12,
        [JsonStringEnumMemberName("10of12")] [SettingName("10")] [AdditionalSetting("--bosshunt_include_agas")] Bosses10of12,
        [JsonStringEnumMemberName("11of12")] [SettingName("11")] [AdditionalSetting("--bosshunt_include_agas")] Bosses11of12,
        [JsonStringEnumMemberName("12of12")] [SettingName("12")] [AdditionalSetting("--bosshunt_include_agas")] Bosses12of12,
    }

    public enum GanonItem {
        Silver,
        Boomerang,
        Hookshot,
        Powder,
        [SettingName("fire_rod")] FireRod,
        [SettingName("ice_rod")] IceRod,
        Bombos,
        Ether,
        Quake,
        Hammer,
        Bee,
        Somaria,
        Byrna,
        Random,
        None,
    }

    public enum EntranceShuffle {
        Vanilla,
        Full,
        Crossed,
        Swapped,
        [SettingName("insanity")] Decoupled,
    }

    public enum LinksHouse {
        Vanilla,
        [AdditionalSetting("--shufflelinks")] Shuffled,
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

    public enum DamageTableShuffle {
        Vanilla,
        Randomized,
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

    public enum ShowLoot {
        Never,
        Presence,
        Compass,
        Always,
    }

    public enum ShowMap {
        Visited,
        Map,
        Always,
    }

    public enum BootsSettings {
        Normal,
        [AdditionalSetting("--pseudoboots")] Pseudoboots,
        [AddStartingItems("Pegasus_Boots")] Starting,
    }

    public enum MirrorSettings {
        Normal,
        [AdditionalSetting("--mirrorscroll")] Scroll,
        [AddStartingItems("Magic_Mirror")] Starting,
    }

    public enum FluteSettings {
        Normal,
        [AdditionalSetting("--flute_mode=pseudo")] Pseudoflute,
        [AdditionalSetting("--flute_mode=active")] Preactivated,
        [AddStartingItems("Ocarina_(Activated)")] Starting,
    }

    public enum DarkRoomSettings {
        [SettingName("require_lamp")] RequireLamp,
        [SettingName("always_light_cone")] AlwaysLightCone,
        [SettingName("no_dark_rooms")] NoDarkRooms,
        [SettingName("require_lamp")] [AddStartingItems("Lamp")] StartingLamp,
        [SettingName("always_in_logic")] AlwaysInLogic,
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

    public enum FluteShuffle {
        Vanilla,
        Random,
        Balanced,
    }

    public enum FollowerShuffle {
        Vanilla,
        [AdditionalSetting("--shuffle_followers")] Shuffled,
    }

    public enum TileSwap {
        Vanilla,
        [AdditionalSetting("--ow_mixed")] TileSwap,
    }

    public enum DamageChallengeMode {
        Normal,
        OHKO,
        Gloom,
    }

    public enum Hints {
        Off,
        [AdditionalSetting("--hints")] On,
    }
}
