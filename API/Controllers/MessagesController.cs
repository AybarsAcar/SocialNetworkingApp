using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  [Authorize]
  public class MessagesController : BaseApiController
  {
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;
    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
      this._mapper = mapper;
      this._messageRepository = messageRepository;
      this._userRepository = userRepository;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
      // get username
      var username = User.GetUsername();

      if (username == createMessageDto.RecipientUsername.ToLower())
      {
        return BadRequest("You cannot send messages to yourself");
      }

      // get hold of both of our users
      var sender = await _userRepository.GetUserByUsernameAsync(username);
      var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

      if (recipient == null) return NotFound();

      var message = new Message
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = sender.UserName,
        RecipientUsername = recipient.UserName,
        Content = createMessageDto.Content
      };

      _messageRepository.AddMessage(message);

      if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

      return BadRequest("Failed to send message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
      messageParams.Username = User.GetUsername();

      var messages = await _messageRepository.GetMessagesForUser(messageParams);

      Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

      return messages;
    }

    // username is the name of the other user
    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
      // get the currently logged in username
      var currentUsername = User.GetUsername();

      return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
    }
  }
}