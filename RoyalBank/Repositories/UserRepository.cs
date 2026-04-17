using Microsoft.EntityFrameworkCore;
using RoyalBank.Data;
using RoyalBank.Interfaces;
using RoyalBank.Models;
namespace RoyalBank.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) { _db = db; }

        public async Task<User?> GetByIdAsync(int id) =>
            await _db.Users.Include(u => u.Customer).FirstOrDefaultAsync(u => u.Id == id);

        public async Task<User?> GetByUsernameAsync(string username) =>
            await _db.Users.Include(u => u.Customer).FirstOrDefaultAsync(u => u.Username == username);

        public async Task<List<User>> GetAllOfficersAsync() =>
            await _db.Users.Where(u => u.Role == UserRole.Compliance || u.Role == UserRole.KYC).ToListAsync();

        public async Task AddAsync(User u) 
        { 
            await _db.Users.AddAsync(u); 
            await _db.SaveChangesAsync(); 
        }

        public async Task UpdateAsync(User u) 
        {
            _db.Users.Update(u); 
            await _db.SaveChangesAsync(); 
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user != null) { _db.Users.Remove(user); await _db.SaveChangesAsync(); }
        }
    }
}
