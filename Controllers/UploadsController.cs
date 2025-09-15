using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace TaskMonitoringApp.Controllers
{
    [Route("uploads")]
    public class UploadsController : Controller
    {
        private readonly long _maxBytes = long.MaxValue; // 10 MB
        private static readonly string[] ImageExts = [".png", ".jpg", ".jpeg", ".gif", ".webp"];
        private static readonly string[] DocExts = [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv", ".zip"];
        private static readonly string[] MediaExts = [".mp4", ".mp3", ".wav", ".webm"];

        private readonly IWebHostEnvironment _env;

        public UploadsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("image")]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<IActionResult> Image(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file.");
            //if (file.Length > _maxBytes) return BadRequest("File too large.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!ImageExts.Contains(ext)) return BadRequest("Invalid image type.");

            var (publicUrl, physPath) = GetTargetPaths("images", ext);
            await using var stream = System.IO.File.Create(physPath);
            await file.CopyToAsync(stream);

            return Ok(new { location = publicUrl });
        }

        [HttpPost("media")]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<IActionResult> Media(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file.");
            //if (file.Length > 1024 * 1024 * 1024) return BadRequest("File too large.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!MediaExts.Contains(ext)) return BadRequest("Invalid media type.");

            var (publicUrl, physPath) = GetTargetPaths("media", ext);
            await using var stream = System.IO.File.Create(physPath);
            await file.CopyToAsync(stream);

            return Ok(new { location = publicUrl });
        }

        [HttpPost("file")]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<IActionResult> FileUpload(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file.");
            //if (file.Length > 30 * 1024 * 1024) return BadRequest("File too large.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!DocExts.Contains(ext)) return BadRequest("Invalid file type.");

            var (publicUrl, physPath) = GetTargetPaths("docs", ext);
            await using var stream = System.IO.File.Create(physPath);
            await file.CopyToAsync(stream);

            return Ok(new { location = publicUrl });
        }

        private (string publicUrl, string physPath) GetTargetPaths(string subfolder, string ext)
        {
            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{RandomString(8)}{ext}";
            var relative = Path.Combine("uploads", subfolder, fileName).Replace('\\', '/');
            var phys = Path.Combine(_env.WebRootPath, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(phys)!);
            var publicUrl = "/" + relative; // If behind CDN/custom domain, build full URL here
            return (publicUrl, phys);
        }

        private static string RandomString(int len)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var data = RandomNumberGenerator.GetBytes(len);
            return new string(data.Select(b => chars[b % chars.Length]).ToArray());
        }
    }

}
