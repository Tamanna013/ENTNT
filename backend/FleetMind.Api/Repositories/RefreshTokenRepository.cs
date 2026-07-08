using FleetMind.Api.Data;
using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(FleetMindDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _dbSet.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && !rt.IsDeleted);
    }
}
