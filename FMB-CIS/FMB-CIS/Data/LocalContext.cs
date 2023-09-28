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
    }
}
