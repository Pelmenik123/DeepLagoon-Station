using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Database;
using Content.Server.Ghost;
using Content.Server.Maps;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Preferences.Managers;
using Content.Server.ServerUpdates;
using Content.Server.Station.Systems;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Robust.Server;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Console;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
#if EXCEPTION_TOLERANCE
using Robust.Shared.Exceptions;
#endif

namespace Content.Server.GameTicking
{
    public sealed partial class GameTicker : SharedGameTicker
    {
        [Dependency] private IAdminLogManager _adminLogger = default!;
        [Dependency] private IBanManager _banManager = default!;
        [Dependency] private IBaseServer _baseServer = default!;
        [Dependency] private IChatManager _chatManager = default!;
        [Dependency] private IConsoleHost _consoleHost = default!;
        [Dependency] private IGameMapManager _gameMapManager = default!;
        [Dependency] private IGameTiming _gameTiming = default!;
        [Dependency] private ILogManager _logManager = default!;
        SharedMapSystem _mapManager => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SharedMapSystem>();
        [Dependency] private IPrototypeManager _prototypeManager = default!;
        [Dependency] private IRobustRandom _robustRandom = default!;
#if EXCEPTION_TOLERANCE
        [Dependency] private  IRuntimeLog _runtimeLog = default!;
#endif
        [Dependency] private IServerPreferencesManager _prefsManager = default!;
        [Dependency] private IServerDbManager _db = default!;
        ChatSystem _chatSystem => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();
        MapLoaderSystem _loader => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<MapLoaderSystem>();
        SharedMapSystem _map => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SharedMapSystem>();
        GhostSystem _ghost => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<GhostSystem>();
        SharedMindSystem _mind => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SharedMindSystem>();
        PlayTimeTrackingSystem _playTimeTrackings => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<PlayTimeTrackingSystem>();
        PvsOverrideSystem _pvsOverride => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<PvsOverrideSystem>();
        [Dependency] private ServerUpdateManager _serverUpdates = default!;
        SharedAudioSystem _audio => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SharedAudioSystem>();
        StationJobsSystem _stationJobs => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<StationJobsSystem>();
        StationSpawningSystem _stationSpawning => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<StationSpawningSystem>();
        SharedTransformSystem _transform => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SharedTransformSystem>();
        [Dependency] private UserDbDataManager _userDb = default!;
        MetaDataSystem _metaData => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<MetaDataSystem>();
        SharedRoleSystem _roles => IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SharedRoleSystem>();
        [Dependency] private ServerDbEntryManager _dbEntryManager = default!;

        [ViewVariables] private bool _initialized;
        [ViewVariables] private bool _postInitialized;

        [ViewVariables] public MapId DefaultMap { get; private set; }

        private ISawmill _sawmill = default!;

        public override void Initialize()
        {
            base.Initialize();

            DebugTools.Assert(!_initialized);
            DebugTools.Assert(!_postInitialized);

            _sawmill = _logManager.GetSawmill("ticker");
            _sawmillReplays = _logManager.GetSawmill("ticker.replays");

            // Initialize the other parts of the game ticker.
            InitializeStatusShell();
            InitializeCVars();
            InitializePlayer();
            InitializeLobbyBackground();
            InitializeGamePreset();
            DebugTools.Assert(_prototypeManager.Index<JobPrototype>(FallbackOverflowJob).Name == FallbackOverflowJobName,
                "Overflow role does not have the correct name!");
            InitializeGameRules();
            InitializeReplays();
            _initialized = true;
        }

        public void PostInitialize()
        {
            DebugTools.Assert(_initialized);
            DebugTools.Assert(!_postInitialized);

            // We restart the round now that entities are initialized and prototypes have been loaded.
            if (!DummyTicker)
                RestartRound();

            _postInitialized = true;
        }

        public override void Shutdown()
        {
            base.Shutdown();

            ShutdownGameRules();
        }

        private void SendServerMessage(string message)
        {
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
            _chatManager.ChatMessageToAll(ChatChannel.Server, message, wrappedMessage, default, false, true);
        }

        public override void Update(float frameTime)
        {
            if (DummyTicker)
                return;
            base.Update(frameTime);
            UpdateRoundFlow(frameTime);
            UpdateGameRules();
        }
    }
}

