using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
  /* 
  Photos are only associated with AppUsers
   */
  [Table("Photos")]
  public class Photo
  {
    public int Id { get; set; }
    public string Url { get; set; }
    public bool IsMain { get; set; }

    // cloudinary publicId
    public string PublicId { get; set; }

    public AppUser AppUser { get; set; }
    public int AppUserId { get; set; }
  }
}