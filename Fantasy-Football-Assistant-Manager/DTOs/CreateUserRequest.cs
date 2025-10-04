namespace Fantasy_Football_Assistant_Manager.DTOs;

public class CreateUserRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? TeamName { get; set; }
}
