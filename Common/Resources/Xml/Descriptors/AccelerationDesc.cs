using System;
using System.Xml.Linq;
using Common.Network;
using Common.Utilities;

namespace Common.Resources.Xml.Descriptors;

public class AccelerationDesc {
    public float Acceleration;
    public float MinSpeed;
    public float MaxSpeed;
    public int DelayMS;
    public int CooldownMS;
    public int CooldownRepeat;

    public AccelerationDesc(float acceleration, float minSpeed=float.NaN, float maxSpeed=float.NaN, int delay=1, int cooldown=0, int repeat=-1)
    {
        Acceleration = acceleration;
        MinSpeed = minSpeed;
        MaxSpeed = maxSpeed;
        DelayMS = delay;
        CooldownMS = cooldown;
        CooldownRepeat = repeat;
    }

    public AccelerationDesc(XElement xml) {
        Acceleration = xml.GetAttribute<float>("value", 0f);
        MinSpeed = xml.GetAttribute<float>("minSpeed", float.NaN);
        MaxSpeed = xml.GetAttribute<float>("maxSpeed", float.NaN);
        DelayMS = xml.GetAttribute<int>("delayMS", 0);
        CooldownMS = xml.GetAttribute<int>("cooldownMS", 0);
        CooldownRepeat = xml.GetAttribute<int>("repeat", 0);
    }
    
    public AccelerationDesc(ref SpanReader rdr) {
        Acceleration = rdr.ReadSingle();
        MinSpeed = rdr.ReadSingle();
        MaxSpeed = rdr.ReadSingle();
        DelayMS = rdr.ReadInt32();
        CooldownMS = rdr.ReadInt32();
        CooldownRepeat = rdr.ReadInt32();
    }

    public virtual AccelerationDesc Clone() {
        return new AccelerationDesc(Acceleration, MinSpeed, MaxSpeed, DelayMS, CooldownMS, CooldownRepeat);
    }
}