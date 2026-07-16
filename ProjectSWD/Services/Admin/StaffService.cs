using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectSWD.Services.Admin
{
    public class StaffService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StaffService(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<ProjectSWD.Data.Entities.Staff>> GetAllAsync()
        {
            return await _context.Staffs
                .Include(s => s.User)
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        public async Task<ProjectSWD.Data.Entities.Staff?> GetByIdAsync(string id)
        {
            return await _context.Staffs
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> CreateAsync(ProjectSWD.Data.Entities.Staff staff, string password)
        {
            // 1. Create IdentityUser
            var user = new IdentityUser
            {
                UserName = staff.Email,
                Email = staff.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description));
            }

            // 2. Add to role "Staff"
            var roleResult = await _userManager.AddToRoleAsync(user, "Staff");
            if (!roleResult.Succeeded)
            {
                // rollback
                await _userManager.DeleteAsync(user);
                return (false, roleResult.Errors.Select(e => e.Description));
            }

            // 3. Create Staff entity
            staff.Id = user.Id;
            staff.User = user;

            try
            {
                _context.Staffs.Add(staff);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // rollback
                await _userManager.DeleteAsync(user);
                return (false, new[] { $"Lỗi lưu cơ sở dữ liệu: {ex.Message}" });
            }

            return (true, Array.Empty<string>());
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateAsync(string id, string fullName, string email, string phone, string? newPassword)
        {
            var staff = await _context.Staffs.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (staff == null)
            {
                return (false, new[] { "Không tìm thấy thông tin nhân viên." });
            }

            var user = staff.User ?? await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return (false, new[] { "Không tìm thấy tài khoản người dùng tương ứng." });
            }

            // Check if email is being changed and if it is already in use
            if (user.Email != email)
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null && existingUser.Id != id)
                {
                    return (false, new[] { "Email đã được sử dụng bởi một tài khoản khác." });
                }

                user.Email = email;
                user.UserName = email;
                var updateEmailResult = await _userManager.UpdateAsync(user);
                if (!updateEmailResult.Succeeded)
                {
                    return (false, updateEmailResult.Errors.Select(e => e.Description));
                }
            }

            // Handle password change if specified
            if (!string.IsNullOrEmpty(newPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!resetResult.Succeeded)
                {
                    return (false, resetResult.Errors.Select(e => e.Description));
                }
            }

            // Update Staff properties
            staff.FullName = fullName;
            staff.Email = email;
            staff.Phone = phone;

            try
            {
                _context.Staffs.Update(staff);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return (false, new[] { $"Lỗi lưu cơ sở dữ liệu: {ex.Message}" });
            }

            return (true, Array.Empty<string>());
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            var user = await _userManager.FindByIdAsync(id);

            if (staff != null)
            {
                _context.Staffs.Remove(staff);
                await _context.SaveChangesAsync();
            }

            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                return result.Succeeded;
            }

            return staff != null;
        }
    }
}
