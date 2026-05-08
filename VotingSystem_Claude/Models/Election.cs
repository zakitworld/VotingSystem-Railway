using System.ComponentModel.DataAnnotations;

namespace VotingSystem_Claude.Models
{
    public class Election
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public string? SchoolYear { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Status Helpers (Time-based)
        public bool IsUpcoming => DateTime.UtcNow < StartDate;
        public bool IsLive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
        public bool IsPast => DateTime.UtcNow > EndDate;

        public string StatusLabel => DateTime.UtcNow switch
        {
            _ when IsUpcoming => "Upcoming",
            _ when IsLive => "Active",
            _ when IsPast => "Ended",
            _ => "Unknown"
        };

        public string StatusClass => DateTime.UtcNow switch
        {
            _ when IsUpcoming => "warning",
            _ when IsLive => "success",
            _ when IsPast => "secondary",
            _ => "info"
        };

        // Navigation properties
        public virtual ICollection<Position> Positions { get; set; } = [];
        public virtual ICollection<Vote> Votes { get; set; } = [];
    }
}
