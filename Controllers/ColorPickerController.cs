using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;

namespace TaskMonitoringApp.Controllers
{
    public class ColorPickerController : Controller
    {
        [HttpPost]
        public IActionResult SaveColor(ColorPicker model)
        {
            if (ModelState.IsValid && IsValidColorCode(model.ColorCode, model.Type))
            {
                // Save color to database or process further
                return Json(new { success = true, message = "Color saved successfully!" });
            }
            return Json(new { success = false, message = "Invalid color format." });
        }

        // Helper method to validate color format
        private bool IsValidColorCode(string colorCode, ColorType type)
        {
            string hexPattern = "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";
            string rgbPattern = @"^rgb\(\s*\d{1,3},\s*\d{1,3},\s*\d{1,3}\s*\)$";
            string rgbaPattern = @"^rgba\(\s*\d{1,3},\s*\d{1,3},\s*\d{1,3},\s*(0(\.\d+)?|1(\.0)?)\s*\)$";
            string hslPattern = @"^hsl\(\s*\d{1,3},\s*\d{1,3}%?,\s*\d{1,3}%?\s*\)$";
            string hslaPattern = @"^hsla\(\s*\d{1,3},\s*\d{1,3}%?,\s*\d{1,3}%?,\s*(0(\.\d+)?|1(\.0)?)\s*\)$";

            return type switch
            {
                ColorType.Hex => Regex.IsMatch(colorCode, hexPattern),
                ColorType.Rgb => Regex.IsMatch(colorCode, rgbPattern),
                ColorType.Rgba => Regex.IsMatch(colorCode, rgbaPattern),
                ColorType.Hsl => Regex.IsMatch(colorCode, hslPattern),
                ColorType.Hsla => Regex.IsMatch(colorCode, hslaPattern),
                _ => false,
            };
        }

    }
}
