using Microsoft.AspNetCore.Mvc;
using V2Unity.Model;
using V2Unity.API.Persistence;

namespace V2Unity.API.Controllers
{
    [Route("v2unity/[controller]")]
    [ApiController]
    public class RecordsController : ControllerBase
    {
        // *****************************************
        // DO NOT EVER RETURN DEVICE ID TO THE USER
        // *****************************************

        private readonly IRepository<Record> _recordsRepo;
        private readonly IRepository<User> _userRepo;

        public RecordsController(
            IRepository<User> userRepo,
            IRepository<Record> recordsRepo)
        {
            _userRepo = userRepo;
            _recordsRepo = recordsRepo;
        }

        [HttpGet]
        public IActionResult GetRecordsForStage([FromQuery] string? stage = null, [FromQuery] Difficulty? difficulty = null)
        {
            var records = _recordsRepo.FindAll();
            if(stage != null)
            {
                records = records.Where(x => x.Stage == stage);
            }

            if(difficulty != null)
            {
                records = records.Where(x => x.Difficulty == difficulty);
            }
            
            foreach (var record in records)
            {
                // Find the user that made this record
                var user = _userRepo.FindOne(user => user.DeviceId == record.DeviceId);
                if (user != null && !string.Equals(record.Name, user.Name))
                {
                    // If the user exists, overwrite the record name
                    // with the current user name if necessary
                    record.Name = user.Name;
                }

                // Clear the device id
                record.DeviceId = string.Empty;
            }
            return Ok(records);
        }

        [HttpGet("{deviceId}")]
        public IActionResult GetPersonalRecords(string deviceId, [FromQuery] string stage)
        {
            var personalRecords = _recordsRepo.FindAll().Where(r => r.Stage == stage && r.DeviceId == deviceId).ToList();
            foreach (var personalRecord in personalRecords)
            {
                // Find the user that made this record
                var user = _userRepo.FindOne(user => user.DeviceId == personalRecord.DeviceId);
                if (user != null && !string.Equals(personalRecord.Name, user.Name))
                {
                    // If the user exists, overwrite the record name
                    // with the current user name if necessary
                    personalRecord.Name = user.Name;
                }

                // Clear the device id
                personalRecord.DeviceId = string.Empty;
            }
            return Ok(personalRecords);
        }

        [HttpPost("{deviceId}")]
        public IActionResult NewRecord(string deviceId, Record record)
        {
            var user = _userRepo.FindOne(user => user.DeviceId == deviceId);
            if (user == null)
            {
                return NotFound("User not found!");
            }

            record.Name = user.Name;
            record.DeviceId = user.DeviceId;

            _recordsRepo.Add(record);
            return Ok();
        }
    }
}
