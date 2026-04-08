using SynapseSRE.Application.Interfaces;
using SynapseSRE.Domain.Entities;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public UserService(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
    {
        if (await _unitOfWork.Users.GetByUsernameAsync(model.Username) != null)
            return new AuthResponseDto(false, "El usuario ya existe");

        var user = new User {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
        };

        _unitOfWork.Users.Add(user);
        await _unitOfWork.SaveAsync();
        return new AuthResponseDto(true, "Usuario registrado correctamente");
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto model)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(model.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            return new AuthResponseDto(false, "Credenciales inválidas");

        var token = _tokenService.CreateToken(user);
        return new AuthResponseDto(true, "Login exitoso", token, user.Username);
    }
}