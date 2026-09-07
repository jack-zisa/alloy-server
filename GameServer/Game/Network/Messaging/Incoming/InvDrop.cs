using Common;
using Common.Network;
using Common.Utilities.Collections;
using GameServer.Game.Entities;
using GameServer.Game.Entities.Extensions;
using GameServer.Game.Network.Messaging.Outgoing;
using GameServer.Utilities;

namespace GameServer.Game.Network.Messaging.Incoming;

[Packet(PacketId.INVDROP)]
public record InvDrop : IIncomingPacket {
    public byte SlotId;

    public async Task Handle(User user) {
        var world = user.GameInfo.World;
        GameLogic.Enqueue(() => {
            ref var playerInv = ref world.EntityInventories.Get(user.GameInfo.PlayerId);
            if (playerInv.Id == EntityId.Null)
                return;
            
            var item = playerInv[SlotId];
            if (item == null)
                return;
            
            ref var pos = ref world.EntityStats.Get(user.GameInfo.PlayerId).Pos;
            
            var bag = new Entity(InventoryUtils.GetBagIdFromType(BagType.Pink));
            ref var en = ref world.EnterWorld(ref bag);
            en.Move(world, pos.X, pos.Y);
            ref var bagInv = ref world.EntityInventories.Get(bag.Id);
            bagInv.SetItem(0, item);
            playerInv.SetItem(SlotId, null);
        });
    }

    public void Read(ref SpanReader rdr) {
        rdr.ReadInt32(); // consume the first part of the packet: int ObjectId
        SlotId = rdr.ReadByte();
    }
}