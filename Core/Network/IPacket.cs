namespace Game.Core.Network
{
    public interface IPacket
    {
        PacketId PacketId { get; }

        void Serialize(BinaryWriter writer);

        void Deserialize(BinaryReader reader);
    }
}
