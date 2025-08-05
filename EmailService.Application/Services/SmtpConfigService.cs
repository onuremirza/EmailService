using EmailService.Application.Interfaces;
using EmailService.Application.Models;
using EmailService.Domain.Entities;
using EmailService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmailService.Application.Services;

public class SmtpConfigService : ISmtpConfigService
{
    private readonly EmailDbContext _context;

    public SmtpConfigService(EmailDbContext context)
    {
        _context = context;
    }

    public async Task<List<SmtpConfig>> GetAllAsync()
    {
        return await _context.SmtpConfigs
            .Include(c => c.Headers)
            .OrderByDescending(c => c.IsDefault)
            .ToListAsync();
    }

    public async Task<SmtpConfig?> GetDefaultAsync()
    {
        return await _context.SmtpConfigs
            .Include(c => c.Headers)
            .FirstOrDefaultAsync(c => c.IsDefault && c.IsActive);
    }

    public async Task<SmtpConfig?> GetByIdAsync(Guid id)
    {
        return await _context.SmtpConfigs
            .Include(c => c.Headers)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<SmtpConfig> CreateAsync(SmtpConfigRequestModel model)
    {
        if (model.IsDefault)
        {
            SmtpConfig? currentDefault = await _context.SmtpConfigs.FirstOrDefaultAsync(c => c.IsDefault);
            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
            }
        }

        SmtpConfig config = new()
        {
            Host = model.Host,
            Port = model.Port,
            EnableSsl = model.EnableSsl,
            ForceTls = model.ForceTls,
            Username = model.Username,
            Password = model.Password,
            From = model.From,
            FromName = model.FromName,
            UnsubscribeUrl = model.UnsubscribeUrl,
            SupportsHtml = model.SupportsHtml,
            Domain = model.Domain,
            DkimSelector = model.DkimSelector,
            Timeout = model.Timeout,
            RateLimitPerMinute = model.RateLimitPerMinute,
            MaxRetries = model.MaxRetries,
            Priority = model.Priority,
            CustomHeadersJson = model.CustomHeadersJson,
            IsDefault = model.IsDefault,
            IsActive = model.IsActive,
            Headers = model.Headers.Select(h => new SmtpHeader
            {
                Key = h.Key,
                Value = h.Value
            }).ToList()
        };

        _ = _context.SmtpConfigs.Add(config);
        _ = await _context.SaveChangesAsync();
        return config;
    }

    public async Task<bool> UpdateAsync(Guid id, SmtpConfigRequestModel model)
    {
        SmtpConfig? existing = await _context.SmtpConfigs.Include(c => c.Headers).FirstOrDefaultAsync(c => c.Id == id);
        if (existing == null)
        {
            return false;
        }

        if (model.IsDefault && !existing.IsDefault)
        {
            SmtpConfig? currentDefault = await _context.SmtpConfigs.FirstOrDefaultAsync(c => c.IsDefault);
            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
            }
        }

        _context.Entry(existing).CurrentValues.SetValues(new
        {
            model.Host,
            model.Port,
            model.EnableSsl,
            model.ForceTls,
            model.Username,
            model.Password,
            model.From,
            model.FromName,
            model.UnsubscribeUrl,
            model.SupportsHtml,
            model.Domain,
            model.DkimSelector,
            model.Timeout,
            model.RateLimitPerMinute,
            model.MaxRetries,
            model.Priority,
            model.CustomHeadersJson,
            model.IsDefault,
            model.IsActive
        });

        existing.Headers.Clear();
        foreach (SmtpHeaderRequestModel header in model.Headers)
        {
            existing.Headers.Add(new SmtpHeader
            {
                Key = header.Key,
                Value = header.Value
            });
        }

        _ = await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        SmtpConfig? config = await _context.SmtpConfigs.Include(c => c.Headers).FirstOrDefaultAsync(c => c.Id == id);
        if (config == null)
        {
            return false;
        }

        _ = _context.SmtpConfigs.Remove(config);
        _ = await _context.SaveChangesAsync();
        return true;
    }
}
