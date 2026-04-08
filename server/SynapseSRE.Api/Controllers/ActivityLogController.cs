using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SynapseSRE.Api.Controllers;
using SynapseSRE.Application.DTOs;

public class ActivityLogController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ActivityLogController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityLogDto>>> GetAll() => 
        Ok(_mapper.Map<IEnumerable<ActivityLogDto>>(await _unitOfWork.Logs.GetAllAsync()));

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<ActivityLogDto>>> GetByUser(Guid userId)
    {
        var logs = _unitOfWork.Logs.Find(l => l.UserId == userId);
        return Ok(_mapper.Map<IEnumerable<ActivityLogDto>>(logs));
    }
}