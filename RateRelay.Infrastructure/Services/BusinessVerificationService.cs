using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Common;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Domain.Interfaces.Services;
using RateRelay.Infrastructure.Helpers;
using Serilog;

namespace RateRelay.Infrastructure.Services;

public class BusinessVerificationService(
    IUnitOfWorkFactory unitOfWorkFactory,
    IGooglePlacesService googlePlacesService,
    ILogger logger)
    : IBusinessVerificationService
{
    public async Task<BusinessVerificationResult> InitiateVerificationAsync(string placeId, long accountId)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        var businessVerificationRepository = unitOfWork.GetRepository<BusinessVerificationEntity>();

        var existingBusinesses = await businessRepository.FindAsync(b => b.OwnerAccountId == accountId);
        var businessEntities = existingBusinesses.ToList();
        if (businessEntities.Count != 0)
        {
            var userBusiness = businessEntities.First();

            if (userBusiness.PlaceId != placeId)
            {
                logger.Warning("Account {AccountId} already has a business registered and cannot register another one",
                    accountId);
                return BusinessVerificationResult.TooManyBusinesses();
            }

            if (userBusiness.IsVerified)
            {
                logger.Warning("Business with place ID {PlaceId} is already verified by account {AccountId}",
                    placeId, userBusiness.OwnerAccountId);
                return BusinessVerificationResult.AlreadyVerified(userBusiness);
            }
        }

        // Check if another user is trying to verify this business
        var businessWithSamePlaceId = (await businessRepository.FindAsync(b => b.PlaceId == placeId)).FirstOrDefault();
        if (businessWithSamePlaceId is not null && businessWithSamePlaceId.OwnerAccountId != accountId)
        {
            logger.Warning("Business with place ID {PlaceId} is already being verified by another account",
                placeId);
            return BusinessVerificationResult.AlreadyBeingVerified(businessWithSamePlaceId);
        }

        var place = await googlePlacesService.GetPlaceDetailsAsync(placeId);
        if (place is null)
        {
            logger.Warning("Failed to retrieve place details for place ID {PlaceId}", placeId);
            return BusinessVerificationResult.InvalidPlaceId();
        }

        var business = businessWithSamePlaceId;
        if (business is null)
        {
            business = new BusinessEntity
            {
                PlaceId = placeId,
                Cid = place.Cid,
                BusinessName = place.Name,
                OwnerAccountId = accountId,
                IsVerified = false
            };

            await businessRepository.InsertAsync(business);
        }
        else
        {
            business.BusinessName = place.Name;
            businessRepository.Update(business);
        }

        await unitOfWork.SaveChangesAsync();

        var activeVerifications = await businessVerificationRepository
            .FindAsync(v => v.BusinessId == business.Id && v.VerificationCompletedUtc == null);

        var activeVerification = activeVerifications.FirstOrDefault();
        if (activeVerification is not null)
        {
            if (activeVerification.IsVerificationExpired)
            {
                logger.Information("Existing verification for business {BusinessId} is expired. Creating a new one.",
                    business.Id);

                businessVerificationRepository.Update(activeVerification);
                activeVerification.VerificationStartedUtc = DateTime.UtcNow;
                activeVerification.VerificationAttempts = 0;
                await unitOfWork.SaveChangesAsync();
            }

            return BusinessVerificationResult.Success(business, false, activeVerification);
        }

        var (openingTime, closingTime) = await GenerateRandomBusinessHoursAsync();

        // (0 = Sunday, 6 = Saturday)
        var verificationDay = RandomHelper.GetRandomEnumValue<DayOfWeek>();

        var verification = new BusinessVerificationEntity
        {
            BusinessId = business.Id,
            VerificationStartedUtc = DateTime.UtcNow,
            VerificationDay = verificationDay,
            VerificationOpeningTime = openingTime,
            VerificationClosingTime = closingTime,
            VerificationAttempts = 0
        };

        await businessVerificationRepository.InsertAsync(verification);
        await unitOfWork.SaveChangesAsync();

        return BusinessVerificationResult.Success(business, false, verification);
    }

    public async Task<BusinessVerificationResult> CheckVerificationStatusAsync(long accountId)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        var businessVerificationRepository = unitOfWork.GetRepository<BusinessVerificationEntity>();

        var business = await businessRepository.GetBaseQueryable()
            .Where(b => b.OwnerAccountId == accountId && b.IsVerified == false)
            .Include(b => b.Verification)
            .FirstOrDefaultAsync();

        if (business is null)
        {
            logger.Warning("Business for account ID {AccountId} not found or already verified", accountId);
            return BusinessVerificationResult.BusinessNotFound("Business not found or already verified.");
        }

        if (business.OwnerAccountId != accountId)
        {
            logger.Warning("Business with ID {BusinessId} is not owned by account {AccountId}",
                business.Id, accountId);
            return BusinessVerificationResult.NotOwnedByAccount();
        }

        if (business.IsVerified)
        {
            logger.Information("Business with ID {BusinessId} is already verified", business.Id);
            return BusinessVerificationResult.AlreadyVerified(business);
        }

        if (business.Verification is null)
        {
            logger.Warning("No active verification found for business {BusinessId}", business.Id);
            return BusinessVerificationResult.VerificationNotFound();
        }

        if (business.Verification.IsVerificationExpired)
        {
            logger.Information("Verification for business {BusinessId} is expired", business.Id);
            return BusinessVerificationResult.VerificationExpired();
        }

        // TODO: max verification attempts

        var isBusinessHoursValid = await VerifyBusinessHoursAsync(business, business.Verification);

        if (isBusinessHoursValid)
        {
            businessRepository.Update(business);
            businessVerificationRepository.Update(business.Verification);
            business.IsVerified = true;
            business.Verification.VerificationCompletedUtc = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();

            logger.Information("Business with ID {BusinessId} has been verified successfully", business.Id);
            return BusinessVerificationResult.Success(business, true, business.Verification);
        }

        businessVerificationRepository.Update(business.Verification);
        business.Verification.VerificationAttempts++;
        await unitOfWork.SaveChangesAsync();

        return BusinessVerificationResult.Success(business, false, business.Verification);
    }

    private async Task<bool> VerifyBusinessHoursAsync(BusinessEntity business, BusinessVerificationEntity verification)
    {
        var place = await googlePlacesService.GetPlaceDetailsAsync(business.PlaceId);

        if (place?.CurrentOpeningHours.BusinessHoursByDay is null)
        {
            logger.Warning("Failed to retrieve place details for place ID {PlaceId}", business.PlaceId);
            return false;
        }

        var placeBusinessHours = place.CurrentOpeningHours.BusinessHoursByDay
            .FirstOrDefault(bh => bh.Key == verification.VerificationDay).Value;

        if (placeBusinessHours is null)
        {
            logger.Warning("No business hours found for day {DayOfWeek} for place ID {PlaceId}",
                verification.VerificationDay, business.PlaceId);
            return false;
        }

        return placeBusinessHours.OpenTime == verification.VerificationOpeningTime &&
               placeBusinessHours.CloseTime == verification.VerificationClosingTime;
    }

    public async Task<BusinessVerificationEntity?> GetActiveVerificationChallengeAsync(long accountId)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        var businessVerificationRepository = unitOfWork.GetRepository<BusinessVerificationEntity>();

        var business = await businessRepository.GetBaseQueryable()
            .Where(b => b.OwnerAccountId == accountId && b.IsVerified == false)
            .Include(b => b.Verification)
            .FirstOrDefaultAsync();

        if (business is null)
        {
            logger.Warning("Business with account ID {AccountId} not found or already verified", accountId);
            return null;
        }

        var verification = business.Verification;

        if (verification is null)
        {
            logger.Warning("No active verification found for business with account ID {AccountId}", accountId);
            return null;
        }

        if (verification.IsVerificationExpired)
        {
            logger.Information("Verification for business {BusinessId} is expired", business.Id);
            return null;
        }

        return verification;
    }

    private static Task<(TimeSpan opening, TimeSpan closing)> GenerateRandomBusinessHoursAsync()
    {
        var random = RandomHelper.Random;
        var openingHour = random.Next(0, 24);
        var openingMinute = random.Next(0, 60);
        var closingHour = random.Next(openingHour + 1, 25);
        var closingMinute = random.Next(0, 60);

        var openingTime = new TimeSpan(openingHour, openingMinute, 0);
        var closingTime = new TimeSpan(closingHour, closingMinute, 0);

        return Task.FromResult((openingTime, closingTime));
    }
}