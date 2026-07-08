using System;
using System.Collections.Generic;

namespace FleetMind.Api.DTOs.Ai
{
    public class NaturalLanguageSearchRequestDto
    {
        public string Query { get; set; } = string.Empty;
    }

    public class NaturalLanguageSearchResultDto
    {
        public string? InterpretedModule { get; set; }
        public Dictionary<string, object> InterpretedFilters { get; set; } = new Dictionary<string, object>();
        public object? Results { get; set; }
        public bool IsAvailable { get; set; }
        public string? Message { get; set; }
    }
}
