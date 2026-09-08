#region

using System;
using System.Numerics;
using System.Xml.Linq;
using Common.Network;
using Common.Resources.Xml.Descriptors;
using Common.Utilities;

#endregion

namespace Common.Projectiles.ProjectilePaths;

public class ProjectilePathSegment {
    public readonly PathType Type;
    public float Speed;
    public readonly int TimeOffset;
    public float FixedAngle;
    public int LifetimeMs;
    public AccelerationDesc Acceleration;
    private int _lastAccelerationStep = -1;
    
    protected readonly int _mods;

    public ProjectilePathSegment(PathType pathType, float speed, float? angle = null, int? lifetimeMs = null, AccelerationDesc acceleration = null,
        int? timeOffset = null, params PathSegmentModifier[] mods) {
        Type = pathType;
        Speed = speed;
        TimeOffset = timeOffset ?? 0;
        FixedAngle = angle.Deg2Rad() ?? float.NaN;
        LifetimeMs = lifetimeMs ?? -1;
        Acceleration = acceleration;
        _mods = GetModsFlag(mods);
    }
    
    public bool HasMod(PathSegmentModifier mod) {
        return (_mods & (1 << (int)mod)) != 0;
    }

    protected void ApplyModifiers(ref int elapsedLifetimeMs) {
        if (HasMod(PathSegmentModifier.Boomerang))
            if (elapsedLifetimeMs > LifetimeMs / 2)
                elapsedLifetimeMs = LifetimeMs - elapsedLifetimeMs;
    }

    public void UpdateAcceleration(float elapsedLifetimeMs)
    {
        if (Acceleration == null)
            return;

        var elapsed = elapsedLifetimeMs - Acceleration.DelayMS;

        if (elapsed < 0)
            return;

        if (Acceleration.CooldownMS <= 0)
        {
            if (_lastAccelerationStep >= 0)
                return;

            Speed += Acceleration.Acceleration;
            _lastAccelerationStep = 0;

            ClampAccelerationSpeed();
            return;
        }

        var currentStep = (int)(elapsed / Acceleration.CooldownMS);

        if (Acceleration.CooldownRepeat >= 0)
        {
            currentStep = Math.Min(
                currentStep,
                Acceleration.CooldownRepeat - 1
            );
        }

        var applications = currentStep - _lastAccelerationStep;

        if (applications <= 0)
            return;

        Speed += Acceleration.Acceleration * applications;

        _lastAccelerationStep = currentStep;

        ClampAccelerationSpeed();
    }

    private void ClampAccelerationSpeed()
    {
        if (Acceleration == null)
            return;

        if (!float.IsNaN(Acceleration.MinSpeed))
            Speed = MathF.Max(Speed, Acceleration.MinSpeed);

        if (!float.IsNaN(Acceleration.MaxSpeed))
            Speed = MathF.Min(Speed, Acceleration.MaxSpeed);
    }

    public virtual Vector2 PositionAt(int elapsedLifetimeMs, int projId, float angle) {
        throw new NotImplementedException();
    }

    public Vector2 PositionAtEnd(int projId, float angle) {
        return PositionAt(LifetimeMs, projId, angle);
    }

    public float GetAngle(float angle) {
        if (float.IsNaN(FixedAngle))
            return angle;
        return FixedAngle;
    }

    public virtual void Write(ref SpanWriter wtr) {
        wtr.Write(Speed);
        wtr.Write(LifetimeMs);
        wtr.Write(FixedAngle);
        wtr.Write(TimeOffset);
        wtr.Write(_mods);
    }

    public virtual ProjectilePathSegment Clone()
    {
        ProjectilePathSegment ret = new ProjectilePathSegment(0, 0);
        ret.Acceleration = Acceleration;
        ret._lastAccelerationStep = -1;
        return ret;
    }

    public static ProjectilePathSegment ParsePath(ProjectileDesc projDesc) {
        // No path defined, import path from old system
        ProjectilePathSegment path;
        if (projDesc.Root.HasElement("Amplitude") || projDesc.Root.HasElement("Frequency"))
            path = new AmplitudePath(projDesc.Speed, projDesc.Amplitude, projDesc.Frequency, null, projDesc.LifetimeMS);
        // else if (projDesc.Parametric) {
        //     path = new ParametricPath(projDesc.Speed);
        // }
        else if (projDesc.Wavy)
            path = new WavyPath(projDesc.Speed, null, projDesc.LifetimeMS);
        else if (projDesc.Boomerang)
            path = new BoomerangPath(projDesc.Speed, null, projDesc.LifetimeMS);
        else
            path = new LinePath(projDesc.Speed, null, projDesc.LifetimeMS);

        return path;
    }

    public static ProjectilePathSegment ParsePath(XElement pathElement) {
        if (pathElement == null)
            return new LinePath(10, null, 100);

        var type = pathElement.Value.Trim();
        var lifeTimeMs = pathElement.GetAttribute<int>("lifetimeMs");
        switch (type) {
            case "Line":
                var speed = pathElement.GetAttribute<float>("speed");
                return new LinePath(speed, null, lifeTimeMs);
            case "Wavy":
                speed = pathElement.GetAttribute<float>("speed");
                return new WavyPath(speed, null, lifeTimeMs);
            case "Boomerang":
                speed = pathElement.GetAttribute<float>("speed");
                return new BoomerangPath(speed, null, lifeTimeMs);
            case "Circle":
                var rps = pathElement.GetAttribute<float>("rotationsPerSecond");
                var radius = pathElement.GetAttribute<float>("radius");
                return new CirclePath(rps, radius, null, lifeTimeMs);
            case "Amplitude":
                speed = pathElement.GetAttribute<float>("speed");
                var amplitude = pathElement.GetAttribute<float>("amplitude");
                var frequency = pathElement.GetAttribute<float>("frequency");
                return new AmplitudePath(speed, amplitude, frequency, null, lifeTimeMs);
        }

        return null;
    }

    public static ProjectilePathSegment ParsePath(ProjectileProps props) {
        return props.PathType switch {
            PathType.LinePath => new LinePath(props.Speed, null, props.LifetimeMS),
            PathType.WavyPath => new WavyPath(props.Speed, null, props.LifetimeMS),
            PathType.AmplitudePath => new AmplitudePath(props.Speed, props.Amplitude, props.Frequency, null,
                props.LifetimeMS),
            PathType.BoomerangPath => new BoomerangPath(props.Speed, null, props.LifetimeMS),
            _ => null
        };
    }

    public ProjectilePath ToPath() {
        return new ProjectilePath(LifetimeMs, this);
    }

    private static int GetModsFlag(PathSegmentModifier[] mods) {
        var ret = 0;
        foreach (var mod in mods)
            ret |= 1 << (int)mod;
        return ret;
    }
}

public enum PathSegmentModifier : byte {
    None,
    Boomerang
}