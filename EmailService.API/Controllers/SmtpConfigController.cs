using EmailService.Application.Interfaces;
using EmailService.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SmtpConfigController : ControllerBase
{
    private readonly ISmtpConfigService _service;

    public SmtpConfigController(ISmtpConfigService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("default")]
    public async Task<IActionResult> GetDefault()
    {
        Domain.Entities.SmtpConfig? config = await _service.GetDefaultAsync();
        return config == null
            ? NotFound(new { error = "Default SMTP configuration not found." })
            : Ok(config);
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        Domain.Entities.SmtpConfig? config = await _service.GetByIdAsync(id);
        return config == null ? NotFound() : Ok(config);
    }

    [HttpPost]
    public async Task<IActionResult> Create(SmtpConfigRequestModel model)
    {
        var created = await _service.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> Update(Guid id, SmtpConfigRequestModel model)
    {
        var success = await _service.UpdateAsync(id, model);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
