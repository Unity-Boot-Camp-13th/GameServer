using System.Runtime.InteropServices;

namespace Core
{
    interface IUserSessionRepository
    {
        void Create(string sessionId, UserInfo userInfo);
        UserInfo Get(string sessionId);
        void Remove(string sessionId);
        void Update(string sessionId, UserInfo userInfo);
    }
}
