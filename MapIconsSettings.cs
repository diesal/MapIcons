using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using System.Numerics;

namespace MapIcons;
public sealed class MapIconsSettings : ISettings {
    public ToggleNode Enable { get; set; } = new(true);

    public int IconListUpdatePeriod = 100; // milliseconds
    public int RunEveryXTicks = 5;

    public bool DrawOnMinimap = true;
    public bool DrawCachedEntities = true;
    public bool DrawOverLargePanels = false;
    public bool DrawOverFullscreenPanels = false;

    public int IgnoredEntitiesHeight = 100;
    public bool DrawSetingsOpen = true;
    public bool IgnoderEntitesOpen = false;
    public bool NPCIconsOpen = true;
    public bool MiscIconsOpen = true;
    public bool ChestIconsOpen = true;
    public bool StrongboxIconsOpen = false;
    public bool RangeIconsOpen = true;

    //debug
    public bool Debug = false;
    public bool DebugChests = false;
    public bool DebugNPC = false;
    public bool DebugIngameIcons = false;
    public bool DebugMiscIcons = false;

    //ingame icons
    public int AreaTransitionState = 1;
    public int BreachState = 2;
    public int CheckpointState = 1;
    public int QuestObjectState = 1;
    public int NPCState = 1;
    public int NPCHideoutState = 0;
    public int RitualState = 2;
    public int ShrineState = 1;
    public bool ShrineTextShow = false;
    public int WaypointState = 1;
    public int UncategorizedState = 1;

    //NPC icons
    public bool NormalMonsterDraw = true;
    public int NormalMonsterSize = 32;
    public int NormalMonsterIconIndex = 1;
    public Vector4 NormalMonsterTint = new(1.0f, 0.0f, 0.0f, 1.0f);
    public Vector4 NormalMonsterHiddenTint = new(1.0f, 0.6862745f, 0.6862745f, 1.0f);

    public bool MagicMonsterDraw = true;
    public int MagicMonsterSize = 32;
    public int MagicMonsterIconIndex = 1;
    public Vector4 MagicMonsterTint = new(0.0f, 0.57254905f, 1.0f, 1.0f);
    public Vector4 MagicMonsterHiddenTint = new(0.7647059f, 0.8975778f, 1.0f, 1.0f);

    public bool RareMonsterDraw = true;
    public int RareMonsterSize = 32;
    public int RareMonsterIconIndex = 2;
    public Vector4 RareMonsterTint = new(1.0f, 0.8235294f, 0.0f, 1.0f);
    public Vector4 RareMonsterHiddenTint = new(1.0f, 0.97231835f, 0.84313726f, 1.0f);

    public bool UniqueMonsterDraw = true;
    public int UniqueMonsterSize = 32;
    public int UniqueMonsterIconIndex = 3;
    public Vector4 UniqueMonsterTint = new(1.0f, 0.44155842f, 0.0f, 1.0f);
    public Vector4 UniqueMonsterHiddenTint = new(1.0f, 0.84821224f, 0.7254902f, 1.0f);

    public bool SpiritDraw = true;
    public int SpiritSize = 32;
    public int SpiritIconIndex = 3;
    public Vector4 SpiritTint = new Vector4(0.8311689f, 1.0f, 0.0f, 1.0f);
    public Vector4 SpiritHiddenTint = new Vector4(0.9474408f, 1.0f, 0.6883117f, 1.0f);

    public bool FracturingMirrorDraw = true;
    public int FracturingMirrorSize = 32;
    public int FracturingMirrorIconIndex = 129;
    public Vector4 FracturingMirrorTint = new Vector4(0.0f, 0.9411764f, 1.0f, 1.0f);
    public Vector4 FracturingMirrorHiddenTint = new Vector4(0.0f, 0.9411765f, 1.0f, 1.0f);

    public bool MinionDraw = true;
    public int MinionSize = 32;
    public int MinionIconIndex = 0;
    public Vector4 MinionTint = new(0.0f, 1.0f, 0.0f, 1.0f);
    public Vector4 MinionHiddenTint = new(0.6862745f, 1.0f, 0.6862745f, 1.0f);

    public bool NPCDraw = true;
    public int NPCSize = 32;
    public int NPCIconIndex = 161;
    public Vector4 NPCTint = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
    public Vector4 NPCHiddenTint = new Vector4(0.6862745f, 1.0f, 0.6862745f, 1.0f);
    public bool NPCTextShow = false;

    // misc icons
    public bool PlayerDraw = true;
    public int PlayerSize = 32;
    public int PlayerIconIndex = 0;
    public Vector4 PlayerTint = new(0.0f, 1.0f, 0.0f, 1.0f);
    public Vector4 PlayerHiddenTint = new( 0.6862745f, 1.0f, 0.6862745f, 1.0f );
    public bool PlayerTextShow = false;

    // breach chests
    public bool BreachChestNormalDraw = true;
    public int BreachChestNormalSize = 32;
    public int BreachChestNormalIconIndex = 129;
    public Vector4 BreachChestNormalTint = new(0.67532444f, 0.0f, 1.0f, 1.0f);

    public bool BreachChestBossDraw = true;
    public int BreachChestBossSize = 32;
    public int BreachChestBossIconIndex = 130;
    public Vector4 BreachChestBossTint = new(0.96862745f, 0.0f, 0.93088883f, 1.0f);

    //expedition chests
    public bool ExpeditionNormalChestDraw = true;
    public int ExpeditionNormalChestSize = 32;
    public int ExpeditionNormalChestIconIndex = 241;
    public Vector4 ExpeditionNormalChestTint = new(1.0f, 1.0f, 1.0f, 1.0f);

    public bool ExpeditionMagicChestDraw = true;
    public int ExpeditionMagicChestSize = 32;
    public int ExpeditionMagicChestIconIndex = 241;
    public Vector4 ExpeditionMagicChestTint = new(0.0f, 0.57254905f, 1.0f, 1.0f);

    public bool ExpeditionRareChestDraw = true;
    public int ExpeditionRareChestSize = 32;
    public int ExpeditionRareChestIconIndex = 241;
    public Vector4 ExpeditionRareChestTint = new(1.0f, 0.8235294f, 0.0f, 1.0f);

    // chests
    public bool NormalChestDraw = false;
    public int NormalChestSize = 32;
    public int NormalChestIconIndex = 240;
    public Vector4 NormalChestTint = new(1.0f, 1.0f, 1.0f, 1.0f);

    public bool MagicChestDraw = false;
    public int MagicChestSize = 32;
    public int MagicChestIconIndex = 241;
    public Vector4 MagicChestTint = new(0.0f, 0.57254905f, 1.0f, 1.0f);

    public bool RareChestDraw = true;
    public int RareChestSize = 32;
    public int RareChestIconIndex = 241;
    public Vector4 RareChestTint = new(1.0f, 0.8235294f, 0.0f, 1.0f);

    // strongboxes

    public bool UnknownStrongboxDraw = true;
    public int UnknownStrongboxSize = 32;
    public int UnknownStrongboxIconIndex = 241;
    public Vector4 UnknownStrongboxTint = new(1.0f, 0.0f, 0.80519485f, 1.0f);

    public bool ArcanistStrongboxDraw = true;
    public int ArcanistStrongboxSize = 32;
    public int ArcanistStrongboxIconIndex = 241;
    public Vector4 ArcanistStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool ArmourerStrongboxDraw = true;
    public int ArmourerStrongboxSize = 32;
    public int ArmourerStrongboxIconIndex = 241;
    public Vector4 ArmourerStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool BlacksmithStrongboxDraw = true;
    public int BlacksmithStrongboxSize = 32;
    public int BlacksmithStrongboxIconIndex = 241;
    public Vector4 BlacksmithStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool ArtisanStrongboxDraw = true;
    public int ArtisanStrongboxSize = 32;
    public int ArtisanStrongboxIconIndex = 241;
    public Vector4 ArtisanStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool CartographerStrongboxDraw = true;
    public int CartographerStrongboxSize = 32;
    public int CartographerStrongboxIconIndex = 241;
    public Vector4 CartographerStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool ChemistStrongboxDraw = true;
    public int ChemistStrongboxSize = 32;
    public int ChemistStrongboxIconIndex = 241;
    public Vector4 ChemistStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool GemcutterStrongboxDraw = true;
    public int GemcutterStrongboxSize = 32;
    public int GemcutterStrongboxIconIndex = 241;
    public Vector4 GemcutterStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool JewellerStrongboxDraw = true;
    public int JewellerStrongboxSize = 32;
    public int JewellerStrongboxIconIndex = 241;
    public Vector4 JewellerStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool LargeStrongboxDraw = true;
    public int LargeStrongboxSize = 32;
    public int LargeStrongboxIconIndex = 241;
    public Vector4 LargeStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool OrnateStrongboxDraw = true;
    public int OrnateStrongboxSize = 32;
    public int OrnateStrongboxIconIndex = 241;
    public Vector4 OrnateStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool StrongboxDraw = true;
    public int StrongboxSize = 32;
    public int StrongboxIconIndex = 241;
    public Vector4 StrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool DivinerStrongboxDraw = true;
    public int DivinerStrongboxSize = 32;
    public int DivinerStrongboxIconIndex = 241;
    public Vector4 DivinerStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool OperativeStrongboxDraw = true;
    public int OperativeStrongboxSize = 32;
    public int OperativeStrongboxIconIndex = 241;
    public Vector4 OperativeStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool ArcaneStrongboxDraw = true;
    public int ArcaneStrongboxSize = 32;
    public int ArcaneStrongboxIconIndex = 241;
    public Vector4 ArcaneStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    public bool ResearcherStrongboxDraw = true;
    public int ResearcherStrongboxSize = 32;
    public int ResearcherStrongboxIconIndex = 241;
    public Vector4 ResearcherStrongboxTint = new(1.0f, 0.4705882f, 0.0f, 1.0f);

    //ignored entities
    public string ignoredEntities =
    @"# Random Ignores
Metadata/Monsters/InvisibleFire/InvisibleSandstorm_
Metadata/Monsters/InvisibleFire/InvisibleFrostnado
Metadata/Monsters/InvisibleFire/InvisibleFireAfflictionDemonColdDegen
Metadata/Monsters/InvisibleFire/InvisibleFireAfflictionDemonColdDegenUnique
Metadata/Monsters/InvisibleFire/InvisibleFireAfflictionCorpseDegen
Metadata/Monsters/InvisibleFire/InvisibleFireEyrieHurricane
Metadata/Monsters/InvisibleFire/InvisibleIonCannonFrost
Metadata/Monsters/InvisibleFire/AfflictionBossFinalDeathZone
Metadata/Monsters/InvisibleFire/InvisibleFireDoedreSewers
Metadata/Monsters/InvisibleFire/InvisibleFireDelveFlameTornadoSpiked
Metadata/Monsters/InvisibleFire/InvisibleHolyCannon
Metadata/Monsters/InvisibleCurse/InvisibleFrostbiteStationary
Metadata/Monsters/InvisibleCurse/InvisibleConductivityStationary
Metadata/Monsters/InvisibleCurse/InvisibleEnfeeble
Metadata/Monsters/InvisibleAura/InvisibleWrathStationary

# Metadata/Monsters/Labyrinth/GoddessOfJustice
# Metadata/Monsters/Labyrinth/GoddessOfJusticeMapBoss
Metadata/Monsters/Frog/FrogGod/SilverOrb
Metadata/Monsters/Frog/FrogGod/SilverPool
Metadata/Monsters/LunarisSolaris/SolarisCelestialFormAmbushUniqueMap
Metadata/Monsters/Invisible/MaligaroSoulInvisibleBladeVortex
Metadata/Monsters/Daemon
Metadata/Monsters/Daemon/MaligaroBladeVortexDaemon
Metadata/Monsters/Daemon/SilverPoolChillDaemon
Metadata/Monsters/AvariusCasticus/AvariusCasticusStatue
Metadata/Monsters/Maligaro/MaligaroDesecrate

# Ritual
Metadata/Monsters/LeagueRitual/FireMeteorDaemon
Metadata/Monsters/LeagueRitual/GenericSpeedDaemon
Metadata/Monsters/LeagueRitual/ColdRotatingBeamDaemon
Metadata/Monsters/LeagueRitual/ColdRotatingBeamDaemonUber
Metadata/Monsters/LeagueRitual/GenericEnergyShieldDaemon
Metadata/Monsters/LeagueRitual/GenericMassiveDaemon
Metadata/Monsters/LeagueRitual/ChaosGreenVinesDaemon_
Metadata/Monsters/LeagueRitual/ChaosSoulrendPortalDaemon
Metadata/Monsters/LeagueRitual/VaalAtziriDaemon
Metadata/Monsters/LeagueRitual/LightningPylonDaemon";

}