using HanwhaAdminApi.Infrastructure.Interfaces;
using HanwhaAdminApi.Model.Common;
using HanwhaAdminApi.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaAdminApi.Application.Services.Migrations.Migratation
{
    [Migration(2025011604, "Add MAC Address field in generate license email template")]
    public class UpdateGenerateLicenseEmailTempScript : IMigration
    {  
        public UpdateGenerateLicenseEmailTempScript()
        {
            
        }

        public async Task RunAsync(IMongoDatabase database)
        {
            var emailTemplatenCollection = database.GetCollection<EmailTemplate>(AppDBConstants.EmailTemplates);
            var userCollection = database.GetCollection<UserMaster>(AppDBConstants.UserMaster);
            var systemAdminFilter = Builders<UserMaster>.Filter.Eq(u => u.Username, "sysadmin");
            var existingSystemAdmin = await userCollection.Find(systemAdminFilter).Project(x => x.Id).FirstOrDefaultAsync();
            var id = await emailTemplatenCollection.Find(x => x.EmailTemplateName == "Generate License").Project(x => x.Id).FirstOrDefaultAsync();
            if (id != null)
            {
                var filterId = Builders<EmailTemplate>.Filter.Eq(x => x.Id, id);
                string emailTemplateHtml = "<body style=\"width: 100%; font-family: 'Calibri', sans-serif; font-size: 11pt; line-height: 1.6; color: #333;\">\r\n  <p><strong>Dear [Distributor Name],</strong></p>  <p>I hope this email finds you well.</p>   <p>We have generated a new <strong>[Trial / Permanent]</strong> license for <strong>[Customer Name]</strong>, associated with your distribution network. Please find the license details below:</p> <ul>   <li><strong>Customer Name:</strong> [Customer Name]   </li>\r\n\t\t\r\n\t\t  <li><strong>License Start Date:</strong>\r\n\t\t[Start Date]\r\n\t\t</li>\r\n\t\t\r\n\t\t  \r\n\t\t[Expiry Date]\r\n\t\t \r\n\t\t\r\n\t\t  <li><strong>Number of Users Allowed:</strong>\r\n\t\t[No. of Users]\r\n\t\t</li>\r\n\t\t\r\n\t\t  <li><strong>Number of Channels Allowed:</strong>\r\n\t\t[No of Cameras]\r\n\t\t</li>\r\n\t\t\r\n\t\t  <li><strong>Site Name:</strong>\r\n\t\t[Site Name]\r\n\t\t</li>\r\n\t\t\r\n <li><strong>MAC Address:</strong>\r\n\t\t[MAC Address]\r\n\t\t</li>\r\n\t\t\r\n    </ul>\r\n\r\n    <p>Additionally, please find the attached files containing important details for your reference:<br />\t\r\n   1. <strong>publickey.pem</strong> – This file contains the customer's public key, which is required for authentication when configuring the license at the customer's site.<br />\r\n        2. <strong>license.lic</strong> – This file contains the generated license details for this customer, which will be used to authenticate the customer's license during configuration.\r\n\r\n</p>\r\n    <p>Kindly proceed with the necessary steps to ensure a smooth activation and deployment for the customer. If you require any further information or assistance, please do not hesitate to reach out.</p>\r\n\r\n    <p><strong>Best regards,<br />\r\n      Vision Insight</strong></p>\r\n</body>\r\n\r\n";
                var update = Builders<EmailTemplate>.Update
                    .Set(x => x.EmailTemplateHtml, emailTemplateHtml)
                    .Set(x => x.UpdatedOn, DateTime.UtcNow)
                    .Set(x => x.UpdatedBy, existingSystemAdmin);

                await emailTemplatenCollection.UpdateOneAsync(filterId, update);
            }
              
           
        }    
    
    }
}
