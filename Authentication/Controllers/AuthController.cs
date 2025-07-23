/*
REST Api
(Represemtational State Transfer) : 특정 자원의 상태를 전송하는 API
특정 자원에 대한 CRUD 를 POST, GET, PUT, DELETE, PATCH 등을 통해 수행함.
*/
using Game.Authentication.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace Game.Authentication.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        public AuthController(IUserSessionRepository sessions, UserService userService)
        {
            _sessions = sessions;
            _userService = userService;
        }

        private readonly IUserSessionRepository _sessions;
        private readonly UserService _userService;

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO dto)
        {
            if (!_userService.ValidateCredentials(dto.id, dto.pw))
            {
                return Unauthorized();
            }
            string sessionId = Guid.NewGuid().ToString();
            _sessions.Create(sessionId, new UserInfo { Id = dto.id, CreatedAt = DateTime.UtcNow });
            var jwt = JwtUtils.Generate(dto.id, sessionId, TimeSpan.FromHours(1));
            return Ok(new { jwt, sessionId });
        }
    }

    /// <summary>
    /// DTO (Date Transfer Object) 데이터 전송시 사용하는 객체
    /// 모든 프로젝트들의 Data Model을 통일하려고 하면
    /// 해당 Data Model 을 공통으로 사용하기 위해서 공용 라이브러리를 빌드해야하고,
    /// 그 라이브러리를 모든 프로젝트에 또 주입하는 것이 프로젝트가 커질수록 번거로워짐.
    /// 특정 계층에서는 데이터 일부만 취급해도 된다고 해도 Data Model 에 의존하면 반드시 필요없는 데이터도 다 취급해야함.
    /// 
    /// DTO 는 프로젝트(계층) 단위로 정의하여 사용하기 때문에 이런 번거로움이 없어짐.
    /// 물론 프로젝트마다 데이터 구성이 완전히 똑같다는 보장이 없으므로 사용시 유의해야 함.
    /// </summary>
    public record LoginDTO(string id, string pw);
}