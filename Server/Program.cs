using System.Net;
using System.Net.Sockets;
using Game.Core.Network;
using Game.Server.Chat;
using Game.Server.Login;
using Game.Server.Network;

namespace Game.Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ClientIdGenerator clientIdGenerator = new ClientIdGenerator();
            TcpClientSessionHub tcpClientSessionHub = new TcpClientSessionHub(100);
            PacketRouter packetRouter = new PacketRouter();

            LoginService loginService = new LoginService(packetRouter); // 로그인 기능 제공
            ChatService chatService = new ChatService(tcpClientSessionHub, packetRouter); // 채팅 기능 제공

            TcpListener listner = new TcpListener(IPAddress.Any, 7777);
            listner.Start();

            while (true)
            {
                Socket socket = await listner.AcceptSocketAsync();
                Console.WriteLine($"[Server] Accepted {socket.RemoteEndPoint}");

                int clientId = clientIdGenerator.AssignClientId();

                var session = new TcpClientSession(clientId, socket);

                if (clientId < 0)
                {
                    session.Send(new S_ConnectionFailure()
                    {
                        Reason = "Server is fulled."
                    });

                    // TODO: 에러메시지 전송 완료될 때까지 기다린 후에 세션 자동 종료 기능 필요함
                    session.Dispose();
                    break;
                }

                session.OnPacketReceived += packetRouter.Dispatch;
                session.OnPacketReceived += (packet, session) => Console.WriteLine($"[Server] Received packet {packet.PacketId} from {session.ClientId}");
                tcpClientSessionHub.Add(session);
                session.Start();
            }
        }
    }
}
