using HanwhaAdminApi.Infrastructure.Interfaces;
using HanwhaAdminApi.Model.DbEntities;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace HanwhaAdminApi.Core.Services
{
    public class EmailSenderService
    {
 
        public readonly IEmailRequestLogRepository _emailRequestLogRepository;
        private readonly IClientSettingRepository _clientSettingRepository;

        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;


        public EmailSenderService(IEmailRequestLogRepository emailRequestLogRepository,
            IClientSettingRepository clientSettingRepository, IConfiguration configuration)
        {
            _smtpHost = configuration["SmtpSettings:Host"];
            _smtpPort = int.Parse(configuration["SmtpSettings:Port"]);
            _smtpUsername = configuration["SmtpSettings:Username"];
            _smtpPassword = configuration["SmtpSettings:Password"];
            this._emailRequestLogRepository = emailRequestLogRepository;
            this._clientSettingRepository = clientSettingRepository;
        }

        public async Task SendEmailAsync(
            IEnumerable<string> to,
            IEnumerable<string> cc,
            IEnumerable<string> bcc,
            string subject,
            string body,
            IEnumerable<(string fileName, byte[] fileBytes)> attachments)
        {


            //var clientSettings = await _clientSettingRepository.GetClientSettingsAsync();
            //if (clientSettings?.SmtpSettings == null)
            //{
            //    return;
            //}
            //var SmtpSettings = clientSettings.SmtpSettings;
            if (to == null || !to.Any())
            {
                throw new ArgumentException("Recipient list cannot be empty.", nameof(to));
            }

            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true,
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUsername),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            foreach (var recipient in to)
            {
                mailMessage.To.Add(recipient);
            }

            if (cc != null)
            {
                foreach (var ccRecipient in cc)
                {
                    mailMessage.CC.Add(ccRecipient);
                }
            }

            if (bcc != null)
            {
                foreach (var bccRecipient in bcc)
                {
                    mailMessage.Bcc.Add(bccRecipient);
                }
            }

            if (attachments != null)
            {
                foreach (var (fileName, fileBytes) in attachments)
                {
                    if (fileBytes != null && fileBytes.Length > 0)
                    {
                        var memoryStream = new MemoryStream(fileBytes);
                        mailMessage.Attachments.Add(new Attachment(memoryStream, fileName));
                    }
                    else
                    {
                        throw new ArgumentException($"Attachment {fileName} is invalid.");
                    }
                }
            }

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                EmailLogs emailRequestLog = new EmailLogs()
                {
                    To = to,
                    CC = cc,
                    BCC = bcc,
                    Subject = subject,
                    Body = body,
                    Attachments = attachments.Select(x => new AttachmentModel { FileName = x.fileName, FileBytes = x.fileBytes }),
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.Now,
                    IsSuccess = false,
                };
                _emailRequestLogRepository.InsertAsync(emailRequestLog);
                throw;
            }
        }
    }
}
