using Common.Network;

namespace GameServer.Game.Network.Messaging.Outgoing;

public readonly record struct InvUpdate((int Slot, int ItemType)[] Updates) : IOutgoingPacket {
    public PacketId ID => PacketId.INVUPDATE;

    public void Write(ref SpanWriter wtr) {
        wtr.Write(Updates.Length);
        foreach (var vt in Updates) {
            wtr.Write(vt.Slot);
            wtr.Write(vt.ItemType);
        }
    }
}