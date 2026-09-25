using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Math;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application
{
    // DELETE this old class
    // public class AppMessageConstants
    // {
    //     public const string RecordRetrieved = "The record has been retrieved successfully.";
    // }

    // CREATE this new class
    public static class MessageKeys
    {
        // License & Authentication
        public const string LicenseAuthenticated = nameof(LicenseAuthenticated);
        public const string InvalidLicense = nameof(InvalidLicense);
        public const string LicenseExpired = nameof(LicenseExpired);

        // Role Management
        public const string RoleExists = nameof(RoleExists);
        public const string RoleNameExits = nameof(RoleNameExits);

        // OTP Related
        public const string OtpSuccessfullyValidated = nameof(OtpSuccessfullyValidated);
        public const string OtpExpiredOrInvalid = nameof(OtpExpiredOrInvalid);
        public const string OtpSuccessfullySentToEmail = nameof(OtpSuccessfullySentToEmail);

        // Email & Password
        public const string EmailNotRegistered = nameof(EmailNotRegistered);
        public const string PasswordSuccessfullyReset = nameof(PasswordSuccessfullyReset);
        public const string PasswordAlreadyReset = nameof(PasswordAlreadyReset);
        public const string OldPasswordDoesNotMatch = nameof(OldPasswordDoesNotMatch);

        // Authorization & Permissions
        public const string UnauthorizedAccessResource = nameof(UnauthorizedAccessResource);
        public const string InsufficientPermissions = nameof(InsufficientPermissions);
        public const string PermissionServiceNotAvailable = nameof(PermissionServiceNotAvailable);
        public const string UnauthorizedAccessAttempt = nameof(UnauthorizedAccessAttempt);

        // License Limits
        public const string UserLimitExceededLicense = nameof(UserLimitExceededLicense);
        public const string DeviceLimitExceededLicense = nameof(DeviceLimitExceededLicense);
        public const string LicenseKeyRequired = nameof(LicenseKeyRequired);
        public const string LicenseInvalidFileTypes = nameof(LicenseInvalidFileTypes);
        public const string LicenseKeyUploaded = nameof(LicenseKeyUploaded);
        public const string LicenseFileNotFound = nameof(LicenseFileNotFound);

        // Dashboard
        public const string DashboardNameAlreadyExist = nameof(DashboardNameAlreadyExist);
        public const string DashboardNameMandatory = nameof(DashboardNameMandatory);

        // Configuration
        public const string GoogleApisave = nameof(GoogleApisave);
        public const string InvalidGoogleKey = nameof(InvalidGoogleKey);
        public const string FtpConfugurationSave = nameof(FtpConfugurationSave);

        // File Generation
        public const string PdfGenerated = nameof(PdfGenerated);
        public const string CsvGenerated = nameof(CsvGenerated);
        public const string ExcelGenerated = nameof(ExcelGenerated);
        public const string FileUploaded = nameof(FileUploaded);
        public const string FileUploadFailure = nameof(FileUploadFailure);

        // CRUD Operations
        public const string RecordAdded = nameof(RecordAdded);
        public const string RecordUpdated = nameof(RecordUpdated);
        public const string RecordDeleted = nameof(RecordDeleted);
        public const string RecordRetrieved = nameof(RecordRetrieved);
        public const string RecordNotInserted = nameof(RecordNotInserted);
        public const string RecordNotFound = nameof(RecordNotFound);

        // User & Login
        public const string UserLoggedIn = nameof(UserLoggedIn);
        public const string UserNotFound = nameof(UserNotFound);
        public const string NewTokenGenerated = nameof(NewTokenGenerated);
        public const string CredetialWrongAddDevice = nameof(CredetialWrongAddDevice);
        public const string ViAdminUserPermisssion = nameof(ViAdminUserPermisssion);

        // Device Management
        public const string DeviceAlreadyExists = nameof(DeviceAlreadyExists);
        public const string DeviceUnavailable = nameof(DeviceUnavailable);
        public const string FileFormatInvalid = nameof(FileFormatInvalid);
        public const string ExcelFileNotSheet = nameof(ExcelFileNotSheet);
        public const string HeaderRowNotExits = nameof(HeaderRowNotExits);
        public const string NoValidDeviceRows = nameof(NoValidDeviceRows);
        public const string AllDeviceUploaded = nameof(AllDeviceUploaded);
        public const string SampleFileNotFound = nameof(SampleFileNotFound);
        public const string ExcelMissingColumn = nameof(ExcelMissingColumn);

        // Database
        public const string RestoreDatabase = nameof(RestoreDatabase);
        public const string AESFileFormat = nameof(AESFileFormat);
        public const string RestoreFailedInvalidFile = nameof(RestoreFailedInvalidFile);
        public const string DatabaseRestoreFailed = nameof(DatabaseRestoreFailed);
        public const string UploadSessionForDB = nameof(UploadSessionForDB);


        // Validation
        public const string InvalidDataModel = nameof(InvalidDataModel);
        public const string DuplicateZoneName = nameof(DuplicateZoneName);
        public const string DuplicateFloor = nameof(DuplicateFloor);

        // Site Management
        public const string SiteUnavailable = nameof(SiteUnavailable);
        public const string ChildSiteUnAvailable = nameof(ChildSiteUnAvailable);
        public const string ChildSiteNotFound = nameof(ChildSiteNotFound);
        public const string ParentSiteNotFound = nameof(ParentSiteNotFound);

        // System
        public const string ApplicationCacheClear = nameof(ApplicationCacheClear);
        public const string SomethingWentWrong = nameof(SomethingWentWrong);

        // SSL Certificate
        public const string PFXCertificateError = nameof(PFXCertificateError);
        public const string PFXFileRequired = nameof(PFXFileRequired);
        public const string PFXFileExtension = nameof(PFXFileExtension);
        public const string PFXFileSize = nameof(PFXFileSize);
        public const string PFXCertificateContainPrivatekey = nameof(PFXCertificateContainPrivatekey);
        public const string CertificateHasExpired = nameof(CertificateHasExpired);
        public const string CertificateNotValid = nameof(CertificateNotValid);
        public const string CertificateExpiresSoon = nameof(CertificateExpiresSoon);
        public const string CertificateNotTrusted = nameof(CertificateNotTrusted);
        public const string InvalidPFXFileIncorrectPassword = nameof(InvalidPFXFileIncorrectPassword);
        public const string ErrorValidatingPFXFile = nameof(ErrorValidatingPFXFile);
        public const string IISSiteNotFound = nameof(IISSiteNotFound);
        public const string InsufficientPermissionsForIIS = nameof(InsufficientPermissionsForIIS);



        // Owner & Vehicle
        public const string FailedToDeleteOwnerOrVehicle = nameof(FailedToDeleteOwnerOrVehicle);

        //Report
        public const string FilterApplied = nameof(FilterApplied);
        public const string CreatedBy = nameof(CreatedBy);
        public const string CreatedOn = nameof(CreatedOn);
        public const string ReportName = nameof(ReportName);
        public const string ReportType = nameof(ReportType);
        public const string Sites = nameof(Sites);
        public const string DateTimeLabel = nameof(DateTimeLabel);
        public const string KeyPerformanceMetric = nameof(KeyPerformanceMetric);
        public const string PerformanceComparisonTable = nameof(PerformanceComparisonTable);
        public const string SummaryInsights = nameof(SummaryInsights);
        public const string OccupancyRateComparison = nameof(OccupancyRateComparison);
        public const string TrafficCompositionPeopleVsVehicles = nameof(TrafficCompositionPeopleVsVehicles);
        public const string Floors = nameof(Floors);
        public const string Zones = nameof(Zones);
        public const string TotalPeopleCount = nameof(TotalPeopleCount);
        public const string AveragePeopleOccupancyRate = nameof(AveragePeopleOccupancyRate);
        public const string TotalVehicleCount = nameof(TotalVehicleCount);
        public const string AverageVehicleOccupancyRate = nameof(AverageVehicleOccupancyRate);
        public const string PeopleCount = nameof(PeopleCount);
        public const string PeopleOccupancy = nameof(PeopleOccupancy);
        public const string VehicleCount = nameof(VehicleCount);
        public const string VehicleOccupancy = nameof(VehicleOccupancy);
        public const string NoDataAvailableForInsights = nameof(NoDataAvailableForInsights);
        public const string NoValidDataAvailableForInsights = nameof(NoValidDataAvailableForInsights);
        public const string NoValidInsightsBasedOnSelection = nameof(NoValidInsightsBasedOnSelection);
        public const string HasHighestVehicleOccupancyRate = nameof(HasHighestVehicleOccupancyRate);
        public const string HasHighestVehicleCount = nameof(HasHighestVehicleCount);
        public const string VehicleOccupancyLevelsSuggest = nameof(VehicleOccupancyLevelsSuggest);
        public const string AreBusierThan = nameof(AreBusierThan);
        public const string HasHighestPeopleOccupancyRate = nameof(HasHighestPeopleOccupancyRate);
        public const string HasHighestPeopleCount = nameof(HasHighestPeopleCount);
        public const string PeopleOccupancyLevelsSuggest = nameof(PeopleOccupancyLevelsSuggest);
        public const string HasHighestPeopleAndVehicleOccupancyRates = nameof(HasHighestPeopleAndVehicleOccupancyRates);
        public const string AndVehicleOccupancyRate = nameof(AndVehicleOccupancyRate);
        public const string PeopleAndVehicleCount = nameof(PeopleAndVehicleCount);
        public const string ByZone = nameof(ByZone);
        public const string BySite = nameof(BySite);

        //MaintenancePlan
        public const string MaintenanceDeviceExits = nameof(MaintenanceDeviceExits);
        public const string MaintenancePlanExists = nameof(MaintenancePlanExists);

        //ANPR
        public const string DeviceNotRegistered = nameof(DeviceNotRegistered);
        public const string DeviceNotRegisteredForIP = nameof(DeviceNotRegisteredForIP);


        public const string ANPRVehicleNotFound = nameof(ANPRVehicleNotFound);
        public const string UnauthorizedVehicle = nameof(UnauthorizedVehicle);
        public const string VehicleOwnerNotFound = nameof(VehicleOwnerNotFound);
        public const string VehicleOwnerNotFoundNotification = nameof(VehicleOwnerNotFoundNotification);
        public const string DeviceNotAllowed = nameof(DeviceNotAllowed);
        public const string GateAccessDenied = nameof(GateAccessDenied);
        public const string VehicleNotAllowedTime = nameof(VehicleNotAllowedTime);
        public const string TimeRestriction = nameof(TimeRestriction);
        public const string VehicleLimitReached = nameof(VehicleLimitReached);
        public const string VehicleCounLimit = nameof(VehicleCounLimit);
        public const string VisitorTimeRestriction = nameof(VisitorTimeRestriction);
        public const string VisitorValidPeriod = nameof(VisitorValidPeriod);
        public const string SystemAutoExited = nameof(SystemAutoExited);
        public const string VehicleEnteredSuccessfully = nameof(VehicleEnteredSuccessfully);
        public const string FailToOpenGateBarrierDesc = nameof(FailToOpenGateBarrierDesc);
        public const string FailToOpenGateBarrierTitle = nameof(FailToOpenGateBarrierTitle);
        public const string PermanentTimeRestrictionDesc = nameof(PermanentTimeRestrictionDesc);
        public const string PermanentTimeRestrictionTitle = nameof(PermanentTimeRestrictionTitle);
        public const string VisitorTimeRestrictionDesc = nameof(VisitorTimeRestrictionDesc);
        public const string VisitorTimeRestrictionTitle = nameof(VisitorTimeRestrictionTitle);
        public const string VehicleExitedSuccssfully = nameof(VehicleExitedSuccssfully);
        public const string OwnerValidityExpired = nameof(OwnerValidityExpired);
        public const string OwnerValidityExpiredTitle = nameof(OwnerValidityExpiredTitle);

        public const string FailToOpenGateManually = nameof(FailToOpenGateManually);
        public const string GateOpenSuccessManually = nameof(GateOpenSuccessManually);
        public const string FailToCloseGate = nameof(FailToCloseGate);

        public const string EmailSentSuccessfully = nameof(EmailSentSuccessfully);

        public const string EventAlreadyExist = nameof(EventAlreadyExist);
        public const string UrlDomainValidation = nameof(UrlDomainValidation);
    }

    // Empty class for resource file association
    public class AppMessages { }
}
