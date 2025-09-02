using backend.Database;
using Microsoft.EntityFrameworkCore;

namespace DRCS.Services
{
    public class VolunteerTaskService
    {
        private readonly DrcsContext _context;

        public VolunteerTaskService(DrcsContext context)
        {
            _context = context;
        }

        // Aid Preparation Tasks
        public async Task<IEnumerable<object>> GetAidPreparationTasksAsync(int volunteerId)
        {
            var tasks = await (
                from apv in _context.AidPreparationVolunteers
                join ap in _context.AidPreparations on apv.PreparationID equals ap.PreparationID
                join ar in _context.AidRequests on ap.RequestID equals ar.RequestID
                where apv.VolunteerID == volunteerId
                select new
                {
                    task_id = ap.PreparationID,
                    departure_time = ap.DepartureTime,
                    estimated_arrival = ap.EstimatedArrival,
                    task_status = ap.Status,
                    task_created_at = ap.CreatedAt,
                    // Request details
                    ar.RequestType,
                    ar.Description,
                    ar.UrgencyLevel
                }
            ).ToListAsync();

            return tasks;
        }

        // Rescue Tracking Tasks
        public async Task<IEnumerable<object>> GetRescueTrackingTasksAsync(int volunteerId)
        {
            var tasks = await (
                from rtv in _context.RescueTrackingVolunteers
                join rt in _context.RescueTrackings on rtv.TrackingID equals rt.TrackingID
                join ar in _context.AidRequests on rt.RequestID equals ar.RequestID
                join aa in _context.AffectedAreas on ar.AreaID equals aa.AreaID
                where rtv.VolunteerID == volunteerId
                select new
                {
                    task_id = rt.TrackingID,
                    req_id = rt.RequestID,
                    status = rt.TrackingStatus,
                    operation_start_time = rt.OperationStartTime,
                    completion_time = rt.CompletionTime,
                    number_of_people_helped = rt.NumberOfPeopleHelped,
                    // Aid Request details
                    ar.RequestID,
                    ar.RequesterName,
                    ar.ContactInfo,
                    ar.RequestType,
                    ar.Description,
                    ar.UrgencyLevel,
                    request_status = ar.Status,
                    ar.NumberOfPeople,
                    ar.RequestDate,
                    ar.ResponseTime,
                    area_name = aa.AreaName
                }
            ).ToListAsync();

            return tasks;
        }
    }
}
