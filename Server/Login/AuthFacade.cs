using System.Net.Http.Json;

namespace Game.Server.Login
{ 
    class AuthFacade
    {
        private static readonly HttpClient _http = new HttpClient();

        // record 한정자
        // 데이터 캡슐 내장 기능을 제공하는 참조형식 정의를 위한 키워드
        public record LoginRequest(string id, string pw);
        public record LoginResponse(string jwt);

        public static async Task<string> LoginAsync(string baseUrl, string id, string pw, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync($"{baseUrl}/auth/login", new LoginRequest(id, pw), cancellationToken);

            response.EnsureSuccessStatusCode(); // status 가 400번대, 500번대처럼 에러관련 코드면 예외던짐
            var body = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
            return body.jwt;
        }
    }
}
