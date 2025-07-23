using Game.Core.Network;

namespace Game.Server.Network
{
    /// <summary>
    /// 서비스와 세션 관리 로직을 분리하기 위함
    /// 서비스가 패킷별 기능을 라우터에 주입하면 
    /// 세션관리 로직에서는 세션이 데이터를 수신했을 때 기능을 라우터를 통해 호출
    /// </summary>
    class PacketRouter
    {
        private readonly Dictionary<PacketId, Action<IPacket, TcpClientSession>> _table = new();

        /// <summary>
        /// 외부 서비스로직을 구독하기 위한 함수
        /// </summary>
        /// <param name="packetId"> 수신 패킷 종류 </param>
        /// <param name="handler"> 서비스 로직 </param>
        public void Register(PacketId packetId, Action<IPacket, TcpClientSession> handler)
        {
            _table[packetId] = handler;
        }

        /// <summary>
        /// 패킷 수신되었을 때 해당 패킷 종류에 따른 서비스가 있다면 실행
        /// </summary>
        /// <param name="packet"> 수신한 패킷 </param>
        /// <param name="session"> 수신한 클라이언트 세션 </param>
        public void Dispatch(IPacket packet, TcpClientSession session)
        {
            if (_table.TryGetValue(packet.PacketId, out var handler))
            {
                try 
                {
                    handler.Invoke(packet, session);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{nameof(PacketRouter)}] : Handler exception {ex}");
                }
            }
        }
    }
}
