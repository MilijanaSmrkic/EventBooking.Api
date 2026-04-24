using EventBooking.Application.Abstractions;
using EventBooking.Application.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EventBooking.Infrastructure.Services
{
    internal sealed class SmtpEmailService : IEmailQueueService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;
        private const int MaxRetries = 3;

        public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task QueueEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_settings.From));
            email.To.Add(MailboxAddress.Parse(message.To));
            email.Subject = message.Subject;
            email.Body = new TextPart("plain") { Text = message.Body };

            Exception? lastException = null;

            for (var attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    using var smtp = new SmtpClient();
                    await smtp.ConnectAsync(
                        _settings.Host,
                        _settings.Port,
                        _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                        cancellationToken);

                    if (!string.IsNullOrWhiteSpace(_settings.Username))
                        await smtp.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

                    await smtp.SendAsync(email, cancellationToken);
                    await smtp.DisconnectAsync(quit: true, cancellationToken);

                    _logger.LogInformation("Email sent → {To} | {Subject}", message.To, message.Subject);
                    return;
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    lastException = ex;
                    _logger.LogWarning(ex,
                        "Email attempt {Attempt}/{Max} failed: {To}", attempt, MaxRetries, message.To);

                    if (attempt < MaxRetries)
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)), cancellationToken);
                }
            }

            _logger.LogError(lastException,
                "Email delivery permanently failed after {Max} attempts: {To} | {Subject}",
                MaxRetries, message.To, message.Subject);
        }
    }
}
