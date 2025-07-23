using Game.Core.Network;
using Game.Server.Network;

namespace Game.Server.Chat
{
    /// <summary>
    /// 클라이언트의 Chat 패킷을 수신하면 다른 클라이언트들에게 해당 클라이언트의 패킷을 브로드캐스트 해줌.
    /// </summary>
    class ChatService
    {
        public ChatService(TcpClientSessionHub hub, PacketRouter packetRouter)
        {
            _hub = hub;
            packetRouter.Register(PacketId.C_ChatSend, C_ChatSend_Handle);
        }

        private readonly TcpClientSessionHub _hub;

        private void C_ChatSend_Handle (IPacket packet, TcpClientSession sender)
        {
            string text = ((C_ChatSend)packet).Text;
            S_ChatSend packetToSend = new S_ChatSend
            {
                SenderId = sender.ClientId,
                Text = text
            };

            _hub.Broadcast(packetToSend);
        }
    }
}
