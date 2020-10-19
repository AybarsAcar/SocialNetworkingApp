namespace API.DTOs
{
  /* 
  return in Likes Repo
  enough info to display in a user card
   */
  public class LikeDto
  {
    public int Id { get; set; }
    public string Username { get; set; }
    public int Age { get; set; }
    public string KnownAs { get; set; }
    public string PhotoUrl { get; set; }
    public string City { get; set; }
  }
}