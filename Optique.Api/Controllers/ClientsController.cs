using Microsoft.AspNetCore.Mvc;
using Optique.Application.DTOs;
using Optique.Application.Interfaces;

namespace Optique.Api.Controllers;

[ApiController]
[Route("api/v1/clients")]
public sealed class ClientsController : ControllerBase
{
    private readonly IClientService _svc;
    public ClientsController(IClientService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<List<ClientDto>>> List([FromQuery] string? q)
        => Ok(await _svc.ListAsync(q));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientDto>> Get(int id)
        => (await _svc.GetAsync(id)) is { } c ? Ok(c) : NotFound();

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] ClientCreateDto dto)
    {
        var id = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }
}
