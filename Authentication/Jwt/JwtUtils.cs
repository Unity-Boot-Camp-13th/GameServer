/* 
 Json Web Token
 Json 포맷으로 인증에 필요한 데이터들을 담아서 비밀키로 서명한 토큰
 Header Payload Signature 구성
 Header : 해시 기반 알고리즘
 Payload : 실제 사용할 데이터
 Signature : Header, Payload, Secret Key 를 합쳐서 암호화한 값
*/

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Game.Authentication.Jwt
{
    /// <summary>
    /// Jwt 생성, 검증 헬퍼
    /// </summary>
    public static class JwtUtils
    {
        internal const string SecretKey = "unity_bootcamp_13asldkgjlahweodkglk";
        private static readonly byte[] KeyBytes = Encoding.UTF8.GetBytes(SecretKey);
        internal static readonly SymmetricSecurityKey SymKey = new SymmetricSecurityKey(KeyBytes);

        internal const string Issuer = "AuthService";
        internal const string Audience = "GameClients";

        public static string Generate(string userId, string sessionId, TimeSpan lifetime)
        {
            var creds = new SigningCredentials(SymKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),   // 사용자 식별
                new Claim("sessionId", sessionId),  // 세션 식별
            };

            var now = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                    issuer: Issuer,
                    audience: Audience,
                    claims: claims,
                    notBefore: now,
                    expires: now + lifetime,
                    signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 토큰 검증
        /// </summary>
        public static ClaimsPrincipal Validate(string token)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = SymKey,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, parameters, out _);
        }
    }
}