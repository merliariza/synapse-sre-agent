using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SynapseSRE.Api.Controllers;
using SynapseSRE.Application.DTOs;
using SynapseSRE.Domain.Entities;

public class AttachmentController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AttachmentController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet("incident/{incidentId}")]
    public async Task<ActionResult<IEnumerable<IncidentAttachmentDto>>> GetByIncident(Guid incidentId)
    {
        var attachments = _unitOfWork.Attachments.Find(a => a.IncidentId == incidentId);    
        return Ok(_mapper.Map<IEnumerable<IncidentAttachmentDto>>(attachments));
    }

    [HttpPost]
    public async Task<ActionResult<IncidentAttachmentDto>> Post(IncidentAttachmentDto dto)
    {
        var attachment = _mapper.Map<IncidentAttachment>(dto);
        _unitOfWork.Attachments.Add(attachment);
        await _unitOfWork.SaveAsync();
        return Ok(_mapper.Map<IncidentAttachmentDto>(attachment));
    }
}