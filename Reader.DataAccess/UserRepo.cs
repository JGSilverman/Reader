using Microsoft.EntityFrameworkCore;
using Reader.Domain;
using System.Linq.Expressions;

namespace Reader.DataAccess
{
    public class UserRepo : IRepository<AppUser>
    {
        private readonly IDbContextFactory<ReaderDbContext> _contextFactory;

        public UserRepo(IDbContextFactory<ReaderDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<bool> DeleteManyAsync(List<AppUser> entities)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            ctx.RemoveRange(entities);
            return await ctx.SaveChangesAsync() >= 1;
        }

        public async Task<bool> DeleteOneAsync(AppUser entity)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            ctx.Remove(entity);
            return await ctx.SaveChangesAsync() >= 1;
        }

        public async Task<List<AppUser>> GetManyAsync(Expression<Func<AppUser, bool>> filter = null)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            IQueryable<AppUser> query = ctx.Users.AsNoTracking();

            if (filter is not null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();
        }

        public async Task<AppUser> GetOneAsync(Expression<Func<AppUser, bool>> filter)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            IQueryable<AppUser> query = ctx.Users.AsNoTracking();

            if (filter is not null)
            {
                query = query.Where(filter);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> InsertManyAsync(List<AppUser> entities)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            await ctx.Users.AddRangeAsync(entities);
            return await ctx.SaveChangesAsync() >= 1;
        }

        public async Task<AppUser> InsertOneAsync(AppUser entity)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            await ctx.Users.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity;
        }

        public Task<bool> UpdateManyAsync(List<AppUser> entities)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateOneAsync(AppUser entity)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            ctx.Users.Attach(entity);
            ctx.Entry(entity).State = EntityState.Modified;
            return await ctx.SaveChangesAsync() >= 1;
        }
    }
}
