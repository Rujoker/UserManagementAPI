using Microsoft.AspNetCore.Mvc;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    // Для примера используем статический список (в реальном проекте — DI + сервис)
    private static readonly List<User> users = new()
    {
        new User { Id = 1, Name = "Alice", Email = "alice@example.com" },
        new User { Id = 2, Name = "Bob", Email = "bob@example.com" }
    };

    [HttpGet]
    public IActionResult GetUsers()
    {
        try
        {
            return Ok(users.ToList());
        }
        catch
        {
            return Problem("Failed to retrieve users.");
        }
    }

    [HttpGet("{id:int}")]
    public IActionResult GetUserById(int id)
    {
        try
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            return user is not null ? Ok(user) : NotFound();
        }
        catch
        {
            return Problem("Failed to retrieve user.");
        }
    }

    [HttpPost]
    public IActionResult CreateUser(User user)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Name and Email are required.");

            if (!user.Email.Contains('@'))
                return BadRequest("Invalid email format.");

            user.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
            users.Add(user);
            return Created($"/users/{user.Id}", user);
        }
        catch
        {
            return Problem("Failed to create user.");
        }
    }

    [HttpPut("{id:int}")]
    public IActionResult UpdateUser(int id, User updatedUser)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(updatedUser.Name) || string.IsNullOrWhiteSpace(updatedUser.Email))
                return BadRequest("Name and Email are required.");

            if (!updatedUser.Email.Contains('@'))
                return BadRequest("Invalid email format.");

            var user = users.FirstOrDefault(u => u.Id == id);
            if (user is null) return NotFound();

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            return Ok(user);
        }
        catch
        {
            return Problem("Failed to update user.");
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        try
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user is null) return NotFound();

            users.Remove(user);
            return NoContent();
        }
        catch
        {
            return Problem("Failed to delete user.");
        }
    }
}