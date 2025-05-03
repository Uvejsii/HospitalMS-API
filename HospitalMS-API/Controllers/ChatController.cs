using AutoMapper;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using HospitalMS_API.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HospitalMS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IMapper _mapper;
        public ChatController(IUnitOfWork unitOfWork, IHubContext<ChatHub> hubContext, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDto sendMessageRequestDto)
        {
            if (string.IsNullOrEmpty(sendMessageRequestDto.Message))
            {
                return BadRequest(new { message = "Message cannot be empty." });
            }

            var chatMessage = _mapper.Map<ChatMessage>(sendMessageRequestDto);
            chatMessage.SentAt = DateTime.UtcNow;
            chatMessage.IsRead = false;

            await _unitOfWork.Chat.CreateAsync(chatMessage);
            await _unitOfWork.SaveAsync();

            await _hubContext.Clients.Users(sendMessageRequestDto.SenderId, sendMessageRequestDto.ReceiverId)
                .SendAsync("ReceiveMessage", sendMessageRequestDto.SenderId, sendMessageRequestDto.Message);

            return Ok(new { message = "Message sent successfully." });
        }

        [HttpGet]
        [Route("GetMessages/{senderId}/{receiverId}")]
        public async Task<IActionResult> GetMessages(string senderId, string receiverId)
        {
            var senderExists = await _unitOfWork.Auth.GetAsync(u => u.Id == senderId) != null;
            var receiverExists = await _unitOfWork.Auth.GetAsync(u => u.Id == receiverId) != null;

            if (!senderExists || !receiverExists)
            {
                return NotFound(new { message = "One or both users not found." });
            }

            var messages = await _unitOfWork.Chat.GetAllAsync(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                                                                    (m.SenderId == receiverId && m.ReceiverId == senderId));

            if (!messages.Any())
            {
                return Ok(new List<ChatMessageResponseDto>());
            }

            return Ok(_mapper.Map<List<ChatMessageResponseDto>>(messages));
        }

        [HttpGet]
        [Route("GetUnreadMessages/{receiverId}")]
        public async Task<IActionResult> GetUnreadMessages(string receiverId)
        {
            var unreadMessages = await _unitOfWork.Chat.GetAllAsync(m => (m.ReceiverId == receiverId && !m.IsRead));

            if (unreadMessages == null || !unreadMessages.Any())
            {
                return NotFound(new { message = "No unread messages found." });
            }

            return Ok(unreadMessages);
        }
    }
}
