using Common;
using Common.Game;
using GameServer.Game.Entities.Components;

namespace GameServer.Game.Entities.Behaviors.Actions;

public record ConditionEffectBehavior : BehaviorScript {
    private readonly ConditionEffectIndex _condEffect;
    private readonly int _durationMS;
    private readonly bool _persist;

    public ConditionEffectBehavior(ConditionEffectIndex effect, int durationMS = -1, bool persist = false) {
        _condEffect = effect;
        _durationMS = durationMS;
        _persist = persist;
    }

    public override void Start(ref EntityView host) {
        EntityStats stats = host.World.EntityStats.Get(host.Id);
        if (_durationMS == 0) { // Remove effect
            stats.RemoveConditionEffect(_condEffect);
            return;
        }
        
        stats.AddConditionEffect(_condEffect, _durationMS);
    }

    public override void End(ref EntityView host, ref RealmTime time) {
        if (_persist)
            return;
        
        EntityStats stats = host.World.EntityStats.Get(host.Id);
        stats.RemoveConditionEffect(_condEffect);
    }
}