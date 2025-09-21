using DRCS.Services;

namespace DRCS.Extensions
{
    public static class ServiceRegistration
    {
        // Extension method to register all your application services
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<AuthService>();
            services.AddScoped<AffectedAreaService>();
            services.AddScoped<AidPreparationService>();
            services.AddScoped<AidRequestService>();
            services.AddScoped<DonationService>();
            services.AddScoped<ReliefCenterService>();
            services.AddScoped<RescueTrackingService>();
            services.AddScoped<RescueTrackingVolunteerService>();
            services.AddScoped<ResourceService>();
            services.AddScoped<UserService>();
            services.AddScoped<VolunteerTaskService>();
            services.AddScoped<SkillService>();
            services.AddScoped<VolunteerSkillService>();
        }
    }
}
