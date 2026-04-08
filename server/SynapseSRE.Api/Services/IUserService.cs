using SynapseSRE.Application.DTOs;

public interface IUserService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto model);
    Task<AuthResponseDto> LoginAsync(LoginDto model);
}