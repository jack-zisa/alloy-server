using Common.Game;
using Common.Resources.Xml;
using Common.Utilities;
using Common.Utilities.Collections;
using GameServer.Game.Entities.Behaviors;
using GameServer.Game.Worlds;

namespace GameServer.Game.Entities.Components;

public struct EntityBehavior : IEntityIdentifiable, IDisposable {
    private static readonly Logger _log = new(typeof(EntityBehavior));

    public EntityId Id { get; set; }

    public readonly World World;
    private HashSet<State> _activeStates;
    private HashSet<BehaviorTransition> _pastTransitions;
    private StateResourceController _resources;

    public HashSet<State> ActiveStates {
        get { Initialize(); return _activeStates!; }
    }

    public HashSet<BehaviorTransition> PastTransitions {
        get { Initialize(); return _pastTransitions!; }
    }

    public StateResourceController Resources {
        get { Initialize(); return _resources!; }
    }
    
    public EntityId ParentId;

    private readonly string _objectId;

    private State _rootState;
    private State _currentState;

    public EntityBehavior(World world, ref Entity en) {
        Id = en.Id;
        World = world;
        _objectId = XmlLibrary.ObjectDescs[en.ObjectType].ObjectId;
    }
    
    public void Initialize() {
        _activeStates ??= [];
        _pastTransitions ??= [];
        _resources ??= new StateResourceController();
    }

    public void Load() {
        Initialize();
        
        if (!BehaviorLibrary.ClassicBehaviors.TryGetValue(_objectId, out var rootState)) {
            _log.Error($"Behavior not found for '{_objectId}'");
            return;
        }
        
        _rootState = rootState;
        Resources.ClearResources();

        _currentState = rootState.GetDeepState();
        var view = new EntityView(World, Id);
        _currentState.Enter(ref view);
    }

    public void TransitionTo(string targetState, ref RealmTime time) {
        if (_currentState == null)
            return;

        var view = new EntityView(World, Id);
        _currentState.Exit(ref view, ref time);

        if (_rootState.States.TryGetValue(targetState, out var newState)) {
            _currentState.ExitInactiveParent(ref view, time,
                newState); // Calls parent's Exit method if it's not parent of the new State

            _currentState = newState.GetDeepState();
            _currentState.Enter(ref view);
        }
        else {
            _log.Error($"{_objectId}: State {targetState} not found.");
            _currentState = null;
        }
    }

    public void Tick(ref RealmTime time) {
        if (_currentState == null)
            return;

        var view = new EntityView(World, Id);
        var targetState = _currentState.Tick(ref view, ref time);
        if (targetState != null)
            TransitionTo(targetState, ref time);
    }

    public void Dispose() {
        ActiveStates.Clear();
        PastTransitions.Clear();
        Resources.ClearResources();
        _currentState = null;
    }
}