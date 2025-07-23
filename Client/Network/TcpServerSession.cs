using System.Net.Sockets;
using Game.Core.Network;

namespace Game.Client.Network
{
    /// <summary>
    /// Client 가 Server 연결을 관리할 때 사용
    /// </summary>
    class TcpServerSession : TcpSession
    {
        public event Action<IPacket> OnPacketReceived;

        public TcpServerSession(Socket socket, int buffersize = 16384) : base(socket, buffersize)
        {

        }

        public static async Task<TcpServerSession> ConnectAsync(string host, int port)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(host, port);

            TcpServerSession session = new TcpServerSession(socket);
            session.Start();
            return session;
        }

        protected override void OnPacket(byte[] body)
        {
            IPacket packet = PacketFactory.FromBytes(body);

            OnPacketReceived?.Invoke(packet);
        }

        public void Send(IPacket packet)
        {
            Send(PacketFactory.ToBytes(packet));
        }
    }
}
