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
            [FromQuery] string? searchTitle = null,
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

                var tasks = await _taskService.GetTasksAsync(userId, role, filterField, customFilterValues, sort, az, searchTitle);

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

            try
            {
                var userId = task.UserId;
                // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var createdTask = await _taskService.CreateTaskAsync(task, userId);

                return Ok(new
                {
                    data = createdTask,
                    status = true,
                    code = 200,
                    message = "Task created successfully."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new
                {
                    data = (object?)null,
                    status = false,
                    code = 500,
                    message = "An error occurred while creating task."
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskItem task)
        {
            try
            {
                var updatedTask = await _taskService.UpdateTaskAsync(id, task);
                if (updatedTask == null)
                    return NotFound("Task not found.");
                return Ok(new
                {
                    data = updatedTask,
                    status = true,
                    code = 200,
                    message = "Task updated successfully."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new
                {
                    data = (object?)null,
                    status = false,
                    code = 500,
                    message = "An error occurred while updating task."
                });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id, [FromQuery] string userId, [FromQuery] string role)
        {
            try
            {
                // var role = User.FindFirst(ClaimTypes.Role)?.Value;
                // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                    return BadRequest(new { code = 400, status = false, message = "User ID or Role is missing." });

                var result = await _taskService.DeleteTaskAsync(id, userId, role);

                if (!result)
                    return NotFound(new { code = 404, status = false, message = "Task not found or not authorized to delete." });

                return Ok(new { code = 200, status = true, message = "Task deleted successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { code = 500, status = false, message = "An error occurred while deleting the task." });
            }
        }
    }
}
