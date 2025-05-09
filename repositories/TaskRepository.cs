using Microsoft.EntityFrameworkCore;
using TaskManagement_BE.data;
using TaskManagement_BE.models;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq.Dynamic.Core;


namespace TaskManagement_BE.Repositories
{
    public interface ITaskRepository
    {
        Task<List<TaskItem>> GetTasksAsync(string userId, string role, string? filterField, string[]? filterValues, string? sort, string? az);

        Task<TaskItem?> GetTaskByIdAsync(int id);
        Task<TaskItem> CreateTaskAsync(TaskItem task);
        Task<TaskItem?> UpdateTaskAsync(TaskItem task);
        Task<bool> DeleteTaskAsync(int id, string userId, string role);
    }

    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskItem>> GetTasksAsync(string userId, string role, string? filterField, string[]? filterValues, string? sort, string? az)
        {
            var query = _context.Tasks.AsQueryable();

            if (role != "admin")
            {
                query = query.Where(t => t.UserId == userId);
            }

            if (!string.IsNullOrEmpty(filterField) && filterValues?.Any() == true)
            {
                if (filterField.Equals("status", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(t => filterValues.Contains(t.Status));
                }
                else
                {
                    query = query.Where(t => filterValues.Contains(EF.Property<string>(t, filterField)));
                }
            }

            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(az))
            {
                if (sort == "due_date" || sort == "dueDate")
                {
                    string sortDirection = az?.ToLower() == "desc" ? "descending" : "ascending";
                    query = query.OrderBy($"DueDate {sortDirection}");
                }

            }

            return await query.ToListAsync();
        }


        public async Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            task.CreateAt = DateTime.UtcNow;
            task.UpdateAt = DateTime.UtcNow;
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem?> UpdateTaskAsync(TaskItem task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteTaskAsync(int id, string userId, string role)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && (t.UserId == userId || role == "admin"));
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
