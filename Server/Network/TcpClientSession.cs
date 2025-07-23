using System.Net.Sockets;
using Game.Core.Network;

namespace Game.Server.Network
{
    /// <summary>
    /// 서버가 클라이언트마다 보관하는 세션
    /// </summary>
    class TcpClientSession : TcpSession
    {
        public int ClientId;
        public event Action<IPacket, TcpClientSession> OnPacketReceived;


        public TcpClientSession(int clientId, Socket socket, int buffersize = 16384) : base(socket, buffersize)
        {
            ClientId = clientId;
        }

        protected override void OnPacket(byte[] body)
        {
            IPacket packet = PacketFactory.FromBytes(body);
            OnPacketReceived?.Invoke(packet, this);
        }

        public void Send(IPacket packet)
        {
            Send(PacketFactory.ToBytes(packet));
        }
    }
}
