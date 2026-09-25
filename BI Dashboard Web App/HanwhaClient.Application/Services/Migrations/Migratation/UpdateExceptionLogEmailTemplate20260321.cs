using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services.Migrations.Migratation
{
    [Migration(2025011611, "Update Email template for exception log")]
    public class UpdateExceptionLogEmailTemplate20260321 : IMigration
    {
        private readonly IRoleRepository _roleRepository;

        public UpdateExceptionLogEmailTemplate20260321(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task RunAsync(IMongoDatabase database)
        {
            var emailTemplateCollection = database.GetCollection<EmailTemplate>(AppDBConstants.EmailTemplates);
            var superAdminId = await _roleRepository.GetRoleIdByRoleName("Super Admin");

            await UpdateEmailTemplatesAsync(emailTemplateCollection, superAdminId);

        }

        private async Task UpdateEmailTemplatesAsync(
        IMongoCollection<EmailTemplate> emailTemplateCollection,
        string createdById)
        {

            var filter = Builders<EmailTemplate>.Filter.Eq(t => t.EmailTemplateName, "Exception Alert");

            var update = Builders<EmailTemplate>.Update
                .Set(t => t.EmailTemplateHtml, "<body style=\"width: 100%; font-family: 'Calibri', sans-serif; font-size: 11pt; line-height: 1.6; color: #333;\">\r\n    <p style=\"margin-top: 0px;\"><strong>Hello Team,</strong></p>\r\n\r\n    <p>An exception has occurred in the <strong>Vision Insight Application</strong>. Please find the details below:</p>\r\n\r\n    <p><strong>Exception Details:</strong></p>\r\n\r\n    <ul>\r\n        <li><strong>Time:</strong> [[Time]]</li>\r\n        <li><strong>Exception Type:</strong> [[ExceptionType]]</li>\r\n        <li><strong>Request Path:</strong> [[RequestPath]]</li>\r\n        <li><strong>Exception Message:</strong> [[ExceptionMessage]]</li>\r\n        <li><strong>Stack Trace:</strong> [[StackTrace]]</li>\r\n    </ul>\r\n\r\n    <p><strong>Regards,<br>\r\n    Hanwha Vision</strong></p>\r\n</body>")
                .Set(t => t.UpdatedBy, createdById)
                .Set(t => t.UpdatedOn, DateTime.UtcNow);

            await emailTemplateCollection.UpdateOneAsync(filter, update);
        }        
    }
}
