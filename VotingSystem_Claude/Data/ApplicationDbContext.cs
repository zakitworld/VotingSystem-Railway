using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Data
{
    public class ApplicationDbContext : DbContext
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Election>()
                .HasMany(e => e.Positions)
                .WithOne(p => p.Election)
                .HasForeignKey(p => p.ElectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Position>()
                .HasMany(p => p.Candidates)
                .WithOne(c => c.Position)
                .HasForeignKey(c => c.PositionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Voter)
                .WithOne(v => v.Student)
                .HasForeignKey<Voter>(v => v.StudentId);

            modelBuilder.Entity<Student>()
                .HasMany(s => s.Candidates)
                .WithOne(c => c.Student)
                .HasForeignKey(c => c.StudentId);

            modelBuilder.Entity<Voter>()
                .HasMany(v => v.Votes)
                .WithOne(vote => vote.Voter)
                .HasForeignKey(vote => vote.VoterId);

            modelBuilder.Entity<Candidate>()
                .HasMany(c => c.Votes)
                .WithOne(v => v.Candidate)
                .HasForeignKey(v => v.CandidateId);

            // Ensure voter codes are unique
            modelBuilder.Entity<Voter>()
                .HasIndex(v => v.VoterCode)
                .IsUnique();

            // Ensure student IDs are unique
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.StudentId)
                .IsUnique();
        }
    }
}
