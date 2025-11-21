using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Repositories;

namespace TaskMonitoringApp.Models.DataAccessLayer
{
    public class CollectionRepository : ICollectionRepository
    {
        private readonly ApplicationDbContext _context;

        public CollectionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Collection>> GetCollectionsByUserIdAsync(string userId)
        {
            return await _context.Collections
                .Where(c => c.UserId == userId)
                .Include(c => c.Items) // To get the ItemCount
                .ToListAsync();
        }

        public async Task<Collection> GetCollectionByIdAsync(int collectionId, string userId)
        {
            return await _context.Collections
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CollectionId == collectionId && c.UserId == userId);
        }

        public async Task<bool> CollectionExistsAsync(int collectionId, string userId)
        {
            return await _context.Collections.AnyAsync(c => c.CollectionId == collectionId && c.UserId == userId);
        }

        public async Task<bool> CollectionNameExistsForUserAsync(string name, string userId)
        {
            return await _context.Collections.AnyAsync(c => c.UserId == userId && c.Name.ToLower() == name.ToLower());
        }

        public async Task AddCollectionAsync(Collection collection)
        {
            await _context.Collections.AddAsync(collection);
        }

        public void UpdateCollection(Collection collection)
        {
            // The context tracks the entity, so no special method is needed.
            // Changes will be saved when SaveChangesAsync is called.
            _context.Entry(collection).State = EntityState.Modified;
        }

        public void DeleteCollection(Collection collection)
        {
            _context.Collections.Remove(collection);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
