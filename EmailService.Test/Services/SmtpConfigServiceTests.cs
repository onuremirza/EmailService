using EmailService.Application.Models;
using EmailService.Application.Services;
using EmailService.Domain.Entities;
using EmailService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EmailService.Test.Services;

public class SmtpConfigServiceTests
{
    private readonly EmailDbContext _context;
    private readonly SmtpConfigService _service;

    public SmtpConfigServiceTests()
    {
        DbContextOptions<EmailDbContext> options = new DbContextOptionsBuilder<EmailDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new EmailDbContext(options);
        _service = new SmtpConfigService(_context);
    }

    private static SmtpConfigRequestModel GetValidModel(bool isDefault = false)
    {
        return new()
        {
            Host = "smtp.test.com",
            Port = 587,
            EnableSsl = true,
            ForceTls = false,
            Username = "user",
            Password = "pass",
            From = "from@test.com",
            FromName = "Test Sender",
            UnsubscribeUrl = "https://unsubscribe.com",
            SupportsHtml = true,
            Domain = "test.com",
            DkimSelector = "dkim",
            Timeout = 60,
            RateLimitPerMinute = 100,
            MaxRetries = 3,
            Priority = 1,
            CustomHeadersJson = "{}",
            IsDefault = isDefault,
            IsActive = true,
            Headers =
        [
            new() { Key = "X-Test", Value = "123" }
        ]
        };
    }

    private static SmtpConfig GetValidEntity(bool isDefault = false)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Host = "smtp.test.com",
            Port = 587,
            EnableSsl = true,
            ForceTls = false,
            Username = "user",
            Password = "pass",
            From = "from@test.com",
            FromName = "Test Sender",
            UnsubscribeUrl = "https://unsubscribe.test.com",
            SupportsHtml = true,
            Domain = "test.com",
            DkimSelector = "dkim",
            Timeout = 60,
            RateLimitPerMinute = 100,
            MaxRetries = 3,
            Priority = 1,
            CustomHeadersJson = "{}",
            IsDefault = isDefault,
            IsActive = true,
            Headers = [],
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task CreateAsync_Should_Add_Config()
    {
        SmtpConfigRequestModel model = GetValidModel();

        SmtpConfig created = await _service.CreateAsync(model);

        Assert.NotNull(created);
        Assert.Equal("smtp.test.com", created.Host);
        _ = Assert.Single(await _context.SmtpConfigs.ToListAsync());
    }

    [Fact]
    public async Task CreateAsync_Should_Replace_Previous_Default_If_New_IsDefault()
    {
        _ = _context.SmtpConfigs.Add(GetValidEntity(isDefault: true));
        _ = await _context.SaveChangesAsync();

        SmtpConfigRequestModel model = GetValidModel(isDefault: true);

        SmtpConfig created = await _service.CreateAsync(model);
        List<SmtpConfig> all = await _context.SmtpConfigs.ToListAsync();

        Assert.True(created.IsDefault);
        _ = Assert.Single(all, c => c.IsDefault);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All()
    {
        _ = _context.SmtpConfigs.Add(GetValidEntity());
        _ = await _context.SaveChangesAsync();

        List<SmtpConfig> result = await _service.GetAllAsync();

        _ = Assert.Single(result);
    }

    [Fact]
    public async Task GetDefaultAsync_Should_Return_Default_When_Exists()
    {
        SmtpConfig config = GetValidEntity(isDefault: true);
        _ = _context.SmtpConfigs.Add(config);
        _ = await _context.SaveChangesAsync();

        SmtpConfig? result = await _service.GetDefaultAsync();

        Assert.NotNull(result);
        Assert.Equal(config.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Correct_Config()
    {
        SmtpConfig config = GetValidEntity();
        _ = _context.SmtpConfigs.Add(config);
        _ = await _context.SaveChangesAsync();

        SmtpConfig? result = await _service.GetByIdAsync(config.Id);

        Assert.NotNull(result);
        Assert.Equal("smtp.test.com", result!.Host);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Config()
    {
        SmtpConfig config = await _service.CreateAsync(GetValidModel());
        SmtpConfigRequestModel updatedModel = GetValidModel();
        updatedModel.Host = "smtp.updated.com";
        updatedModel.Headers =
        [
            new() { Key = "X-Updated", Value = "999" }
        ];

        bool result = await _service.UpdateAsync(config.Id, updatedModel);

        Assert.True(result);
        SmtpConfig? updated = await _context.SmtpConfigs.FindAsync(config.Id);
        Assert.Equal("smtp.updated.com", updated!.Host);
        _ = Assert.Single(updated.Headers);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_False_When_Not_Found()
    {
        SmtpConfigRequestModel model = GetValidModel();
        bool result = await _service.UpdateAsync(Guid.NewGuid(), model);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Config()
    {
        SmtpConfig config = await _service.CreateAsync(GetValidModel());

        bool result = await _service.DeleteAsync(config.Id);

        Assert.True(result);
        Assert.Null(await _context.SmtpConfigs.FindAsync(config.Id));
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_False_When_Not_Found()
    {
        bool result = await _service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }
}
