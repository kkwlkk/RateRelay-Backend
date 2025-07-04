using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Helpers;
using Serilog;

namespace RateRelay.Infrastructure.Services;

public class BusinessVerificationService(
    IUnitOfWorkFactory unitOfWorkFactory,
    IGooglePlacesService googlePlacesService,
    IPointService pointService,
    IReferralService referralService,
    ILogger logger)
    : IBusinessVerificationService
{
    public async Task<BusinessVerificationResult> InitiateVerificationAsync(string placeId, long accountId)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();

        var existingBusiness = await GetUserBusinessAsync(unitOfWork, accountId);

        var validationResult =
            await ValidateBusinessVerificationRequest(unitOfWork, existingBusiness, placeId, accountId);
        if (!validationResult.IsValid)
        {
            return validationResult.Result;
        }

        var place = await googlePlacesService.GetPlaceDetailsAsync(placeId);
        if (place is null)
        {
            logger.Warning("Failed to retrieve place details for place ID {PlaceId}", placeId);
            return BusinessVerificationResult.InvalidPlaceId(placeId);
        }

        var business = await CreateOrUpdateBusinessAsync(unitOfWork, existingBusiness, place, accountId);
        await unitOfWork.SaveChangesAsync();

        var activeVerification = await GetActiveVerificationAsync(unitOfWork, business.Id);

        if (activeVerification != null)
        {
            if (activeVerification.IsVerificationExpired)
            {
                await ResetExpiredVerificationAsync(unitOfWork, activeVerification);
            }

            return BusinessVerificationResult.Success(business, false, activeVerification);
        }

        var newVerification = await CreateNewVerificationAsync(unitOfWork, business.Id);
        return BusinessVerificationResult.Success(business, false, newVerification);
    }

    public async Task<BusinessVerificationResult> CheckVerificationStatusAsync(long accountId)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();

        var business = await GetBusinessWithVerificationAsync(unitOfWork, accountId);

        if (business == null)
        {
            logger.Warning("Business for account ID {AccountId} not found", accountId);
            return BusinessVerificationResult.BusinessNotFound("Business not found.");
        }

        if (business.IsVerified)
        {
            logger.Information("Business with account ID {AccountId} is already verified", accountId);
            return BusinessVerificationResult.AlreadyVerified(business);
        }

        if (business.Verification == null)
        {
            logger.Warning("No active verification found for business {BusinessId}", business.Id);
            return BusinessVerificationResult.VerificationNotFound();
        }

        if (business.Verification.IsVerificationExpired)
        {
            logger.Information("Verification for business {BusinessId} is expired", business.Id);
            return BusinessVerificationResult.VerificationExpired();
        }

        var isBusinessHoursValid = await VerifyBusinessHoursAsync(business, business.Verification);

        if (isBusinessHoursValid)
        {
            await CompleteVerificationAsync(unitOfWork, business);
            logger.Information("Business with ID {BusinessId} has been verified successfully", business.Id);
            return BusinessVerificationResult.Success(business, true, business.Verification);
        }

        await IncrementVerificationAttemptsAsync(unitOfWork, business.Verification);
        return BusinessVerificationResult.Success(business, false, business.Verification);
    }

    public async Task<BusinessVerificationEntity?> GetActiveVerificationChallengeAsync(long accountId)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();

        var business = await GetBusinessWithVerificationAsync(unitOfWork, accountId);

        if (business == null)
        {
            logger.Warning("Business with account ID {AccountId} not found", accountId);
            return null;
        }

        if (business.IsVerified)
        {
            logger.Information("Business with account ID {AccountId} is already verified", accountId);
            var metadata = new Dictionary<string, object>
            {
                { "placeId", business.PlaceId }
            };
            throw new AppException("Business is already verified", "ERR_ALREADY_VERIFIED", metadata);
        }

        var verification = business.Verification;

        if (verification?.IsVerificationExpired != false)
        {
            if (verification?.IsVerificationExpired == true)
            {
                logger.Information("Verification for business {BusinessId} is expired", business.Id);
            }
            else
            {
                logger.Warning("No active verification found for business with account ID {AccountId}", accountId);
            }

            return null;
        }

        return verification;
    }

    private async Task<BusinessEntity?> GetUserBusinessAsync(IUnitOfWork unitOfWork, long accountId)
    {
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        var existingBusinesses = await businessRepository.FindAsync(b => b.OwnerAccountId == accountId);
        return existingBusinesses.FirstOrDefault();
    }

    private async Task<BusinessEntity?> GetBusinessWithVerificationAsync(IUnitOfWork unitOfWork, long accountId)
    {
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        return await businessRepository.GetBaseQueryable()
            .Where(b => b.OwnerAccountId == accountId)
            .Include(b => b.Verification)
            .FirstOrDefaultAsync();
    }

    private async Task<(bool IsValid, BusinessVerificationResult Result)> ValidateBusinessVerificationRequest(
        IUnitOfWork unitOfWork,
        BusinessEntity? existingBusiness,
        string placeId,
        long accountId)
    {
        if (existingBusiness != null)
        {
            if (existingBusiness.PlaceId != placeId)
            {
                logger.Warning("Account {AccountId} already has a business registered and cannot register another one",
                    accountId);
                return (false, BusinessVerificationResult.TooManyBusinesses());
            }

            if (existingBusiness.IsVerified)
            {
                logger.Warning("Business with place ID {PlaceId} is already verified by account {AccountId}", placeId,
                    accountId);
                return (false, BusinessVerificationResult.AlreadyExists(existingBusiness));
            }
        }

        var businessWithSamePlaceId = await GetBusinessByPlaceIdAsync(unitOfWork, placeId);

        if (businessWithSamePlaceId != null && businessWithSamePlaceId.OwnerAccountId != accountId)
        {
            if (businessWithSamePlaceId.IsVerified)
            {
                logger.Warning("Business with place ID {PlaceId} is already verified by another account", placeId);
                return (false, BusinessVerificationResult.AlreadyExists(businessWithSamePlaceId));
            }
            else
            {
                logger.Warning("Business with place ID {PlaceId} is already being verified by another account",
                    placeId);
                return (false, BusinessVerificationResult.AlreadyExists(businessWithSamePlaceId));
            }
        }

        if (businessWithSamePlaceId != null && businessWithSamePlaceId.OwnerAccountId == accountId &&
            businessWithSamePlaceId.IsVerified)
        {
            logger.Warning("Business with place ID {PlaceId} is already verified by the same account", placeId);
            return (false, BusinessVerificationResult.AlreadyVerified(businessWithSamePlaceId));
        }

        return (true, null);
    }

    private async Task<BusinessEntity?> GetBusinessByPlaceIdAsync(IUnitOfWork unitOfWork, string placeId)
    {
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        var businesses = await businessRepository.FindAsync(b => b.PlaceId == placeId);
        return businesses.FirstOrDefault();
    }

    private async Task<BusinessEntity> CreateOrUpdateBusinessAsync(
        IUnitOfWork unitOfWork,
        BusinessEntity? existingBusiness,
        dynamic place,
        long accountId)
    {
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();

        if (existingBusiness != null)
        {
            existingBusiness.BusinessName = place.DisplayName.Text;
            businessRepository.Update(existingBusiness);
            return existingBusiness;
        }

        var newBusiness = new BusinessEntity
        {
            PlaceId = place.PlaceId,
            Cid = place.Cid,
            BusinessName = place.DisplayName.Text,
            OwnerAccountId = accountId,
            IsVerified = false
        };

        await businessRepository.InsertAsync(newBusiness);
        return newBusiness;
    }

    private async Task<BusinessVerificationEntity?> GetActiveVerificationAsync(
        IUnitOfWork unitOfWork,
        long businessId)
    {
        var businessVerificationRepository = unitOfWork.GetRepository<BusinessVerificationEntity>();
        var activeVerifications = await businessVerificationRepository
            .FindAsync(v => v.BusinessId == businessId && v.VerificationCompletedUtc == null);
        return activeVerifications.FirstOrDefault();
    }

    private async Task ResetExpiredVerificationAsync(
        IUnitOfWork unitOfWork,
        BusinessVerificationEntity verification)
    {
        logger.Information("Existing verification for business {BusinessId} is expired. Resetting it.",
            verification.BusinessId);

        var businessVerificationRepository = unitOfWork.GetRepository<BusinessVerificationEntity>();
        verification.VerificationStartedUtc = DateTime.UtcNow;
        verification.VerificationAttempts = 0;
        businessVerificationRepository.Update(verification);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task<BusinessVerificationEntity> CreateNewVerificationAsync(
        IUnitOfWork unitOfWork,
        long businessId)
    {
        var businessVerificationRepository = unitOfWork.GetRepository<BusinessVerificationEntity>();
        var (openingTime, closingTime) = GenerateRandomBusinessHours();
        var verificationDay = RandomHelper.GetRandomEnumValue<DayOfWeek>();

        var verification = new BusinessVerificationEntity
        {
            BusinessId = businessId,
            VerificationStartedUtc = DateTime.UtcNow,
            VerificationDay = verificationDay,
            VerificationOpeningTime = openingTime,
            VerificationClosingTime = closingTime,
            VerificationAttempts = 0
        };

        await businessVerificationRepository.InsertAsync(verification);
        await unitOfWork.SaveChangesAsync();
        return verification;
    }

    private async Task CompleteVerificationAsync(
        IUnitOfWork unitOfWork,
        BusinessEntity business)
    {
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        var businessVerificationRepository = unitOfWork.GetRepository<BusinessVerificationEntity>();

        business.IsVerified = true;
        business.Verification.VerificationCompletedUtc = DateTime.UtcNow;

        businessRepository.Update(business);
        businessVerificationRepository.Update(business.Verification);
        await unitOfWork.SaveChangesAsync();

        await referralService.UpdateReferralProgressAsync(
            business.OwnerAccountId,
            ReferralGoalType.BusinessVerified
        );
    }

    private async Task IncrementVerificationAttemptsAsync(
        IUnitOfWork unitOfWork,
        BusinessVerificationEntity verification)
    {
        var businessVerificationRepository = unitOfWork.GetRepository<BusinessVerificationEntity>();
        verification.VerificationAttempts++;
        businessVerificationRepository.Update(verification);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task<bool> VerifyBusinessHoursAsync(BusinessEntity business, BusinessVerificationEntity verification)
    {
        var place = await googlePlacesService.GetPlaceDetailsAsync(business.PlaceId);

        if (place?.CurrentOpeningHours.BusinessHoursByDay is null)
        {
            logger.Warning("Failed to retrieve place details for place ID {PlaceId}", business.PlaceId);
            return false;
        }

        if (!place.CurrentOpeningHours.BusinessHoursByDay.TryGetValue(verification.VerificationDay,
                out var placeBusinessHours)
            || placeBusinessHours == null)
        {
            logger.Warning("No business hours found for day {DayOfWeek} for place ID {PlaceId}",
                verification.VerificationDay, business.PlaceId);
            return false;
        }

        return placeBusinessHours.OpenTime == verification.VerificationOpeningTime &&
               placeBusinessHours.CloseTime == verification.VerificationClosingTime;
    }

    private static (TimeSpan opening, TimeSpan closing) GenerateRandomBusinessHours()
    {
        var random = RandomHelper.Random;

        var openingHour = random.Next(6, 12);
        var openingMinute = random.Next(0, 4) * 15;
        var operatingHours = random.Next(6, 12);

        var openingTime = new TimeSpan(openingHour, openingMinute, 0);
        var closingTime = openingTime.Add(TimeSpan.FromHours(operatingHours));

        if (closingTime.Days > 0)
        {
            closingTime = new TimeSpan(23, 45, 0);
        }

        return (openingTime, closingTime);
    }
}