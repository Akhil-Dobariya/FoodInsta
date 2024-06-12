using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.EmailAPI.Service
{
    public class EmailService : IEmailService
    {
        private DbContextOptions<AppDbContext> _dbOption;

        public EmailService(DbContextOptions<AppDbContext> dbOption)
        {
            this._dbOption = dbOption;
        }

        public async Task EmailCartAndLog(CartDTO cartDTO)
        {
            StringBuilder message = new StringBuilder();

            message.AppendLine("<br/>Cart Email Requested ");
            message.AppendLine("<br/>Total " + cartDTO.CartHeader.CartTotal);
            message.AppendLine("<br/>");
            message.AppendLine("<ul>");
            foreach (var item in cartDTO.CartDetails)
            {
                message.AppendLine($"<li>{item.Product.Name} x {item.Count}</li>");
            }
            message.AppendLine("</ul>");

            await LogAndEmail(message.ToString(), cartDTO.CartHeader.Email);
        }

        private async Task<bool> LogAndEmail(string message, string emailId)
        {
            try
            {
                EmailLogger emailLogger = new EmailLogger()
                {
                    Email=emailId,
                    EmailSent=DateTime.UtcNow,
                    Message=message,
                    IsProcessed=true
                };

                await using var _db = new AppDbContext(_dbOption);
                await _db.EmailLoggers.AddAsync(emailLogger);    
                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
