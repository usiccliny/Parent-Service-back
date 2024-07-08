using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Contexts;
using Server.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DevicesController : Controller
    {
        private DatabaseContext _context;
        public string? PCToken;

        public DevicesController(DatabaseContext context)
        {
            _context = context;

            var builder = WebApplication.CreateBuilder();
            PCToken = builder.Configuration["PCToken"];
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> GetDevice(string? Token, string id)
        {
            var user = await _context.Users
                    .Where(u => u.Token == Token)
                    .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            var link = await _context.Links
                .Where(l => l.UserId == user.TGID)
                .Where(l => l.DeviceID == id)
                .FirstOrDefaultAsync();


            if (link == null)
            {
                return NotFound();
            }

            var device = await _context.Devices
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();

            var returnedDevice = new Device()
            {
                DaysOfWeek = device.DaysOfWeek,
                HourLimit = device.HourLimit,
                LastOnline = device.LastOnline,
                MinuteLimit = device.MinuteLimit,
                Id = device.Id,
                Name = device.Name
            };

            return Ok(returnedDevice);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices(string? Token)
        {
            var user = await _context.Users
                    .Where(u => u.Token == Token)
                    .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            List<Link> links = [];
            links = await _context.Links
                .Where(l => l.UserId == user.TGID)
                .ToListAsync();

            List<Device> devices = [];
            if (!links.Any())
            {
                return Ok(devices);
            }

            foreach (var link in links)
            {
                var device = await _context.Devices
                .Where(d => d.Id == link.DeviceID)
                .FirstOrDefaultAsync();

                var returnedDevice = new Device()
                {
                    DaysOfWeek = device.DaysOfWeek,
                    HourLimit = device.HourLimit,
                    LastOnline = device.LastOnline,
                    MinuteLimit = device.MinuteLimit,
                    Id = device.Id,
                    Name = device.Name
                };

                devices.Add(returnedDevice);
            }


            return Ok(devices);
        }

        [HttpPost]
        public async Task<IActionResult> PostDevice([FromBody] DevicePC devicePC, [FromHeader(Name = "Token")] string Token)
        {
            if (Token != PCToken)
                return BadRequest();

            Device NewDevice = new Device
            {
                Name = devicePC.Name,
                LastOnline = devicePC.LastOnline,
                Id = devicePC.Id
            };

            _context.Devices.Add(NewDevice);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("shutdown/")]
        public async Task<IActionResult> ShutDown(string Token, string id)
        {
            if (id == null)
                return BadRequest();

            var user = await _context.Users
                    .Where(u => u.Token == Token)
                    .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            var link = await _context.Links
                .Where(l => l.UserId == user.TGID)
                .Where(l => l.DeviceID == id)
                .FirstOrDefaultAsync();

            if (link == null)
            {
                return NotFound();
            }

            var device = await _context.Devices
                .Where(d => d.Id == link.DeviceID)
                .FirstOrDefaultAsync();

            if (device != null)
                device.TurnOff = true;

            _context.Devices.Update(device);
            await _context.SaveChangesAsync();

            return Ok();
        }



        [HttpGet("listener/")]
        public async Task<IActionResult> Listener(string Token, string id)
        {
            if (Token != PCToken)
                return BadRequest();

            var device = await _context.Devices
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();

            if (device == null)
                return BadRequest();

            DateTime currentTime = DateTime.Now.ToUniversalTime();

            if (device.TurnOff)
            {
                if (device.LastOnline.Day != currentTime.Day)
                {
                    device.TurnOff = false;
                    device.HourUsed = device.MinuteUsed = device.MinuteUsed = 0;

                    _context.Devices.Update(device);
                    await _context.SaveChangesAsync();
                }

                return Ok(device.TurnOff);
            }

            //TimeSpan timeDifference = currentTime - device.LastOnline;

            //int hours = (int)timeDifference.Hours;
            //int minutes = (int)timeDifference.Minutes;

            //device.HourUsed += (device.HourUsed + hours + (device.MinuteUsed + minutes) / 60) % 24;
            //device.MinuteUsed = (device.MinuteUsed + minutes) % 60;

            device.second_used += Math.Abs(currentTime.Second - device.LastOnline.Second);

            if (device.second_used >= 60)
            {
                device.MinuteUsed++;
                device.second_used -= 60;
            }

            if (device.MinuteUsed == 60)
            {
                device.HourUsed++;
                device.MinuteUsed -= 60;
            }

            device.LastOnline = currentTime;

            if (!(device.HourLimit == 0 & device.MinuteLimit == 0))
                if (device.HourLimit == device.HourUsed & device.MinuteLimit == device.MinuteUsed)
                    device.TurnOff = true;

            _context.Devices.Update(device);
            await _context.SaveChangesAsync();

            return Ok(device.TurnOff);
        }


        [HttpPut]
        public async Task<IActionResult> ConnectDeviceWithUser(string? Token, [FromBody] Device deviceTG)
        {
            var user = await _context.Users
                    .Where(u => u.Token == Token)
                    .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            if (deviceTG.Id == null)
                return BadRequest();

            var device = await _context.Devices
                   .Where(d => d.Id == deviceTG.Id)
                   .FirstOrDefaultAsync();

            if (device == null)
                return NoContent();

            Link link = new()
            {
                DeviceID = deviceTG.Id,
                UserId = user.TGID
            };

            var linkInDB = await _context.Links
                .Where(l => l.UserId == user.TGID)
                .Where(l => l.DeviceID == deviceTG.Id)
                .FirstOrDefaultAsync();

            if (linkInDB == null)
            {
                _context.Links.Add(link);
            }

            if (deviceTG.Name == null)
                return BadRequest();

            device.Name = deviceTG.Name;
            device.HourLimit = deviceTG.HourLimit;
            device.MinuteLimit = deviceTG.MinuteLimit;
            device.DaysOfWeek = deviceTG.DaysOfWeek;


            _context.Devices.Update(device);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(string? Token, string id)
        {
            if (id == null)
                return BadRequest();

            var user = await _context.Users
                    .Where(u => u.Token == Token)
                    .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            var link = await _context.Links
                .Where(link => link.DeviceID == id)
                .Where(link => link.UserId == user.TGID)
                .FirstOrDefaultAsync();

            if (link == null)
                return NotFound();

            _context.Links.Remove(link);
            _context.SaveChanges();

            return Ok();
        }
    }
}
