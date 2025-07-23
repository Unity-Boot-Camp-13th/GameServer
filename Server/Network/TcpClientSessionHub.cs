using System.Collections.Concurrent;
using Game.Core.Network;

namespace Game.Server.Network
{
    class TcpClientSessionHub
    {
        public TcpClientSessionHub(int capacity)
        {
            _map = new ConcurrentDictionary<int, TcpClientSession>(Environment.ProcessorCount, capacity);
        }

        private readonly ConcurrentDictionary<int, TcpClientSession> _map;

        public IEnumerable<TcpClientSession> All => _map.Values;

        /// <summary>
        /// 세션 등록
        /// </summary>
        public void Add(TcpClientSession session)
        {
            if (_map.TryAdd(session.ClientId, session))
            {
                // 세션이 끊기면 알아서 Dictionary 에서 제거
                session.OnDisconnected += () => Remove(session.ClientId);
            }
        }

        /// <summary>
        /// 세션 해제
        /// </summary>
        public void Remove(int clientId)
        {
            if(_map.TryRemove(clientId, out var session))
            {
            }
        }

        /// <summary>
        /// 세션 조회
        /// </summary>
        public bool TryGet(int clientId, out TcpClientSession session)
        {
            return _map.TryGetValue(clientId, out session);
        }

        public void Broadcast(IPacket packet)
        {
            foreach (var session in _map.Values)
                session.Send(packet);
        }

        public void BroadcastToOthers(IPacket packet, int clientIdExclusive)
        {
            foreach (var session in _map.Values)
            {
                if (session.ClientId != clientIdExclusive)
                {
                    session.Send(packet);
                }
            }
        }
    }
}
