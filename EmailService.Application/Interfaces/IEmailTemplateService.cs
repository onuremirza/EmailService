using EmailService.Domain.Entities;

namespace EmailService.Application.Interfaces;

public interface IEmailTemplateService
{
    Task<List<EmailTemplate>> GetAllAsync();
    Task<EmailTemplate?> GetByCodeAsync(string code);
    Task<EmailTemplate> CreateAsync(EmailTemplate template);
    Task<bool> UpdateAsync(Guid id, EmailTemplate updated);
    Task<bool> DeleteAsync(Guid id);
}
