using Microsoft.AspNetCore.Identity.UI.Services;

namespace SauGYM.Services
{
    // Bu sınıf "Mail atarmış gibi" yapar ama aslında hiçbir şey yapmaz.
    // Hatanın çözülmesi için gereklidir.
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Burası boş, mail atmıyoruz. İleride gerçek mail ayarı yapılabilir.
            return Task.CompletedTask;
        }
    }
}