public record AuthResponseDto(
    bool IsAuthenticated, 
    string Message, 
    string? Token = null, 
    string? Username = null
);