namespace Game.Authentication
{
    /// <summary>
    /// 유저의 계정 정보 제공 (ID . PW 저장)
    /// </summary>
    public class UserService
    {
        private static readonly Dictionary<string, string> _users = new()
        {
            { "luke" , "1234" },
            { "suna" , "5678" }
        };

        public bool ValidateCredentials(string id, string pw)
        {
            return _users.TryGetValue(id, out pw);
        }
    }
}