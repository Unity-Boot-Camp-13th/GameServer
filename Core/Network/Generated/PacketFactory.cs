namespace Game.Core.Network
{
    public static class PacketFactory
    {

        // 생성자 테이블
        private static readonly Dictionary<PacketId, Func<IPacket>> _constructors = new()
        {
            { PacketId.S_ConnectionSuccess, () => new S_ConnectionSuccess() },
            { PacketId.S_ConnectionFailure, () => new S_ConnectionFailure() },
            { PacketId.C_Ping, () => new C_Ping() },
            { PacketId.S_Ping, () => new S_Ping() },
            { PacketId.C_Login, () => new C_Login() },
            { PacketId.S_LoginResult, () => new S_LoginResult() },
            { PacketId.S_ChatSend, () => new S_ChatSend() },
            { PacketId.C_ChatSend, () => new C_ChatSend() },
        };

        /// <summary>
        /// 데이터 송신 시 원하는 패킷 객체를 Serialize 하기 위함
        /// </summary>
        /// <param name="packet"> 송신 대상 </param>
        /// <returns> Serialized </returns>
        public static byte[] ToBytes(IPacket packet)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((ushort)packet.PacketId);
                packet.Serialize(writer);

                return stream.ToArray();
            }
        }

        /// <summary>
        /// 수신한 데이터를 Deserialize 하기 위함
        /// </summary>
        /// <param name="body"> 수신한 데이터 </param>
        /// <returns> Deserialized </returns>
        public static IPacket FromBytes(byte[] body)
        {
            // Packet Id 도 못 읽으면 잘못된 데이터임
            if (body.Length < sizeof(PacketId))
                return null;

            PacketId packetId = (PacketId)BitConverter.ToUInt16(body, 0);

            // 유효한 Packet Id 인지 
            if (_constructors.TryGetValue(packetId, out Func<IPacket> constructor) == false)
                return null;

            IPacket packet = constructor.Invoke();

            // packetId 만큼을 제외한 순수 데이터만 스트림에 취급
            using (MemoryStream stream = new MemoryStream(body, sizeof(ushort), body.Length - sizeof(ushort)))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                packet.Deserialize(reader);
            }

            return packet;
        }
    }
}