namespace EmailService.Domain.ValueObjects
{
    public class EmailRecipientMessage
    {
        public string To { get; set; } = null!;
        public Dictionary<string, string> Params { get; set; } = [];
        public string? UnsubscribeToken { get; init; }
    }

}
