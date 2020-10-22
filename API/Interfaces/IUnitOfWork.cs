using System.Threading.Tasks;

namespace API.Interfaces
{
  public interface IUnitOfWork
  {
    IUserRepository UserRepository { get; }

    IMessageRepository MessageRepository { get; }

    ILikesRepository LikesRepository { get; }

    /* 
    to save all teh changes
     */
    Task<bool> Complete();

    /* 
    to see if the EF has been tracking or has any changes
     */
    bool HasChanges();
  }
}