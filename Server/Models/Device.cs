using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class Device
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public DateTime LastOnline { get; set; }

        public int HourLimit { get; set; }

        public int MinuteLimit { get; set; }

        public string? DaysOfWeek { get; set; }

        public bool TurnOff { get; set; }

        public int HourUsed { get; set; }

        public int MinuteUsed { get; set; }

        public int second_used { get; set; }
    }
}
