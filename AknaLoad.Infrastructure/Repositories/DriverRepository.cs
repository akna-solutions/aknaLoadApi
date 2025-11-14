using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using AknaLoad.Infrastructure.Persistence;

namespace AknaLoad.Infrastructure.Repositories
{
    public class DriverRepository : BaseRepository<Driver>, IDriverRepository
    {
        public DriverRepository(AknaLoadDbContext context) : base(context)
        {
        }

     
    }
}