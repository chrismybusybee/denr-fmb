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
        public DbSet<FMB_CIS.Models.tbl_application_group> tbl_application_group { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_application_type> tbl_application_type { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_permit_status> tbl_permit_status { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_permit_statuses> tbl_permit_statuses { get; set; } = default!;
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
        public DbSet<FMB_CIS.Models.tbl_user_type_user> tbl_user_type_user { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_permit_types> tbl_permit_types { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_permit_pages> tbl_permit_pages { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_permit_workflow> tbl_permit_workflow { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_permit_workflow_step> tbl_permit_workflow_step { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_permit_workflow_next_step> tbl_permit_workflow_next_step { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_document_checklist> tbl_document_checklist { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_application_read> tbl_application_read { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_files_checklist_bridge> tbl_files_checklist_bridge { get; set; } = default!;
        public DbSet<FMB_CIS.Models.tbl_application_payment> tbl_application_payment { get; set; } = default!;
    }
}
