namespace FleetMind.Api.Services.Ai.PromptBuilders
{
    public class NaturalLanguageSearchPromptBuilder
    {
        public string BuildSystemPrompt()
        {
            return "You translate natural language search queries into structured search parameters for a maritime operations system. You MUST respond with ONLY a valid JSON object matching this EXACT shape, with no additional text, explanation, or markdown formatting: { \"module\": string, \"filters\": object }. The \"module\" field MUST be exactly one of: \"Cargo\", \"Voyage\", \"Ship\", \"Incident\" - choose whichever best matches the query's subject. The \"filters\" object should contain only simple key-value pairs relevant to narrowing that module's results (for example, for Cargo: type, status; for Voyage: status, departureFrom, departureTo; for Ship: status, type; for Incident: severity, status) - only include a filter key if the query clearly implies a specific value for it, and use ISO date format (yyyy-MM-dd) for any date values. If the query doesn't clearly map to any of the four supported modules, or doesn't clearly imply any useful filters, respond with { \"module\": null, \"filters\": {} }.";
        }

        public string BuildUserPrompt(string userQuery)
        {
            return $"Query: {userQuery}";
        }
    }
}
