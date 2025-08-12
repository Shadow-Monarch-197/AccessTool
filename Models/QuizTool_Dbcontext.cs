using Microsoft.EntityFrameworkCore;

namespace quizTool.Models
{
    public class QuizTool_Dbcontext : DbContext
    {
        public QuizTool_Dbcontext(DbContextOptions<QuizTool_Dbcontext> options) : base(options) { }

        public DbSet<UserDataModel> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDataModel>()
                .Property(u => u.role)
                .HasDefaultValue("basic");
        }

        public void SeedUsers()
        {
            if (!Users.Any())
            {
                Users.AddRange(
                    new UserDataModel
                    {
                        name = "Admin User",
                        email = "admin@example.com",
                        password = "Admin@123",  
                        role = "admin",
                        createddate = DateTime.UtcNow,
                        mobileno = "1234567890"
                    },
                    new UserDataModel
                    {
                        name = "Basic User",
                        email = "user@example.com",
                        password = "User@123",
                        role = "user",
                        createddate = DateTime.UtcNow,
                        mobileno = "0987654321"
                    }
                );

                SaveChanges();
            }
        }
    }
}


