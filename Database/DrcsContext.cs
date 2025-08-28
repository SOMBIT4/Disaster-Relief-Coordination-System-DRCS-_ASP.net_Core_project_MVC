using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Database
{
    public class DrcsContext : DbContext
    {
        public DrcsContext(DbContextOptions<DrcsContext> options)
            : base(options)
        {
        }

        // Users
        public DbSet<User> Users { get; set; }

        // Relief Centers & Volunteers
        public DbSet<ReliefCenter> ReliefCenters { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }

        // Resources
        public DbSet<Resource> Resources { get; set; }

        // Affected Areas
        public DbSet<AffectedArea> AffectedAreas { get; set; }

        // Donations
        public DbSet<Donation> Donations { get; set; }

        // Aid Requests & Preparation
        public DbSet<AidRequest> AidRequests { get; set; }
        public DbSet<AidPreparation> AidPreparations { get; set; }
        public DbSet<AidPreparationResource> AidPreparationResources { get; set; }
        public DbSet<AidPreparationVolunteer> AidPreparationVolunteers { get; set; }

        // Rescue Tracking
        public DbSet<RescueTracking> RescueTrackings { get; set; }
        public DbSet<RescueTrackingVolunteer> RescueTrackingVolunteers { get; set; }

        // Skills
        public DbSet<Skill> Skills { get; set; }
        public DbSet<VolunteerSkill> VolunteerSkills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite keys
            modelBuilder.Entity<AidPreparationResource>()
                .HasKey(apr => new { apr.ID });

            modelBuilder.Entity<AidPreparationVolunteer>()
                .HasKey(apv => new { apv.ID });

            modelBuilder.Entity<RescueTrackingVolunteer>()
                .HasKey(rtv => new { rtv.ID });

            modelBuilder.Entity<VolunteerSkill>()
                .HasKey(vs => new { vs.VolunteerID, vs.SkillID });

            // Single key entities
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserID);

            modelBuilder.Entity<ReliefCenter>()
                .HasKey(rc => rc.CenterID);

            modelBuilder.Entity<Volunteer>()
                .HasKey(v => v.VolunteerID);

            modelBuilder.Entity<Resource>()
                .HasKey(r => r.ResourceID);

            modelBuilder.Entity<AffectedArea>()
                .HasKey(aa => aa.AreaID);

            modelBuilder.Entity<Donation>()
                .HasKey(d => d.DonationID);

            modelBuilder.Entity<AidRequest>()
                .HasKey(ar => ar.RequestID);

            modelBuilder.Entity<AidPreparation>()
                .HasKey(ap => ap.PreparationID);

            modelBuilder.Entity<RescueTracking>()
                .HasKey(rt => rt.TrackingID);

            modelBuilder.Entity<Skill>()
                .HasKey(s => s.SkillID);
        }
    }
}
