using EmailService.Application.Interfaces;
using EmailService.Domain.Entities;
using EmailService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmailService.Application.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly EmailDbContext _db;

    public EmailTemplateService(EmailDbContext db)
    {
        _db = db;
    }

    public async Task<List<EmailTemplate>> GetAllAsync()
    {
        return await _db.EmailTemplates.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<EmailTemplate?> GetByCodeAsync(string code)
    {
        return await _db.EmailTemplates.FirstOrDefaultAsync(x => x.Code == code);
    }

    public async Task<EmailTemplate> CreateAsync(EmailTemplate template)
    {
        _ = _db.EmailTemplates.Add(template);
        _ = await _db.SaveChangesAsync();
        return template;
    }

    public async Task<bool> UpdateAsync(Guid id, EmailTemplate updated)
    {
        EmailTemplate? existing = await _db.EmailTemplates.FindAsync(id);
        if (existing == null)
        {
            return false;
        }

        _db.Entry(existing).CurrentValues.SetValues(updated);
        existing.UpdatedAt = DateTime.UtcNow;
        _ = await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        EmailTemplate? template = await _db.EmailTemplates.FindAsync(id);
        if (template == null)
        {
            return false;
        }

        _ = _db.EmailTemplates.Remove(template);
        _ = await _db.SaveChangesAsync();
        return true;
    }
}
