using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;

namespace ProjectSWD.Services.Customer;

public class ProfileService
{
    private readonly ApplicationDbContext _context;

    public ProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Data.Entities.Customer?> GetCustomerByIdAsync(string userId)
    {
        return await _context.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == userId);
    }

    public async Task<Data.Entities.Staff?> GetStaffByIdAsync(string userId)
    {
        return await _context.Staffs
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == userId);
    }

    public async Task<bool> UpdateCustomerProfileAsync(string userId, string fullName, string phone, string address)
    {
        var customer = await _context.Customers.FindAsync(userId);
        if (customer == null) return false;

        customer.FullName = fullName;
        customer.Phone = phone;
        customer.Address = address;
        customer.Email = (await _context.Users.FindAsync(userId))?.Email ?? customer.Email;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStaffProfileAsync(string userId, string fullName, string phone)
    {
        var staff = await _context.Staffs.FindAsync(userId);
        if (staff == null) return false;

        staff.FullName = fullName;
        staff.Phone = phone;
        staff.Email = (await _context.Users.FindAsync(userId))?.Email ?? staff.Email;

        await _context.SaveChangesAsync();
        return true;
    }
}
