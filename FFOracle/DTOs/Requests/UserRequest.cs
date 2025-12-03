namespace FFOracle.DTOs.Requests;

//Data to create a new user
public class SignUpRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Fullname { get; set; }
    public string? PhoneNumber { get; set; }
    public bool AllowEmails { get; set; }
}

//Data to update user info
public class UpdateUserRequest
{
    public string? Fullname { get; set; }
    public string? PhoneNumber { get; set; }
    public bool AllowEmails { get; set; } = true;
    public int TokensLeft { get; set; } = 0;
}
