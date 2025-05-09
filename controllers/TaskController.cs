using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement_BE.Services;
using TaskManagement_BE.models;
using System;
using System.Web;

namespace TaskManagement_BE.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks(
            [FromQuery] string? userId,
            [FromQuery] string? role,
            [FromQuery] string? filterField = null,
            [FromQuery] string? filterValues = null,
            [FromQuery] string? sort = null,
            [FromQuery] string? az = "asc",
            [FromQuery] string? token = null)
        {
            try
            {
                if (userId == null || role == null)
                    return Unauthorized(new
                    {
                        data = (object?)null,
                        status = "fail",
                        code = 401,
                        message = "Invalid user."
                    });

                string[] customFilterValues = filterValues != null
            ? HttpUtility.UrlDecode(filterValues).Split(',', StringSplitOptions.RemoveEmptyEntries)
            : new string[0];

                var tasks = await _taskService.GetTasksAsync(userId, role, filterField, customFilterValues, sort, az);

                return Ok(new
                {
                    data = tasks,
                    status = "success",
                    code = 200,
                    message = "Tasks retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new
                {
                    data = (object?)null,
                    status = "error",
                    code = 500,
                    message = "An error occurred while retrieving tasks."
                });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskDetail(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var task = await _taskService.GetTaskDetailAsync(id, userId, role);
            if (task == null)
                return NotFound("Task not found.");

            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskItem task)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var createdTask = await _taskService.CreateTaskAsync(task, userId);
            return Ok(createdTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskItem task)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var updatedTask = await _taskService.UpdateTaskAsync(id, task, userId, role);
            if (updatedTask == null)
                return NotFound("Task not found.");

            return Ok(updatedTask);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _taskService.DeleteTaskAsync(id, userId, role);
            if (!result)
                return NotFound("Task not found.");

            return Ok("Task deleted successfully.");
        }
    }
}
