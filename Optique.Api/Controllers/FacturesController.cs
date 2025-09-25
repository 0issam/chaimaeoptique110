using Microsoft.AspNetCore.Mvc;
using Optique.Application.DTOs;
using Optique.Application.Interfaces;

namespace Optique.Api.Controllers;

[ApiController]
[Route("api/v1/factures")]
public sealed class FacturesController : ControllerBase
{
    private readonly IFactureService _svc;
    public FacturesController(IFactureService svc) => _svc = svc;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FactureDto>> Get(int id)
        => (await _svc.GetAsync(id)) is { } f ? Ok(f) : NotFound();

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] FactureCreateDto dto)
    {
        var id = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> GetPdf(int id, CancellationToken ct)
    {
        var res = await _svc.BuildPdfAsync(id, ct);
        if (res is null) return NotFound();

        var (pdf, numero) = res.Value;
        return File(pdf, "application/pdf", $"Facture_{numero}.pdf");
    }

}
