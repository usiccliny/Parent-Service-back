using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Contexts;
using Server.Models;
using System.Security.Cryptography;
using System.Text;
using System.Web;


namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly DatabaseContext _context;
        public string? TelegramBotToken, ReturnedToken;
        public int _TGID;

        public UsersController(DatabaseContext context)
        {
            _context = context;

            var builder = WebApplication.CreateBuilder();
            TelegramBotToken = builder.Configuration["TelegramBotToken"];
        }

        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] string initData)
        {
            if (initData == null)
                return NoContent();

            var data = HttpUtility.ParseQueryString(initData);

            var dataDict = new SortedDictionary<string, string>(
                data.AllKeys.ToDictionary(x => x!, x => data[x]!),
                StringComparer.Ordinal);

            var constantKey = "WebAppData";

            var dataCheckString = string.Join(
                '\n', dataDict.Where(x => x.Key != "hash") // Hash should be removed.
                    .Select(x => $"{x.Key}={x.Value}"));

            var secretKey = HMACSHA256.HashData(
                Encoding.UTF8.GetBytes(constantKey), // WebAppData
                Encoding.UTF8.GetBytes(TelegramBotToken)); // Bot's token

            var generatedHash = HMACSHA256.HashData(
                secretKey,
                Encoding.UTF8.GetBytes(dataCheckString)); // data_check_string

            var actualHash = Convert.FromHexString(dataDict["hash"]);


            // Compare our hash with the one from telegram.
            if (actualHash.SequenceEqual(generatedHash))
            {
                Guid myuuid = Guid.NewGuid();
                string response;
                ReturnedToken = myuuid.ToString();
                _TGID = int.Parse(dataDict["user"].Split('\"', StringSplitOptions.RemoveEmptyEntries)[2].Trim(':', ','));

                var user = await _context.Users.FindAsync(_TGID);

                if (user != null)
                {
                    user.Token = ReturnedToken;

                    _context.Users.Update(user);
                    _context.SaveChanges();

                    response = JsonConvert.SerializeObject(user.Token);

                    return Ok(response);
                }

                User NewUser = new User()
                {
                    Token = ReturnedToken,
                    PrimeStatus = 1,
                    TGID = _TGID
                };

                _context.Users.Add(NewUser);

                _context.SaveChanges();

                response = JsonConvert.SerializeObject(NewUser.Token);

                return Ok(response);
            }

            return NotFound();
        }

        [HttpPut]
        public async Task<IActionResult> PutUser(string? Token, string InputData)
        {
            if (InputData == null)
                return NoContent();

            var user = await _context.Users
                .Where(b => b.Token == Token)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            //изменения прописать

            return Ok(user);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(string? Token)
        {
            var user = await _context.Users
                .Where(b => b.Token == Token)
                .FirstOrDefaultAsync();

            if (user == null)
                return NoContent();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(user);

        }
    }
}
