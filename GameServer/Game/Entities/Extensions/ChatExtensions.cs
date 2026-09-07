using Common;
using Common.Utilities.Collections;
using GameServer.Game.Entities.Components;
using GameServer.Game.Entities.Systems;
using GameServer.Game.Network;
using GameServer.Game.Network.Messaging.Outgoing;

namespace GameServer.Game.Entities.Extensions;

public static class ChatExtensions {
    extension(User user) {
        public void SendInfo(string text) {
            user.SendPacket(new Text(
                "",
                EntityId.Null,
                (0, 0),
                -1,
                0,
                null,
                text));
        }

        public void SendParty(string text, ref EntityStats speaker)
        {
            user.SendPacket(new Text(
                speaker.GetString(StatType.Name),
                speaker.Id,
                EntityManager.GetCell(speaker.Pos.X, speaker.Pos.Y),
                speaker.GetInt(StatType.NumStars),
                5,
                "*Party*",
                text));
        }
        
        public void SendPartyAnnounce(string text) {
            user.SendPacket(new Text(
                null,
                EntityId.Null,
                (0, 0),
                -1,
                0,
                "*Party*",
                text));
        }

        public void SendError(string text) {
            user.SendPacket(new Text(
                "*Error*",
                EntityId.Null,
                (0, 0),
                -1,
                0,
                null,
                text));
        }

        public void SendHelp(string text) {
            user.SendPacket(new Text(
                "*Help*",
                EntityId.Null,
                (0, 0),
                -1,
                0,
                null,
                text));
        }

        public void SendEnemy(EntityView entity, string text) {
            user.SendPacket(new Text($"#{entity.Entity.Desc.DisplayName}", entity.Id, EntityManager.GetCell(entity.Stats.Pos), -1, 3, null, text));
        }

        public void SendEnemy(string name, string text) {
            user.SendPacket(new Text($"#{name}", EntityId.Null, (0, 0), -1, 3, null, text));
        }
    }
}