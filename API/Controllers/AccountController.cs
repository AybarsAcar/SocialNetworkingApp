using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  public class AccountController : BaseApiController
  {
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    public AccountController(DataContext context, ITokenService tokenService)
    {
      this._tokenService = tokenService;
      this._context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
      // check to see if the username already exists
      if (await UserExists(registerDto.Username))
      {
        return BadRequest("Username already exists");
      }

      // using cryptography to hash the users password
      using var hmac = new HMACSHA512();

      var user = new AppUser
      {
        UserName = registerDto.Username.ToLower(),
        PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
        PasswordSalt = hmac.Key
      };

      _context.Users.Add(user);

      await _context.SaveChangesAsync();

      return new UserDto
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user)
      };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
      // find the user
      var user = await _context.Users.SingleOrDefaultAsync(
          x => x.UserName == loginDto.Username
      );

      if (user == null) return Unauthorized("Invalid username");

      // pass in the stored user password salt to initialise this so it can revert
      using var hmac = new HMACSHA512(user.PasswordSalt);

      var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

      // loop over the byte array
      for (int i = 0; i < computedHash.Length; i++)
      {
        if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid credentials");
      }

      // if passes return the user dto
      return new UserDto
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user)
      };
    }

    /* 
    helper method to check if the user already exists
     */
    private async Task<bool> UserExists(string username)
    {
      return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
  }
}