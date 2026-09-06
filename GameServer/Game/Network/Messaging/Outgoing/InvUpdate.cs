using Common.Network;

namespace GameServer.Game.Network.Messaging.Outgoing;

public readonly record struct InvUpdate(int Slot, ushort ItemType) : IOutgoingPacket {
    public PacketId ID => PacketId.INVUPDATE;

    public void Write(ref SpanWriter wtr) {
        wtr.Write(Slot);
        wtr.Write(ItemType);
    }
}