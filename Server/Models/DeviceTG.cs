namespace Server.Models
{
    public class DeviceTG
    {
        public string? Name { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public string[]? DayTable { get; set; }
        public string? id { get; set; }
    }
}
