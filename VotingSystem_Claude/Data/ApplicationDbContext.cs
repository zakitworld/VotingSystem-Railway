using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Middleware;

namespace VotingSystem_Claude.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Election> Elections { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Voter> Voters { get; set; }
        public DbSet<VoterCode> VoterCodes { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships and constraints
            builder.Entity<Election>()
                .HasMany(e => e.Positions)
                .WithOne(p => p.Election)
                .HasForeignKey(p => p.ElectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Position>()
                .HasMany(p => p.Candidates)
                .WithOne(c => c.Position)
                .HasForeignKey(c => c.PositionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Vote>()
                .HasOne(v => v.Election)
                .WithMany(e => e.Votes)
                .HasForeignKey(v => v.ElectionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Vote>()
                .HasOne(v => v.Position)
                .WithMany(p => p.Votes)
                .HasForeignKey(v => v.PositionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Vote>()
                .HasOne(v => v.Candidate)
                .WithMany(c => c.Votes)
                .HasForeignKey(v => v.CandidateId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Vote>()
                .HasOne(v => v.Voter)
                .WithMany(v => v.Votes)
                .HasForeignKey(v => v.VoterId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Voter>()
                .HasOne(v => v.Student)
                .WithOne(s => s.Voter)
                .HasForeignKey<Voter>(v => v.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<VoterCode>()
                .HasOne(vc => vc.Voter)
                .WithOne(v => v.VoterCode)
                .HasForeignKey<VoterCode>(vc => vc.VoterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure AuditLog
            builder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Action).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.Details).HasColumnType("nvarchar(max)");
            });
        }
    }
}
