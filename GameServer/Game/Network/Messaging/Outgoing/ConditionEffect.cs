using Common.Network;

namespace GameServer.Game.Network.Messaging.Outgoing;

public readonly record struct ConditionEffect(string Effect, bool Apply) : IOutgoingPacket {
    public PacketId ID => PacketId.CONDITIONEFFECT;

    public void Write(ref SpanWriter wtr) {
        wtr.WriteUTF(Effect);
        wtr.Write(Apply);
    }
}