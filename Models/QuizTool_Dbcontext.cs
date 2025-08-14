using Microsoft.EntityFrameworkCore;

namespace quizTool.Models
{
    public class QuizTool_Dbcontext : DbContext
    {
        public QuizTool_Dbcontext(DbContextOptions<QuizTool_Dbcontext> options) : base(options) { }

        public DbSet<UserDataModel> Users { get; set; }
        //public DbSet<Question> Questions => Set<Question>();
        //public DbSet<Choice> Choices => Set<Choice>();
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<TestAttempt> TestAttempts { get; set; }
        public DbSet<TestAttemptAnswer> TestAttemptAnswers { get; set; }


        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<UserDataModel>()
        //        .Property(u => u.role)
        //        .HasDefaultValue("basic");

        //    //modelBuilder.Entity<Question>(e =>
        //    //{
        //    //    e.HasKey(x => x.Id);
        //    //    e.Property(x => x.Text).IsRequired();
        //    //    e.HasMany(x => x.Choices)
        //    //    .WithOne(x => x.Question)
        //    //    .HasForeignKey(x => x.QuestionId)
        //    //    .OnDelete(DeleteBehavior.Cascade);
        //    //});

        //    //modelBuilder.Entity<Choice>(e =>
        //    //{
        //    //    e.HasKey(x => x.Id);
        //    //    e.Property(x => x.Text).IsRequired();
        //    //});
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserDataModel>()
            .Property(u => u.role)
            .HasDefaultValue("basic");

            modelBuilder.Entity<Question>()
            .HasOne(q => q.Test)
            .WithMany(t => t.Questions)
            .HasForeignKey(q => q.TestId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Option>()
            .HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TestAttemptAnswer>()
            .HasOne(a => a.Attempt)
            .WithMany(at => at.Answers)
            .HasForeignKey(a => a.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
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


