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

            // ========================
            // Cascade delete relationships
            // ========================

            // Resource -> AidPreparationResource
            modelBuilder.Entity<AidPreparationResource>()
                .HasOne<Resource>()
                .WithMany()
                .HasForeignKey(apr => apr.ResourceID)
                .OnDelete(DeleteBehavior.Cascade);

            // AidPreparation -> AidPreparationResource
            modelBuilder.Entity<AidPreparationResource>()
                .HasOne<AidPreparation>()
                .WithMany()
                .HasForeignKey(apr => apr.PreparationID)
                .OnDelete(DeleteBehavior.Cascade);

            // AidPreparation -> AidPreparationVolunteer
            modelBuilder.Entity<AidPreparationVolunteer>()
                .HasOne<AidPreparation>()
                .WithMany()
                .HasForeignKey(apv => apv.PreparationID)
                .OnDelete(DeleteBehavior.Cascade);

            // RescueTracking -> RescueTrackingVolunteer
            modelBuilder.Entity<RescueTrackingVolunteer>()
                .HasOne<RescueTracking>()
                .WithMany()
                .HasForeignKey(rtv => rtv.TrackingID)
                .OnDelete(DeleteBehavior.Cascade);

            // Skill -> VolunteerSkill
            modelBuilder.Entity<VolunteerSkill>()
                .HasOne<Skill>()
                .WithMany()
                .HasForeignKey(vs => vs.SkillID)
                .OnDelete(DeleteBehavior.Cascade);

            // Volunteer -> VolunteerSkill
            modelBuilder.Entity<VolunteerSkill>()
                .HasOne<Volunteer>()
                .WithMany()
                .HasForeignKey(vs => vs.VolunteerID)
                .OnDelete(DeleteBehavior.Cascade);

            // ReliefCenter -> Resource
            modelBuilder.Entity<Resource>()
                .HasOne<ReliefCenter>()
                .WithMany()
                .HasForeignKey(r => r.ReliefCenterID)
                .OnDelete(DeleteBehavior.Cascade);

            // ReliefCenter -> Volunteer
            modelBuilder.Entity<Volunteer>()
                .HasOne<ReliefCenter>()
                .WithMany()
                .HasForeignKey(v => v.AssignedCenter)
                .OnDelete(DeleteBehavior.Cascade);

            // AidRequest -> AidPreparation
            modelBuilder.Entity<AidPreparation>()
                .HasOne<AidRequest>()
                .WithMany()
                .HasForeignKey(ap => ap.RequestID)
                .OnDelete(DeleteBehavior.Cascade);

            // AidRequest -> RescueTracking
            modelBuilder.Entity<RescueTracking>()
                .HasOne<AidRequest>()
                .WithMany()
                .HasForeignKey(rt => rt.RequestID)
                .OnDelete(DeleteBehavior.Cascade);

            // AffectedArea -> AidRequest
            modelBuilder.Entity<AidRequest>()
                .HasOne<AffectedArea>()
                .WithMany()
                .HasForeignKey(ar => ar.AreaID)
                .OnDelete(DeleteBehavior.Cascade);

            // Donation -> (Optionally) link to ReliefCenter (can cascade if  want)
            modelBuilder.Entity<Donation>()
                .HasOne<ReliefCenter>()
                .WithMany()
                .HasForeignKey(d => d.AssociatedCenter)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> AidRequest
            modelBuilder.Entity<AidRequest>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ar => ar.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Donation
            modelBuilder.Entity<Donation>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(d => d.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Volunteer
            modelBuilder.Entity<Volunteer>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(v => v.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // ReliefCenter -> Resource
            modelBuilder.Entity<Resource>()
                .HasOne(r => r.ReliefCenter)
                .WithMany(rc => rc.Resources)
                .HasForeignKey(r => r.ReliefCenterID)
                .OnDelete(DeleteBehavior.Cascade);

            // ReliefCenter -> Volunteer
            modelBuilder.Entity<Volunteer>()
                .HasOne(v => v.ReliefCenter)
                .WithMany(rc => rc.Volunteers)
                .HasForeignKey(v => v.AssignedCenter)
                .OnDelete(DeleteBehavior.Cascade);

            // ReliefCenter -> Donation
            modelBuilder.Entity<Donation>()
                .HasOne(d => d.ReliefCenter)
                .WithMany(rc => rc.Donations)
                .HasForeignKey(d => d.AssociatedCenter)
                .OnDelete(DeleteBehavior.Cascade);
           
            modelBuilder.Entity<Volunteer>()
                 .HasOne<User>()
                 .WithMany()
                 .HasForeignKey(v => v.UserID);


        }
    }
}
