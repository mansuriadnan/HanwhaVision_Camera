using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Services;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Utilities;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.Extensions.Options;

namespace HanwhaClient.Application.Services
{
    public class ExceptionLogService : IExceptionLogService
    {
        private readonly IExceptionLogRepository _exceptionLogRepository;
        private readonly IExceptionLogArchiveRepository _exceptionLogArchiveRepository;
        private readonly EmailSenderService _emailSenderService;
        private readonly Logs _logs;
        private readonly IEmailTemplateService _emailTemplateService;
       

        public ExceptionLogService(IExceptionLogRepository exceptionLogRepository, IExceptionLogArchiveRepository exceptionLogArchiveRepository, EmailSenderService emailSenderService,
             IOptions<Logs> logs, IEmailTemplateService emailTemplateService
             )
        {
            this._exceptionLogRepository = exceptionLogRepository;
            _exceptionLogArchiveRepository = exceptionLogArchiveRepository;
            _emailSenderService = emailSenderService;
            _logs = logs.Value;
            _emailTemplateService = emailTemplateService;
        }

        public async Task<string> SaveExceptionLogAsync(ExceptionLog exceptionLog)
        {
            var result = await _exceptionLogRepository.InsertAsync(exceptionLog);
            return await Task.FromResult(result);
        }

        public async Task<ExceptionLogsResponse> GetExceptionLogsAsync(ExceptionLogsRequest request)
        {
            var data = await _exceptionLogRepository.GetExceptionLogs(request);
            ExceptionLogsResponse exceptionLogsResponse = new ExceptionLogsResponse
            {
                ExceptionLogsDetails = data.ExceptionLogsDetails.Select(x => new ExceptionLogsDetails
                {
                    ExceptionMessage = x.ExceptionMessage,
                    ExceptionType = x.ExceptionType,
                    HttpMethod = x.HttpMethod,
                    IsSuccess = x.IsSuccess,
                    LoggedAt = x.LoggedAt,
                    QueryString = x.QueryString,
                    RequestBody = x.RequestBody,
                    RequestPath = x.RequestPath,
                    RequestTime = x.RequestTime,
                    ResponseBody = x.ResponseBody,
                    ResponseTime = x.ResponseTime,
                    StackTrace = x.StackTrace,
                    StatusCode = x.StatusCode,
                    id = x.Id
                }).AsEnumerable(),
                TotalCount = data.TotalCount,
            };

            return exceptionLogsResponse;
            
        }

        public async Task<int> ProcessExceptionLogsRetentionData(int retentionPeriod)
        {
            var data = await RetentionHelper.ProcessRetentionData<ExceptionLog, ExceptionLogArchive>(
                             _exceptionLogRepository,                // IRepositoryBase<VehicleCount>
                             _exceptionLogArchiveRepository,    // IRepositoryBase<VehicleCountArchive>
                             _exceptionLogRepository,                // IRetentionRepository<VehicleCount>
                             x => new ExceptionLogArchive
                             {
                                 Id = x.Id,
                                 HttpMethod = x.HttpMethod,
                                 QueryString = x.QueryString,
                                 RequestBody = x.RequestBody,
                                 StatusCode = x.StatusCode,
                                 ResponseBody = x.ResponseBody,
                                 RequestTime = x.RequestTime,
                                 ResponseTime = x.ResponseTime,
                                 IsSuccess = x.IsSuccess,
                                 ExceptionMessage = x.ExceptionMessage,
                                 StackTrace = x.StackTrace,
                                 ExceptionType = x.ExceptionType,
                                 LoggedAt = x.LoggedAt,
                                 UserId = x.UserId,
                                 RequestPath = x.RequestPath,
                                 IsDeleted = x.IsDeleted,
                                 DeletedOn = x.DeletedOn,
                                 CreatedOn = x.CreatedOn,
                                 CreatedBy = x.CreatedBy,
                                 UpdatedOn = x.UpdatedOn,
                                 UpdatedBy = x.UpdatedBy
                             },
                            retentionPeriod: retentionPeriod);

            return data;
        }

        public async Task<bool> SendExceptionAlertEmail(SendExceptionAlertEmailRequest request)
        {
            EmailTemplates WelcomeUserEmailTemplate = await _emailTemplateService.GetEmailTemplateByTitle("Exception Alert");
            if (WelcomeUserEmailTemplate != null)
            {
                var emailBody = WelcomeUserEmailTemplate.EmailTemplateHtml;


                // Replace placeholders
                string formattedTime =
                    DateTime.TryParse(request.ExceptionTime, null,
                        System.Globalization.DateTimeStyles.RoundtripKind,
                        out var dt)
                    ? dt.ToString("dd MMM yyyy HH:mm")
                    : "";
                emailBody = emailBody.Replace("[[Time]]", formattedTime);
                emailBody = emailBody.Replace("[[ExceptionType]]", request.ExceptionType);
                emailBody = emailBody.Replace("[[RequestPath]]", request.RequestPath);
                emailBody = emailBody.Replace("[[ExceptionMessage]]", request.ExceptionMessage);
                emailBody = emailBody.Replace("[[StackTrace]]", request.StackTrace);

                await _emailSenderService.SendEmailAsync(
                    new List<string> { _logs.ExceptionLogsEmailAddr },
                    null,
                    null,
                    WelcomeUserEmailTemplate.EmailTemplateTitle,
                    emailBody,
                    null
                );
            }
            return true;
        }
    }
}
