using EmailService.Infrastructure.Interfaces;

namespace EmailService.Infrastructure.Email;

public class TemplateRenderer : ITemplateRenderer
{
    public string Render(string template, Dictionary<string, string> parameters, string? unsubscribeToken = null, bool appendUnsubscribe = true)
    {
        foreach (KeyValuePair<string, string> param in parameters)
        {
            template = template.Replace($"{{{{{param.Key}}}}}", param.Value, StringComparison.OrdinalIgnoreCase);
        }

        if (appendUnsubscribe && !string.IsNullOrWhiteSpace(unsubscribeToken))
        {
            string link = $"<a href=\"{unsubscribeToken}\">Unsubscribe</a>";
            template += $"<br/><p style=\"font-size:12px;color:#888\">{link}</p>";
        }

        return template;
    }
}
