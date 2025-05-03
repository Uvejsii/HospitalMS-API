using HospitalMS.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository.IRepository
{
    public interface IChatRepository : IRepository<ChatMessage>
    {
        //Task<IEnumerable<ChatMessage>> GetMessagesBetweenUsersAsync(string senderId, string receiverId);
        //Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(string receiverId);
    }
}
