using EmailService.Application.Interfaces;
using EmailService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailTemplateController : ControllerBase
{
    private readonly IEmailTemplateService _service;

    public EmailTemplateController(IEmailTemplateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        List<EmailTemplate> templates = await _service.GetAllAsync();
        return Ok(templates);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        EmailTemplate? template = await _service.GetByCodeAsync(code);
        return template == null ? NotFound() : Ok(template);
    }

    [HttpPost]
    public async Task<IActionResult> Create(EmailTemplate model)
    {
        EmailTemplate created = await _service.CreateAsync(model);
        return CreatedAtAction(nameof(GetByCode), new { code = created.Code }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(Guid id, EmailTemplate updated)
    {
        bool success = await _service.UpdateAsync(id, updated);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
