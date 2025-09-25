using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
public class OrdonnancesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    public OrdonnancesController(IWebHostEnvironment env) => _env = env;

    [HttpPost("upload")]
    [RequestSizeLimit(20_000_000)] // 20 Mo
    public async Task<ActionResult<string>> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Fichier vide");

        // Dossier wwwroot/uploads/ordonnances
        var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"),
                                   "uploads", "ordonnances");
        Directory.CreateDirectory(uploads);

        var ext = Path.GetExtension(file.FileName); // .png / .jpg / .pdf …
        var name = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploads, name);

        using (var stream = System.IO.File.Create(fullPath))
            await file.CopyToAsync(stream);

        // URL publique relative (ex: /uploads/ordonnances/xxxx.png)
        var publicUrl = $"/uploads/ordonnances/{name}";
        return Ok(publicUrl);
    }
}
