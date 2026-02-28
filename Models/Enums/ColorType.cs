namespace TaskMonitoringApp.Models.Enums
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
}