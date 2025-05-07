using Microsoft.AspNetCore.Identity;

namespace TaskManagement_BE.models

{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // To-Do, In Progress, Completed
        public DateTime DueDate { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}
