using AknaLoad.Domain.Dtos;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;
using AknaLoad.Domain.Interfaces.Services;
using AknaLoad.Domain.Interfaces.UnitOfWorks;


namespace AknaLoad.Application.Services
{
    public class MatchingService : IMatchingService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly ILoadRepository _loadRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MatchingService(
            IMatchRepository matchRepository,
            ILoadRepository loadRepository,
            IDriverRepository driverRepository,
            IUnitOfWork unitOfWork)
        {
            _matchRepository = matchRepository;
            _loadRepository = loadRepository;
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Match>> FindMatchesForLoadAsync(long loadId, int maxMatches = 10)
        {
            var load = await _loadRepository.GetByIdAsync(loadId);
            if (load == null || load.Status != LoadStatus.Published)
                return new List<Match>();

            // Get available drivers in the area
            var availableDrivers = await _driverRepository.GetAvailableDriversAsync(
                load.PickupLocation,
                500, // 500km max distance
                load.PickupDateTime.AddHours(-2), // 2 hours before pickup
                load.DeliveryDeadline.AddHours(2) // 2 hours after deadline
            );

            var matches = new List<Match>();

            foreach (var driver in availableDrivers.Take(maxMatches))
            {
                // Check if driver already has active match
                var activeMatch = await _matchRepository.GetActiveMatchByDriverAsync(driver.Id);
                if (activeMatch != null)
                    continue;

                // Calculate match score
                var matchScore = await CalculateMatchScoreAsync(load, driver, null); // Vehicle will be fetched separately

                if (matchScore >= 50) // Minimum match threshold
                {
                    var match = new Match
                    {
                        LoadId = loadId,
                        DriverId = driver.Id,
                        VehicleId = driver.CurrentVehicleId ?? 0, // This should be properly resolved
                        MatchCode = await GenerateMatchCodeAsync(),
                        MatchScore = matchScore,
                        Status = MatchStatus.Proposed,
                        ProposedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddHours(24), // 24 hours to respond
                        MatchingFactorsJson = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            DistanceScore = CalculateDistanceScore(load, driver),
                            RatingScore = CalculateRatingScore(driver),
                            ExperienceScore = CalculateExperienceScore(driver),
                            AvailabilityScore = CalculateAvailabilityScore(driver, load)
                        })
                    };

                    matches.Add(match);
                }
            }

            // Sort by match score (highest first)
            return matches.OrderByDescending(m => m.MatchScore).ToList();
        }

        public async Task<List<Match>> FindMatchesForDriverAsync(long driverId, int maxMatches = 10)
        {
            var driver = await _driverRepository.GetByIdAsync(driverId);
            if (driver == null || driver.Status != DriverAvailabilityStatus.Available)
                return new List<Match>();

            // Get available loads in driver's area
            var availableLoads = await _loadRepository.GetAvailableLoadsAsync(
                driver.CurrentLocation,
                driver.MaxDistanceKm
            );

            var matches = new List<Match>();

            foreach (var load in availableLoads.Take(maxMatches))
            {
                // Check if load already has active match
                var activeMatch = await _matchRepository.GetActiveMatchByLoadAsync(load.Id);
                if (activeMatch != null)
                    continue;

                // Calculate match score
                var matchScore = await CalculateMatchScoreAsync(load, driver, null);

                if (matchScore >= 50) // Minimum match threshold
                {
                    var match = new Match
                    {
                        LoadId = load.Id,
                        DriverId = driverId,
                        VehicleId = driver.CurrentVehicleId ?? 0,
                        MatchCode = await GenerateMatchCodeAsync(),
                        MatchScore = matchScore,
                        Status = MatchStatus.Proposed,
                        ProposedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddHours(24),
                        MatchingFactorsJson = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            DistanceScore = CalculateDistanceScore(load, driver),
                            RatingScore = CalculateRatingScore(driver),
                            ExperienceScore = CalculateExperienceScore(driver),
                            AvailabilityScore = CalculateAvailabilityScore(driver, load)
                        })
                    };

                    matches.Add(match);
                }
            }

            return matches.OrderByDescending(m => m.MatchScore).ToList();
        }

        public async Task<Match> CreateMatchAsync(long loadId, long driverId, long vehicleId, string createdBy)
        {
            var load = await _loadRepository.GetByIdAsync(loadId);
            var driver = await _driverRepository.GetByIdAsync(driverId);

            if (load == null || driver == null)
                throw new ArgumentException("Load or Driver not found");

            var matchScore = await CalculateMatchScoreAsync(load, driver, null);

            var match = new Match
            {
                LoadId = loadId,
                DriverId = driverId,
                VehicleId = vehicleId,
                MatchCode = await GenerateMatchCodeAsync(),
                MatchScore = matchScore,
                Status = MatchStatus.Proposed,
                ProposedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedUser = createdBy,
                UpdatedUser = createdBy
            };

            await _matchRepository.AddAsync(match);
            await _unitOfWork.SaveChangesAsync();

            return match;
        }

        public async Task<bool> AcceptMatchAsync(long matchId, string acceptedBy)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null || match.Status != MatchStatus.Proposed)
                return false;

            await _matchRepository.UpdateMatchStatusAsync(matchId, MatchStatus.DriverAccepted, acceptedBy);
            await _unitOfWork.SaveChangesAsync();

            // Update load status
            await _loadRepository.UpdateLoadStatusAsync(match.LoadId, LoadStatus.DriverAccepted, acceptedBy);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectMatchAsync(long matchId, string reason, string rejectedBy)
        {
            await _matchRepository.UpdateMatchStatusAsync(matchId, MatchStatus.DriverRejected, rejectedBy, reason);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<Match?> GetMatchByIdAsync(long matchId)
        {
            return await _matchRepository.GetByIdAsync(matchId);
        }

        public async Task<Match?> GetMatchByCodeAsync(string matchCode)
        {
            return await _matchRepository.GetMatchByCodeAsync(matchCode);
        }

        public async Task<List<Match>> GetMatchesForLoadAsync(long loadId, MatchStatus? status = null)
        {
            return await _matchRepository.GetMatchesForLoadAsync(loadId, status);
        }

        public async Task<List<Match>> GetMatchesForDriverAsync(long driverId, MatchStatus? status = null)
        {
            return await _matchRepository.GetMatchesForDriverAsync(driverId, status);
        }

        public async Task<bool> ExpireMatchAsync(long matchId, string expiredBy)
        {
            await _matchRepository.UpdateMatchStatusAsync(matchId, MatchStatus.Expired, expiredBy);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<Match>> GetExpiredMatchesAsync()
        {
            return await _matchRepository.GetExpiredMatchesAsync();
        }

        public async Task<decimal> CalculateMatchScoreAsync(Load load, Driver driver, VehicleDto? vehicle)
        {
            decimal totalScore = 0;
            int factorCount = 0;

            // Distance Score (30% weight)
            var distanceScore = CalculateDistanceScore(load, driver);
            totalScore += distanceScore * 0.3m;
            factorCount++;

            // Rating Score (25% weight)
            var ratingScore = CalculateRatingScore(driver);
            totalScore += ratingScore * 0.25m;
            factorCount++;

            // Experience Score (20% weight)
            var experienceScore = CalculateExperienceScore(driver);
            totalScore += experienceScore * 0.2m;
            factorCount++;

            // Availability Score (15% weight)
            var availabilityScore = CalculateAvailabilityScore(driver, load);
            totalScore += availabilityScore * 0.15m;
            factorCount++;

            // Special Requirements Score (10% weight)
            var specialReqScore = CalculateSpecialRequirementsScore(load, driver);
            totalScore += specialReqScore * 0.1m;
            factorCount++;

            return Math.Round(totalScore, 2);
        }

        private decimal CalculateDistanceScore(Load load, Driver driver)
        {
            if (load.PickupLocation == null || driver.CurrentLocation == null)
                return 0;

            var distance = load.PickupLocation.DistanceTo(driver.CurrentLocation);

            // Score decreases with distance
            if (distance <= 10) return 100;
            if (distance <= 50) return 90;
            if (distance <= 100) return 80;
            if (distance <= 200) return 70;
            if (distance <= 300) return 60;
            if (distance <= 400) return 50;
            if (distance <= 500) return 40;
            return 20;
        }

        private decimal CalculateRatingScore(Driver driver)
        {
            if (driver.TotalRatings == 0)
                return 60; // Default score for new drivers

            // Convert 5-star rating to 100-point scale
            return Math.Min(driver.AverageRating * 20, 100);
        }

        private decimal CalculateExperienceScore(Driver driver)
        {
            if (driver.ExperienceYears >= 10) return 100;
            if (driver.ExperienceYears >= 5) return 90;
            if (driver.ExperienceYears >= 3) return 80;
            if (driver.ExperienceYears >= 1) return 70;
            return 50; // New drivers
        }

        private decimal CalculateAvailabilityScore(Driver driver, Load load)
        {
            var score = 100m;

            // Check if driver is available during load timeframe
            if (driver.AvailableFrom.HasValue && driver.AvailableFrom > load.PickupDateTime)
                score -= 30;

            if (driver.AvailableUntil.HasValue && driver.AvailableUntil < load.DeliveryDeadline)
                score -= 30;

            // Check working hours if available
            if (driver.WorkingHours != null)
            {
                if (!driver.WorkingHours.IsAvailableAt(load.PickupDateTime))
                    score -= 20;
            }

            return Math.Max(score, 0);
        }

        private decimal CalculateSpecialRequirementsScore(Load load, Driver driver)
        {
            if (load.SpecialRequirements == null || !load.SpecialRequirements.Any())
                return 100;

            var score = 100m;
            var requirementsPenalty = 0m;

            foreach (var requirement in load.SpecialRequirements)
            {
                switch (requirement)
                {
                    case SpecialRequirement.Hazardous:
                        if (!driver.HasADRLicense)
                            requirementsPenalty += 50;
                        break;
                    case SpecialRequirement.ColdChain:
                    case SpecialRequirement.TemperatureControlled:
                        // Would need to check vehicle capabilities
                        break;
                        // Add more requirement checks
                }
            }

            return Math.Max(score - requirementsPenalty, 0);
        }

        public async Task<string> GenerateMatchCodeAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"MT{timestamp}{random}";
        }

        public async Task<bool> NotifyDriverAsync(long matchId)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                return false;

            await _matchRepository.UpdateMatchStatusAsync(matchId, MatchStatus.DriverNotified, "System");
            match.NotifiedAt = DateTime.UtcNow;
            _matchRepository.Update(match);
            await _unitOfWork.SaveChangesAsync();

            // Here you would implement notification logic (SMS, Push, Email)
            return true;
        }

        public async Task<bool> ConfirmMatchAsync(long matchId, string confirmedBy)
        {
            await _matchRepository.UpdateMatchStatusAsync(matchId, MatchStatus.Confirmed, confirmedBy);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelMatchAsync(long matchId, string reason, string cancelledBy)
        {
            await _matchRepository.UpdateMatchStatusAsync(matchId, MatchStatus.Cancelled, cancelledBy, reason);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<Match>> GetPendingMatchesAsync(long? driverId = null)
        {
            return await _matchRepository.GetPendingMatchesAsync();
        }

        public async Task ProcessExpiredMatchesAsync()
        {
            var expiredMatches = await _matchRepository.GetExpiredMatchesAsync();

            foreach (var match in expiredMatches)
            {
                await ExpireMatchAsync(match.Id, "System");
            }
        }

        public async Task<Match?> GetActiveMatchByLoadAsync(long loadId)
        {
            return await _matchRepository.GetActiveMatchByLoadAsync(loadId);
        }

        public async Task<Match?> GetActiveMatchByDriverAsync(long driverId)
        {
            return await _matchRepository.GetActiveMatchByDriverAsync(driverId);
        }
    }
}