using Application.Interfaces;
using Domain.Entities.BookingManagement;
using Domain.Entities.StationManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace WebAPI;

public static class CreateStaffs
{
    public static async Task CreateAdminStaffs(this WebApplication app)
    {

        // Create 2 accounts for staffs
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // 1) Ensure STAFF role exists
            const string staffRole = "STAFF";
            if (!await roleManager.RoleExistsAsync(staffRole))
            {
                var _ = await roleManager.CreateAsync(new IdentityRole(staffRole));
            }

            // Helper to create or update a user, and return the user + parsed Guid Id
            async Task<(IdentityUser user, Guid userGuid)> UpsertUserAsync(string email, string defaultPassword)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };
                    var createResult = await userManager.CreateAsync(user, defaultPassword);
                    if (!createResult.Succeeded)
                    {
                        var errs = string.Join("; ", createResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                        throw new InvalidOperationException($"Failed creating user {email}: {errs}");
                    }
                }
                else
                {
                    // Make sure basics are aligned if user exists
                    bool changed = false;
                    if (!user.EmailConfirmed) { user.EmailConfirmed = true; changed = true; }
                    if (user.UserName != email) { user.UserName = email; changed = true; }
                    if (changed)
                    {
                        var updateResult = await userManager.UpdateAsync(user);
                        if (!updateResult.Succeeded)
                        {
                            var errs = string.Join("; ", updateResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                            throw new InvalidOperationException($"Failed updating user {email}: {errs}");
                        }
                    }
                }

                // Ensure user in STAFF role
                if (!await userManager.IsInRoleAsync(user, staffRole))
                {
                    var addRoleResult = await userManager.AddToRoleAsync(user, staffRole);
                    if (!addRoleResult.Succeeded)
                    {
                        var errs = string.Join("; ", addRoleResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                        throw new InvalidOperationException($"Failed adding user {email} to role {staffRole}: {errs}");
                    }
                }

                // Your Identity uses GUID-like string keys; parse to Guid for Staff.UserId
                if (!Guid.TryParse(user.Id, out var userGuid))
                    throw new InvalidOperationException($"User.Id for {email} is not a valid GUID: {user.Id}");

                return (user, userGuid);
            }

            // Helper to upsert Staff by UserId (fallback by EmployeeCode)
            async Task<Staff> UpsertStaffAsync(Guid userId, string employeeCode, DateTime hireDate, string position)
            {
                var staffRepo = unitOfWork.Repository<Staff>();

                // Try by UserId first
                var existing = await staffRepo
                    .AsQueryable()
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                // Fallback by EmployeeCode if needed (in case schema allows duplicates)
                if (existing == null)
                {
                    existing = await staffRepo
                        .AsQueryable()
                        .FirstOrDefaultAsync(s => s.EmployeeCode == employeeCode);
                }

                if (existing == null)
                {
                    existing = new Staff
                    {
                        StaffId = Guid.NewGuid(),
                        UserId = userId,
                        EmployeeCode = employeeCode,
                        HireDate = hireDate,
                        Position = position
                    };
                    await staffRepo.AddAsync(existing);
                }
                else
                {
                    // Update mutable fields
                    existing.EmployeeCode = employeeCode;
                    existing.HireDate = hireDate;
                    existing.Position = position;
                    staffRepo.Update(existing);
                }

                return existing;
            }

            // Helper to upsert StaffAtStation: keep at most one active assignment per (Staff, Station)
            async Task<StaffAtStation> UpsertStaffAtStationAsync(Guid staffId, Guid stationId, string roleAtStation, DateTime startTime)
            {
                var sasRepo = unitOfWork.Repository<StaffAtStation>();

                // Find the current active assignment for this Staff at this Station
                var active = await sasRepo
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.StaffId == staffId && x.StationId == stationId && x.EndTime == null);

                if (active == null)
                {
                    // Optionally, if there is any other active record for this Staff (at a different station),
                    // you may want to end it here to enforce "one active station per staff" rule:
                    // var otherActive = await sasRepo.Queryable()
                    //     .Where(x => x.StaffId == staffId && x.EndTime == null)
                    //     .ToListAsync();
                    // foreach (var oa in otherActive) { oa.EndTime = startTime; sasRepo.Update(oa); }

                    active = new StaffAtStation
                    {
                        StaffId = staffId,
                        StationId = stationId,
                        RoleAtStation = roleAtStation,
                        StartTime = startTime,
                        EndTime = null
                    };
                    await sasRepo.AddAsync(active);
                }
                else
                {
                    // Update current assignment details
                    active.RoleAtStation = roleAtStation;

                    // If caller wants to reset the start time (only if you intend to rebase)
                    // You can choose to keep the earliest start; here we update to the provided start
                    active.StartTime = startTime;

                    sasRepo.Update(active);
                }

                return active;
            }

            // === Actual seeding / upserting ===
            var (u1, u1Guid) = await UpsertUserAsync("staff1@ev.local", "Staff@1234");
            var (u2, u2Guid) = await UpsertUserAsync("staff2@ev.local", "Staff@1234");

            var staff1 = await UpsertStaffAsync(
                userId: u1Guid,
                employeeCode: "STF001",
                hireDate: DateTime.UtcNow.AddYears(-2),
                position: "Staff"
            );

            var staff2 = await UpsertStaffAsync(
                userId: u2Guid,
                employeeCode: "STF002",
                hireDate: DateTime.UtcNow.AddYears(-1),
                position: "Staff"
            );

            // Station seeds you referenced
            var station1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var station2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

            await UpsertStaffAtStationAsync(staff1.StaffId, station1, roleAtStation: "Staff", startTime: DateTime.UtcNow.AddYears(-1));
            await UpsertStaffAtStationAsync(staff2.StaffId, station2, roleAtStation: "Staff", startTime: DateTime.UtcNow.AddMonths(-6));

            await unitOfWork.SaveChangesAsync();
        }
    }
}
