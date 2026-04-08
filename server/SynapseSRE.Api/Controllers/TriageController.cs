using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SynapseSRE.Api.Controllers;
using SynapseSRE.Application.DTOs;
using SynapseSRE.Domain.Entities;

public class TriageController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TriageController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TriageAnalysisDto>>> Get() => 
        Ok(_mapper.Map<IEnumerable<TriageAnalysisDto>>(await _unitOfWork.TriageAnalyses.GetAllAsync()));

    [HttpPost]
    public async Task<ActionResult<TriageAnalysisDto>> Post(TriageAnalysisDto dto)
    {
        var triage = _mapper.Map<TriageAnalysis>(dto);
        _unitOfWork.TriageAnalyses.Add(triage);
        await _unitOfWork.SaveAsync();
        return CreatedAtAction(nameof(Get), new { id = triage.Id }, _mapper.Map<TriageAnalysisDto>(triage));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var triage = await _unitOfWork.TriageAnalyses.GetByIdAsync(id);
        if (triage == null) return NotFound();
        _unitOfWork.TriageAnalyses.Remove(triage);
        await _unitOfWork.SaveAsync();
        return NoContent();
    }
}