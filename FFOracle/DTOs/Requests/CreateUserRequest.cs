namespace FFOracle.DTOs.Requests;

public class SignUpRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Fullname { get; set; }
    public string? PhoneNumber { get; set; }
    public bool AllowEmails { get; set; }
}
