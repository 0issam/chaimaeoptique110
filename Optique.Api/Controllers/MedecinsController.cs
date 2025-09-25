using Microsoft.AspNetCore.Mvc;
using Optique.Application.DTOs;
using Optique.Application.Interfaces;

namespace Optique.Api.Controllers;

[ApiController]
[Route("api/v1/medecins")]
public sealed class MedecinsController : ControllerBase
{
    private readonly IMedecinService _svc;
    public MedecinsController(IMedecinService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<List<MedecinDto>>> List([FromQuery] string? q)
        => Ok(await _svc.ListAsync(q));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MedecinDto>> Get(int id)
        => (await _svc.GetAsync(id)) is { } m ? Ok(m) : NotFound();

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] MedecinCreateDto dto)
    {
        var id = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }
}
