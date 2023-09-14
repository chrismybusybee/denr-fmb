using Microsoft.EntityFrameworkCore;
namespace FMB_CIS.Data

{
    public class LocalContext : DbContext
    {
        public LocalContext(DbContextOptions<LocalContext> options) :
            base(options)
        { }

        public DbSet<FMB_CIS.Models.tbl_chainsaw> tbl_chainsaw { get; set; } = default!;
    }
}
