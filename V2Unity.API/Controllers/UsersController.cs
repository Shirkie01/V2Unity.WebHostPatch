using Microsoft.AspNetCore.Mvc;
using V2Unity.Model;
using V2Unity.API.Persistence;

namespace V2Unity.API.Controllers
{
    [Route("v2unity/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IRepository<User> _userRepository;

        public UsersController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _userRepository.FindAll();
            // We could just return a list of strings here,
            // but doing it this way allows us to debug better
#if DEBUG
            return Ok(users);
#else
            return Ok(users.Select(user => new {user.Name}));
#endif
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] User request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3)
            {
                return BadRequest("Username must be at least 3 characters long.");
            }

            // Check that the username does not already exist
            // Can't use string.Equals since LiteDB does not support it.
            var existingUser = _userRepository.FindOne(user => user.Name.ToLower() == request.Name.ToLower());

            if (existingUser != null)
            {
                // If it does, tell the client
                return Conflict("This username is already taken.");
            }

            // Create the new user
            var userId = _userRepository.Add(new User()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                DeviceId = request.DeviceId
            });

            return Created();
        }

        [HttpPut("{deviceId}")]
        public IActionResult UpdateUser(string deviceId, string name)
        {
            var user = _userRepository.FindOne(user => user.DeviceId == deviceId);
            if (user == null)
                return NotFound("User not found!");

            var success = _userRepository.Update(new User()
            {
                Id = user.Id,
                Name = name,
                DeviceId = user.DeviceId
            });

            // The user could have been deleted between when we
            // checked and when we are updating.
            return success ? Ok() : NotFound("Unable to update user.");
        }
        [HttpDelete("{deviceId}")]
        public IActionResult DeleteUser(string deviceId)
        {
            var user = _userRepository.FindOne(user => user.DeviceId == deviceId);
            if (user == null)
                return NotFound("User not found!");

#if DEBUG
            var success = _userRepository.Delete(user.Id);
            return success ? Ok() : Problem("Unable to delete user.");
#else
            return NotFound();
#endif
        }


        [HttpGet("{deviceId}/login")]
        public IActionResult LoginUser(string deviceId)
        {
            var user = _userRepository.FindOne(user => user.DeviceId == deviceId);
            if (user == null)
                return NotFound("User not found!");

            return Ok(user.Name);
        }

        public class UserRequest
        {
            public string DeviceId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }
    }
}
