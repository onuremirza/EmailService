using EmailService.Application.Services;
using EmailService.Domain.Entities;
using EmailService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EmailService.Test.Services;

public class EmailTemplateServiceTests
{
    private readonly EmailDbContext _context;
    private readonly EmailTemplateService _service;

    public EmailTemplateServiceTests()
    {
        DbContextOptions<EmailDbContext> options = new DbContextOptionsBuilder<EmailDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new EmailDbContext(options);
        _service = new EmailTemplateService(_context);
    }

    private static EmailTemplate GetValidTemplate(string code = "test", string name = "Template", string subject = "Subject", string body = "Body", DateTime? createdAt = null)
    {
        return new EmailTemplate
        {
            Code = code,
            Name = name,
            Subject = subject,
            Body = body,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
    }

    [Fact]
    public async Task CreateAsync_Should_Add_Template()
    {
        EmailTemplate template = GetValidTemplate("test", "Test Template", "Hello", "World");

        EmailTemplate result = await _service.CreateAsync(template);

        Assert.NotNull(result);
        Assert.Equal("test", result.Code);
        _ = Assert.Single(_context.EmailTemplates);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Templates()
    {
        _ = _context.EmailTemplates.Add(GetValidTemplate("a", "Template A", "Subject A", "Body A", DateTime.UtcNow.AddDays(-1)));
        _ = _context.EmailTemplates.Add(GetValidTemplate("b", "Template B", "Subject B", "Body B", DateTime.UtcNow));
        _ = await _context.SaveChangesAsync();

        List<EmailTemplate> result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("b", result.First().Code);
    }

    [Fact]
    public async Task GetByCodeAsync_Should_Return_Template_When_Exists()
    {
        _ = _context.EmailTemplates.Add(GetValidTemplate("abc", "Test Template", "S", "Body of the template"));
        _ = await _context.SaveChangesAsync();

        EmailTemplate? result = await _service.GetByCodeAsync("abc");

        Assert.NotNull(result);
        Assert.Equal("S", result.Subject);
    }

    [Fact]
    public async Task GetByCodeAsync_Should_Return_Null_When_Not_Exists()
    {
        EmailTemplate? result = await _service.GetByCodeAsync("notfound");

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Should_Modify_Existing_Template()
    {
        EmailTemplate template = GetValidTemplate("edit", "Template Name", "Old Subject", "Old Body");
        _ = _context.EmailTemplates.Add(template);
        _ = await _context.SaveChangesAsync();

        template.Subject = "New Subject";
        template.Body = "New Body";

        bool result = await _service.UpdateAsync(template.Id, template);

        Assert.True(result);
        Assert.Equal("New Subject", _context.EmailTemplates.First().Subject);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_False_When_NotFound()
    {
        Guid nonExistentId = Guid.NewGuid();
        EmailTemplate template = GetValidTemplate("notexist");
        template.Id = nonExistentId;

        bool result = await _service.UpdateAsync(nonExistentId, template);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Template()
    {
        EmailTemplate template = GetValidTemplate("del", "Delete Template", "To be deleted", "This template will be deleted");
        _ = _context.EmailTemplates.Add(template);
        _ = await _context.SaveChangesAsync();

        bool result = await _service.DeleteAsync(template.Id);

        Assert.True(result);
        Assert.Empty(_context.EmailTemplates);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_False_When_NotFound()
    {
        bool result = await _service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }
}
