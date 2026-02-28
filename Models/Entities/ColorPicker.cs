using TaskMonitoringApp.Models.Enums; // Added using

namespace TaskMonitoringApp.Models.Entities
{
    // Enum removed from here

    public class ColorPicker
    {
        public int Id { get; set; }

        public ColorType Type { get; set; } // Enum to store type of color format

        public string ColorCode { get; set; } // Stores color value

        public string? Name { get; set; } // Optional color name
    }
}
