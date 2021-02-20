using E_Radar.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Radar.Data
{
    public class E_RadarDbContext : DbContext
    {
        public DbSet<MessageModel> Messages { get; set; }
        public DbSet<SentMessageModel> SentMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
                => options.UseSqlite("Data Source=eradar.db");
        
    }
}
