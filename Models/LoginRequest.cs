// DTO para receber os dados do login
public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterRequest
{
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}