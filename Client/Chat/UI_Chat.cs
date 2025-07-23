using Game.Client.Network;
using Game.Core.Network;

namespace Game.Client.Chat
{
    class UI_Chat
    {
        public UI_Chat(TcpServerSession session)
        {
            _session = session;
            _session.OnPacketReceived += OnReceiveMessage;
        }

        TcpServerSession _session;

        private void OnReceiveMessage(IPacket packet)
        {
            if (packet is S_ChatSend p)
            {
                Console.WriteLine($"[{p.SenderId}] : {p.Text}");
            }
        }

        public async Task SendMessageLoopAsync()
        {
            while (_session.IsConnected)
            {
                string input = await Task.Run(Console.ReadLine).ConfigureAwait(true);

                C_ChatSend packet = new C_ChatSend
                {
                    Text = input
                };

                _session.Send(packet);
            }
        }
    }
}
