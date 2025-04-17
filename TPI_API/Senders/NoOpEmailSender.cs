using Microsoft.AspNetCore.Identity;

namespace TPI_API.Senders;

public class NoOpEmailSender<TUser> : IEmailSender<TUser> where TUser : class
{
    public Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink) => Task.CompletedTask;

    public Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink) => Task.CompletedTask;

    public Task SendEmailAsync(TUser user, string email, string subject, string htmlMessage) => Task.CompletedTask;

    public Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }
}
