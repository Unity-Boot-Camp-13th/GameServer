namespace Game.Server.Network
{
    /// <summary>
    /// 클라이언트 접속시 고유 Id 를 부여하기 위한 생성기
    /// </summary>
    class ClientIdGenerator
    {
        public ClientIdGenerator(int maxClients = 100)
        {
            _idSet = new HashSet<int>(maxClients);
            _availableIdQueue = new Queue<int>(maxClients);

            for (int i = 0; i < maxClients; i++)
            {
                _availableIdQueue.Enqueue(i);
            }
        }

        readonly HashSet<int> _idSet;
        readonly Queue<int> _availableIdQueue;

        public int AssignClientId()
        {
            if (_availableIdQueue.Count > 0)
            {
                int id = _availableIdQueue.Dequeue();
                _idSet.Add(id);
                return id;
            }
            else
            {
                Console.WriteLine($"[{nameof(ClientIdGenerator)}] : Server is fulled ... reached to max clients");
                return -1;
            }
        }

        public void ReleaseClientId(int clientId)
        {
            if (_idSet.Remove(clientId))
            {
                _availableIdQueue.Enqueue(clientId);
            }
            else
            {
                // TODO: 예외
            }
        }
    }
}
