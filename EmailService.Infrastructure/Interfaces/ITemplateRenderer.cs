namespace EmailService.Infrastructure.Interfaces
{
    public interface ITemplateRenderer
    {
        string Render(string template, Dictionary<string, string> parameters, string? unsubscribeToken = null, bool appendUnsubscribe = true);
    }
}
