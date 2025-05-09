using TaskManagement_BE.Repositories;
using TaskManagement_BE.models;

namespace TaskManagement_BE.Services
{
    public interface ITaskService
    {
        Task<List<TaskItem>> GetTasksAsync(string userId, string role, string? filterField, string[]? filterValues, string? sort, string? az);
        Task<TaskItem?> GetTaskDetailAsync(int id, string userId, string role);
        Task<TaskItem> CreateTaskAsync(TaskItem task, string userId);
        Task<TaskItem?> UpdateTaskAsync(int id, TaskItem task);
        Task<bool> DeleteTaskAsync(int id, string userId, string role);
    }

    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<List<TaskItem>> GetTasksAsync(string userId, string role, string? filterField, string[]? filterValues, string? sort, string? az)
        {
            return await _taskRepository.GetTasksAsync(userId, role, filterField, filterValues, sort, az);
        }


        public async Task<TaskItem?> GetTaskDetailAsync(int id, string userId, string role)
        {
            return await _taskRepository.GetTaskByIdAsync(id);
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task, string userId)
        {
            task.UserId = userId;
            return await _taskRepository.CreateTaskAsync(task);
        }

        public async Task<TaskItem?> UpdateTaskAsync(int id, TaskItem task)
        {
            var existingTask = await _taskRepository.GetTaskByIdAsync(id);
            if (existingTask == null) return null;

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.Status = task.Status;
            existingTask.DueDate = task.DueDate;
            existingTask.UpdateAt = DateTime.UtcNow;

            return await _taskRepository.UpdateTaskAsync(existingTask);
        }

        public async Task<bool> DeleteTaskAsync(int id, string userId, string role)
        {
            return await _taskRepository.DeleteTaskAsync(id, userId, role);
        }
    }
}
