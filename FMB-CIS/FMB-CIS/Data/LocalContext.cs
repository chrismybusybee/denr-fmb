using Microsoft.EntityFrameworkCore;
namespace FMB_CIS.Data
{
    public class LocalContext : DbContext
    {
        public LocalContext(DbContextOptions<LocalContext> options) :
            base(options)
        { }

        public DbSet<FMB_CIS.Models.tbl_chainsaw> tbl_chainsaw { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_application> tbl_application { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_application_type> tbl_application_type { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_permit_status> tbl_permit_status { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_permit_type> tbl_permit_type { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_user> tbl_user { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_user_types> tbl_user_types { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_files> tbl_files { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_region> tbl_region { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_province> tbl_province { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_city> tbl_city { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_brgy> tbl_brgy { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_user_temp_passwords> tbl_user_temp_passwords { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_comments> tbl_comments { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_announcement> tbl_announcement { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_division> tbl_division { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_user_change_info_request> tbl_user_change_info_request { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_user_change_info_request_status> tbl_user_change_info_request_status { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_email_template> tbl_email_template { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_announcement_type> tbl_announcement_type { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_office> tbl_office { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_office_type> tbl_office_type { get; set; } = default!;
        //public DbSet<FMB_CIS.Models.tbl_office> tbl_office { get; set; } = default!; // same as division
        public DbSet<FMB_CIS.Models.tbl_access_right> tbl_access_right { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_user_type_access_right> tbl_user_type_access_right { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_site_settings> tbl_site_settings { get; set; } = default!;


    }
}
