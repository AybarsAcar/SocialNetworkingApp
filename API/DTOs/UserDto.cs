namespace API.DTOs
{
  public class UserDto
  {
    public string Username { get; set; }
    public string Token { get; set; }
    public string KnownAs { get; set; }

    // theirm main photo
    public string PhotoUrl { get; set; }

    public string Gender { get; set; }
  }
}