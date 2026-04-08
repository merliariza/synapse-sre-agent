using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SynapseSRE.Application.DTOs;
using SynapseSRE.Domain.Entities;
using SynapseSRE.Domain.Interfaces;

namespace SynapseSRE.Api.Controllers;

public class UserController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> Get() => 
        Ok(_mapper.Map<IEnumerable<UserDto>>(await _unitOfWork.Users.GetAllAsync()));

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Get(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        return user == null ? NotFound("Usuario no encontrado") : Ok(_mapper.Map<UserDto>(user));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<UserDto>> Post(UserDto dto)
    {
        var user = _mapper.Map<User>(dto);
        _unitOfWork.Users.Add(user);
        await _unitOfWork.SaveAsync();
        return CreatedAtAction(nameof(Get), new { id = user.Id }, _mapper.Map<UserDto>(user));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, UserDto dto)
    {
        var existing = await _unitOfWork.Users.GetByIdAsync(id);
        if (existing == null) return NotFound();
        _mapper.Map(dto, existing);
        _unitOfWork.Users.Update(existing);
        await _unitOfWork.SaveAsync();
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return NotFound();
        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveAsync();
        return NoContent();
    }
}