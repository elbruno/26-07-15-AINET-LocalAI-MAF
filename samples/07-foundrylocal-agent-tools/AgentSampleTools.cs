using Microsoft.Extensions.AI;

internal static class AgentSampleTools
{
    internal static IList<AITool> BuildTools()
    {
        return
        [
            AIFunctionFactory.Create(
                (string timezoneId) => GetTimeInTimezone(timezoneId),
                "get_time_in_timezone",
                "Get current local time for an IANA/Windows timezone id. Use 'local' for machine local time."),

            AIFunctionFactory.Create(
                (double amount, double percentage) => CalculateTip(amount, percentage),
                "calculate_tip",
                "Calculate a tip amount from bill amount and percentage."),

            AIFunctionFactory.Create(
                (string topic) => GetDemoFact(topic),
                "get_demo_fact",
                "Return a short deterministic demo fact about a topic.")
        ];
    }

    private static string GetTimeInTimezone(string timezoneId)
    {
        if (string.IsNullOrWhiteSpace(timezoneId) ||
            timezoneId.Equals("local", StringComparison.OrdinalIgnoreCase))
        {
            return DateTimeOffset.Now.ToString("O");
        }

        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, tz).ToString("O");
        }
        catch
        {
            return DateTimeOffset.Now.ToString("O");
        }
    }

    private static string CalculateTip(double amount, double percentage)
    {
        var tip = Math.Round(amount * percentage / 100.0, 2, MidpointRounding.AwayFromZero);
        var total = Math.Round(amount + tip, 2, MidpointRounding.AwayFromZero);
        return $"tip={tip:F2};total={total:F2}";
    }

    private static string GetDemoFact(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return "Local AI keeps model execution on your machine for privacy and low latency.";
        }

        return $"Demo fact about {topic.Trim()}: local agent workflows can use tools without a REST endpoint.";
    }
}
