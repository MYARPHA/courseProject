using Microsoft.AspNetCore.Mvc;
using courseProject.Services;

namespace courseProject.Controllers
{
    [Route("api/user/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// POST: api/user/auth/register
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(new { message = "Email и пароль обязательны" });

                if (request.Password.Length < 6)
                    return BadRequest(new { message = "Пароль должен быть не менее 6 символов" });

                var user = await _authService.Register(request);
                var token = _authService.Authenticate(request.Email, request.Password);
                
                _logger.LogInformation($"Пользователь {user.Email} успешно зарегистрирован");

                return Ok(new 
                { 
                    success = true,
                    user = new 
                    { 
                        id = user.UserId,
                        email = user.Email,
                        fullName = user.FullName,
                        role = user.Role
                    },
                    token,
                    expiresIn = 3600
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Ошибка регистрации: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Неожиданная ошибка при регистрации: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Авторизация пользователя
        /// POST: api/user/auth/login
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(new { success = false, message = "Email и пароль обязательны" });

                var token = _authService.Authenticate(request.Email, request.Password);
                
                if (token == null)
                {
                    _logger.LogWarning($"Неудачная попытка входа для {request.Email}");
                    return Unauthorized(new { success = false, message = "Неверный email или пароль" });
                }

                _logger.LogInformation($"Пользователь {request.Email} успешно авторизован");

                return Ok(new 
                { 
                    success = true,
                    token,
                    expiresIn = 3600
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при авторизации: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Внутренняя ошибка сервера" });
            }
        }
    }
}
