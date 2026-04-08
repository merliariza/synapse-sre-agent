using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    public AuthController(IUserService userService) => _userService = userService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        var result = await _userService.RegisterAsync(model);
        return result.IsAuthenticated ? Ok(result) : BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto model)
    {
        var result = await _userService.LoginAsync(model);
        return result.IsAuthenticated ? Ok(result) : Unauthorized(result);
    }
}