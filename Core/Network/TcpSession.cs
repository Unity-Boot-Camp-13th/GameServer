using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Core.Network
{
    abstract class TcpSession
    {
        const int KB = 1_024;

        public bool IsConnected => Socket.Connected;

        protected Socket Socket;
        private readonly int _maxSegmentSize = 4 * KB;

        // Send
        private readonly ConcurrentQueue<ArraySegment<byte>> _sendQueue; // Segment 전송 대기열

        // Receive
        private readonly byte[] _receiveBuffer; // 수신 버퍼
        private int _receiveBufferCount;

        /// <summary>
        /// 데이터 수신 루프
        /// </summary>
        private async Task ReceiveLoopAsync()
        {
            while (IsConnected)
            {
                ArraySegment<byte> remainBufferSegment = new ArraySegment<byte>(_receiveBuffer, _receiveBufferCount, _receiveBuffer.Length - _receiveBufferCount); // 남은 버퍼 세그먼트
                int bytesRead = await Socket.ReceiveAsync(remainBufferSegment, SocketFlags.None);

                if (bytesRead == 0)
                {
                    // TODO : Disconnect 처리
                    break;
                }

                _receiveBufferCount += bytesRead;
                // TODO : Handle ReceiveBuffer
            }
        }
    }
}
