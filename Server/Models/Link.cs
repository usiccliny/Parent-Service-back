using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Link
    {
        [Key]
        public string? DeviceID { get; set; }

        [Key]
        public int UserId { get; set; }

    }
}
