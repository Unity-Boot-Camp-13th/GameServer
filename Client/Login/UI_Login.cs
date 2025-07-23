using Game.Client.Network;
using Game.Core.Network;

namespace Game.Client.Login
{
    class UI_Login
    {
        public UI_Login(TcpServerSession session)
        {
            _session = session;
            _session.OnPacketReceived += OnReceiveResult;
        }

        TcpServerSession _session;
        S_LoginResult _cachedResult;

        void OnReceiveResult(IPacket packet)
        {
            if (packet is S_LoginResult p)
            {
                if (p.Success)
                    Console.WriteLine($"[Login] : {p.Message}");
                else
                    Console.WriteLine($"[Login] : Failed. {p.Message}");

                _cachedResult = p;
            }
        }

        public async Task<bool> LoginAsync(string id, string pw, CancellationToken cancellationToken = default)
        {
            _cachedResult = null;

            _session.Send(new C_Login
            {
                Id = id,
                Pw = pw
            });

            while (_cachedResult == null)
            {
                await Task.Delay(500);
            }

            return _cachedResult.Success;
        }
    }
}
