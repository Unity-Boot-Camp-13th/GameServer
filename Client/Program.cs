using Game.Client.Chat;
using Game.Client.Login;
using Game.Client.Network;

namespace Game.Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Task connection = Task.Run(async () =>
            {
                Console.WriteLine("[Client] : 시작. 서버 접속 시도...");
                TcpServerSession serverSession = await TcpServerSession.ConnectAsync(ConnectionSettings.SeverIp, ConnectionSettings.ServerPort);
                Console.WriteLine("[Client] : 서버 접속 완료");

                
                UI_Login uiLogin = new UI_Login(serverSession);
                bool loginResult = await uiLogin.LoginAsync("suna", "5678");

                if (loginResult == false)
                {
                    serverSession.Dispose();
                    return;
                }

                UI_Chat uiChat = new UI_Chat(serverSession);
                await uiChat.SendMessageLoopAsync();
            });

            connection.Wait();
        }
    }
}
