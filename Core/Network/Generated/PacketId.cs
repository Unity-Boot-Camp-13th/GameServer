namespace Game.Core.Network
{
    public enum PacketId : ushort
    {
        S_ConnectionSuccess = 0x0001,
        S_ConnectionFailure = 0x0002,
        C_Ping = 0x0005,
        S_Ping = 0x0006,
        C_Login = 0x0011,
        S_LoginResult = 0x0012,
        S_ChatSend = 0x1010,
        C_ChatSend = 0x1020,
    }
}
