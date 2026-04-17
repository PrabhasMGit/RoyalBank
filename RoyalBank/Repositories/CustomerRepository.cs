using Microsoft.EntityFrameworkCore;
using RoyalBank.Data;
using RoyalBank.Interfaces;
using RoyalBank.Models;

namespace RoyalBank.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _db;
        public CustomerRepository(AppDbContext db) { _db = db; }

        public async Task<Customer?> GetByIdAsync(int id) =>
            await _db.Customers
                .Include(c => c.KycDocuments)
                .Include(c => c.Accounts)
                .Include(c => c.RiskProfile)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

        public async Task<Customer?> GetByEmailAsync(string email) =>
            await _db.Customers.FirstOrDefaultAsync(c => c.Email == email);

        public async Task<List<Customer>> GetAllAsync() =>
            await _db.Customers
                .Include(c => c.KycDocuments)
                .Include(c => c.Accounts)
                .Include(c => c.RiskProfile)
                .ToListAsync();

        public async Task AddAsync(Customer c)
        {
            await _db.Customers.AddAsync(c);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer c)
        {
            _db.Customers.Update(c);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int customerId)
        {
            // Remove  user first (FK to Customer)
            var linkedUser = await _db.Users.FirstOrDefaultAsync(u => u.CustomerId == customerId);
            if (linkedUser != null)
            {
                _db.Users.Remove(linkedUser);
                await _db.SaveChangesAsync();
            }

            // Then delete the customer
            var customer = await _db.Customers.FindAsync(customerId);
            if (customer != null)
            {
                _db.Customers.Remove(customer);
                await _db.SaveChangesAsync();
            }
        }
    }
}
