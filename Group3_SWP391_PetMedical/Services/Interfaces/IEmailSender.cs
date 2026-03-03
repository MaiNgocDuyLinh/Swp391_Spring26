namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}

