using CatCafeGameServer.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Requests;
using SharedLibrary.Responses;

namespace CatCafeGameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    public IActionResult Register(AuthRequest request)
    {
        var result = _authService.Register(request.Username, request.Password);
        return !result.success
            ? BadRequest(result.content)
            : Login(request);
    } 
    
    [HttpPost("login")]
    public IActionResult Login(AuthRequest request)
    {
        var result = _authService.Login(request.Username, request.Password);
        if (!result.success)
            return BadRequest(result.token);

        return Ok(new AuthResponse { Token = result.token });
    }
}