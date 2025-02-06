namespace TaskMonitoringApp.Models.Entities
{
    public enum ColorType
    {
        Hex,   // #RRGGBB or #RGB
        Rgb,   // rgb(255,0,0)
        Rgba,  // rgba(255,0,0,0.5)
        Hsl,   // hsl(360, 100%, 50%)
        Hsla,   // hsla(360, 100%, 50%, 0.5)
        name, //red,blue,black,etc...
    }


    public class ColorPicker
    {
        public int Id { get; set; }

        public ColorType Type { get; set; } // Enum to store type of color format

        public string ColorCode { get; set; } // Stores color value

        public string? Name { get; set; } // Optional color name
    }
}
