using backend.Database;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRCS.Services
{
    public class UserService
    {
        private readonly DrcsContext _context;

        public UserService(DrcsContext context)
        {
            _context = context;
        }

        // ------------------ GET SINGLE USER ------------------
        private async Task<User> GetUserById(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found.");
            return user;
        }

        // ------------------ GET ALL USERS ------------------
        public async Task<IEnumerable<object>> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.UserID,
                    u.Email,
                    u.Name,
                    u.RoleName,
                    u.PhoneNo,
                    DonationsCount = _context.Donations.Count(d => d.UserID == u.UserID),
                    AidRequestsCount = _context.AidRequests.Count(ar => ar.UserID == u.UserID),
                    ManagedCenters = u.RoleName == "Manager"
                        ? _context.ReliefCenters.Where(rc => rc.ManagerID == u.UserID).ToList()
                        : null,
                    AssignedCenter = u.RoleName == "Volunteer"
                        ? _context.Volunteers.FirstOrDefault(v => v.UserID == u.UserID)
                        : null
                })
                .ToListAsync();

            return users;
        }

        // ------------------ GET USER WITH VOLUNTEER INFO ------------------
        public async Task<object> GetUserWithVolunteerInfo(int userId)
        {
            var user = await GetUserById(userId);

            if (user.RoleName == "Volunteer")
            {
                var volunteer = await _context.Volunteers
                    .FirstOrDefaultAsync(v => v.UserID == userId);

                if (volunteer == null)
                    return new { user, volunteer = (object?)null, skills = new List<object>() };

                // Fetch volunteer skills without navigation properties
                var skills = await (from vs in _context.VolunteerSkills
                                    join s in _context.Skills
                                    on vs.SkillID equals s.SkillID
                                    where vs.VolunteerID == volunteer.VolunteerID
                                    select new
                                    {
                                        s.SkillID,
                                        s.SkillName
                                    })
                                   .ToListAsync();

                return new { user, volunteer, skills };
            }

            return new { user, volunteer = (object?)null, skills = new List<object>() };
        }


        // ------------------ GET ALL VOLUNTEERS ------------------
        public async Task<IEnumerable<Volunteer>> GetAllVolunteers()
        {
            return await _context.Volunteers.ToListAsync();
        }

        // ------------------ CREATE USER ------------------
        public async Task<User> CreateUser(User newUser, int? assignedCenterId = null)
        {
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            if (newUser.RoleName == "Volunteer" && assignedCenterId.HasValue)
            {
                var volunteer = new Volunteer
                {
                    Name = newUser.Name,
                    ContactInfo = newUser.PhoneNo,
                    UserID = newUser.UserID,
                    AssignedCenter = assignedCenterId.Value,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Volunteers.Add(volunteer);
                await _context.SaveChangesAsync();

                await UpdateReliefCenterVolunteerCount(assignedCenterId.Value, 1);
            }

            return newUser;
        }

        // ------------------ UPDATE USER ------------------
        public async Task<User> UpdateUser(int id, User updatedUser, int? assignedCenterId = null)
        {
            var user = await GetUserById(id);
            string oldRole = user.RoleName;

            // Update user fields
            user.Email = updatedUser.Email;
            user.Name = updatedUser.Name;
            user.RoleName = updatedUser.RoleName;
            user.PhoneNo = updatedUser.PhoneNo;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Fetch old volunteer record if any
            Volunteer? oldVolunteer = null;
            if (oldRole == "Volunteer")
                oldVolunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.UserID == id);

            string newRole = user.RoleName;

            // 1) Non-volunteer -> Volunteer
            if (oldRole != "Volunteer" && newRole == "Volunteer" && assignedCenterId.HasValue)
            {
                var newVolunteer = new Volunteer
                {
                    Name = user.Name,
                    ContactInfo = user.PhoneNo,
                    UserID = user.UserID,
                    AssignedCenter = assignedCenterId.Value,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Volunteers.Add(newVolunteer);
                await _context.SaveChangesAsync();

                await UpdateReliefCenterVolunteerCount(assignedCenterId.Value, 1);
            }
            // 2) Volunteer -> Volunteer (center change)
            else if (oldRole == "Volunteer" && newRole == "Volunteer" && assignedCenterId.HasValue && oldVolunteer != null)
            {
                int? oldAssignedCenter = oldVolunteer.AssignedCenter;
                int newAssignedCenter = assignedCenterId.Value;

                if (!oldAssignedCenter.HasValue || oldAssignedCenter.Value != newAssignedCenter)
                {
                    oldVolunteer.AssignedCenter = newAssignedCenter;
                    oldVolunteer.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    if (oldAssignedCenter.HasValue)
                        await UpdateReliefCenterVolunteerCount(oldAssignedCenter.Value, -1);

                    await UpdateReliefCenterVolunteerCount(newAssignedCenter, 1);
                }
            }
            // 3) Volunteer -> Non-volunteer
            else if (oldRole == "Volunteer" && newRole != "Volunteer" && oldVolunteer != null)
            {
                int? oldAssignedCenter = oldVolunteer.AssignedCenter;

                _context.Volunteers.Remove(oldVolunteer);
                await _context.SaveChangesAsync();

                if (oldAssignedCenter.HasValue)
                    await UpdateReliefCenterVolunteerCount(oldAssignedCenter.Value, -1);
            }

            return user;
        }

        // ------------------ DELETE USER ------------------
        public async Task DeleteUser(int id)
        {
            var user = await GetUserById(id);

            if (user.RoleName == "Volunteer")
            {
                var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.UserID == id);
                if (volunteer != null && volunteer.AssignedCenter.HasValue)
                {
                    int centerId = volunteer.AssignedCenter.Value;
                    _context.Volunteers.Remove(volunteer);
                    await _context.SaveChangesAsync();

                    await UpdateReliefCenterVolunteerCount(centerId, -1);
                }
            }

            // Set donations' UserID to null
            var donations = await _context.Donations.Where(d => d.UserID == id).ToListAsync();
            if (donations.Any())
            {
                _context.Donations.RemoveRange(donations);
                await _context.SaveChangesAsync();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        // ------------------ HELPER ------------------
        private async Task UpdateReliefCenterVolunteerCount(int centerId, int change)
        {
            var center = await _context.ReliefCenters.FindAsync(centerId);
            if (center != null)
            {
                center.NumberOfVolunteersWorking += change;
                if (center.NumberOfVolunteersWorking < 0)
                    center.NumberOfVolunteersWorking = 0;

                center.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
