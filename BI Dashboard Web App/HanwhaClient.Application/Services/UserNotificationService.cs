using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.SignalR;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Utilities;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace HanwhaClient.Application.Services
{
    public class UserNotificationService : IUserNotificationService
    {
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IUserNotificationArchiveRepository _userNotificationArchiveRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        public UserNotificationService(IUserNotificationRepository userNotificationRepository,
            IHubContext<NotificationHub> hubContext,
            IUserNotificationArchiveRepository userNotificationArchiveRepository)
        {
            _userNotificationRepository = userNotificationRepository;
            _hubContext = hubContext;
            _userNotificationArchiveRepository = userNotificationArchiveRepository;
        }
        public async Task<bool> AddUserNotification(string title, string content, string? ActionName, string? ActionParameter)
        {
            var data = new UserNotification
            {
                Title = title,
                Content = content,
                ActionName = ActionName,
                ActionParameter = ActionParameter,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };
            await _userNotificationRepository.InsertAsync(data);
            var jsonMessage = JsonConvert.SerializeObject(data);
            await _hubContext.Clients.All.SendAsync("userNotification", jsonMessage);
            return true;
        }

        public async Task<bool> MarkReadUserNotification(MarkReadNotificationRequest markReadNotificationRequest)
        {
            if (string.IsNullOrEmpty(markReadNotificationRequest.NotificationId))
            {
                return await _userNotificationRepository.MarkAllReadUserNotification(markReadNotificationRequest);
            }
            else
            {
                var update = Builders<UserNotification>.Update
                    .Set(n => n.IsRead, true)
                    .Set(n => n.UpdatedBy, markReadNotificationRequest.UserId)
                    .Set(n => n.UpdatedOn, DateTime.UtcNow);
                return await _userNotificationRepository.UpdateFieldsAsync(markReadNotificationRequest.NotificationId, update);
            }
        }

        public async Task<int> ProcessUserNotificationRetentionData(int retentionPeriod)
        {
            var data = await RetentionHelper.ProcessRetentionData<UserNotification, UserNotificationArchive>(
                             _userNotificationRepository,                // IRepositoryBase<VehicleCount>
                             _userNotificationArchiveRepository,    // IRepositoryBase<VehicleCountArchive>
                             _userNotificationRepository,                // IRetentionRepository<VehicleCount>
                             x => new UserNotificationArchive
                             {
                                 Id = x.Id,
                                 Title = x.Title,
                                 Content = x.Content,
                                 IsRead = x.IsRead,
                                 ActionName = x.ActionName,
                                 ActionParameter = x.ActionParameter,
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
    }
}
