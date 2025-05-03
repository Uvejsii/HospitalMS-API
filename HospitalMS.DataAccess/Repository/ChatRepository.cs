using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository
{
    public class ChatRepository : Repository<ChatMessage>, IChatRepository
    {
        private ApplicationDbContext _db;
        public ChatRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        //public async Task<IEnumerable<ChatMessage>> GetMessagesBetweenUsersAsync(string senderId, string receiverId)
        //{
        //    return await _db.ChatMessages
        //        .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
        //                    (m.SenderId == receiverId && m.ReceiverId == senderId))
        //        .ToListAsync();
        //}
        //public async Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(string receiverId)
        //{
        //    return await _db.ChatMessages
        //        .Where(m => m.ReceiverId == receiverId && !m.IsRead)
        //        .ToListAsync();
        //}
    }
}
