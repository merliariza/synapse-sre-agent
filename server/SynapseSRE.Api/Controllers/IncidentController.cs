using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SynapseSRE.Application.DTOs;
using SynapseSRE.Domain.Entities;
using SynapseSRE.Domain.Interfaces;

namespace SynapseSRE.Api.Controllers;

public class IncidentController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public IncidentController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IncidentDto>>> Get()
    {
        var incidents = await _unitOfWork.Incidents.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<IncidentDto>>(incidents));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> Get(Guid id)
    {
        var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
        if (incident == null) return NotFound("El incidente no existe.");
        return Ok(_mapper.Map<IncidentDto>(incident));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentDto>> Post(IncidentDto dto)
    {
        var incident = _mapper.Map<Incident>(dto);
        _unitOfWork.Incidents.Add(incident);
        await _unitOfWork.SaveAsync();
        return CreatedAtAction(nameof(Get), new { id = incident.Id }, _mapper.Map<IncidentDto>(incident));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, [FromBody] IncidentDto dto)
    {
        var existing = await _unitOfWork.Incidents.GetByIdAsync(id);
        if (existing == null) return NotFound("Inicidente no encontrado.");

        _mapper.Map(dto, existing); // Actualiza los campos
        _unitOfWork.Incidents.Update(existing);
        await _unitOfWork.SaveAsync();
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
        if (incident == null) return NotFound();

        _unitOfWork.Incidents.Remove(incident);
        await _unitOfWork.SaveAsync();
        return NoContent();
    }
}