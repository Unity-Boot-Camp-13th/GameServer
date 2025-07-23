using Game.Core.Network;
using Game.Server.Network;

namespace Game.Server.Login
{
    class LoginService
    {
        public LoginService(PacketRouter router)
        {
            router.Register(PacketId.C_Login, C_Login_Handle);
        }

        private async void C_Login_Handle(IPacket packet, TcpClientSession session)
        {
            try
            {
                var p = ((C_Login)packet);

                string jwt = await AuthFacade.LoginAsync(AuthApiSettings.BASE_URL, p.Id, p.Pw);

                session.Send(new S_LoginResult
                {
                    Success = true,
                    AssignedClientId = session.ClientId,
                    Message = "Login OK"
                });
            }
            catch (Exception ex)
            {
                session.Send(new S_LoginResult
                {
                    Success = false,
                    AssignedClientId = -1,
                    Message = ex.ToString()
                });
            }
        }
    }
}
