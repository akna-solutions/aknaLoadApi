using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Interfaces.Repositories;
using AknaLoad.Infrastructure.Persistence;
using AknaLoad.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AknaLoad.Infrastructure.Repositories
{
    public class PricingCalculationRepository : BaseRepository<PricingCalculation>, IPricingCalculationRepository
    {
        public PricingCalculationRepository(AknaLoadDbContext context) : base(context)
        {
        }

     
    }
}