using EmailService.Application.Models;
using EmailService.Domain.Entities;

namespace EmailService.Application.Interfaces;

public interface ISmtpConfigService
{
    Task<List<SmtpConfig>> GetAllAsync();
    Task<SmtpConfig?> GetDefaultAsync();
    Task<SmtpConfig?> GetByIdAsync(Guid id);
    Task<SmtpConfig> CreateAsync(SmtpConfigRequestModel model);
    Task<bool> UpdateAsync(Guid id, SmtpConfigRequestModel model);
    Task<bool> DeleteAsync(Guid id);
}
