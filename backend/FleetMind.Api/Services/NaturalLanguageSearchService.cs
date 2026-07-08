using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.DTOs.Cargo;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.DTOs.Ships;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.Services.Ai;
using FleetMind.Api.Services.Ai.PromptBuilders;

namespace FleetMind.Api.Services
{
    public class NaturalLanguageSearchService : INaturalLanguageSearchService
    {
        private readonly IAiProvider _aiProvider;
        private readonly ICargoService _cargoService;
        private readonly IVoyageService _voyageService;
        private readonly IShipService _shipService;
        private readonly IIncidentService _incidentService;

        private record ParsedSearchIntent(string? module, Dictionary<string, JsonElement>? filters);

        public NaturalLanguageSearchService(
            IAiProvider aiProvider,
            ICargoService cargoService,
            IVoyageService voyageService,
            IShipService shipService,
            IIncidentService incidentService)
        {
            _aiProvider = aiProvider;
            _cargoService = cargoService;
            _voyageService = voyageService;
            _shipService = shipService;
            _incidentService = incidentService;
        }

        public async Task<NaturalLanguageSearchResultDto> ParseAndSearchAsync(string query)
        {
            if (!_aiProvider.IsAvailable)
            {
                return new NaturalLanguageSearchResultDto
                {
                    IsAvailable = false,
                    Message = "AI Provider is currently unavailable. Please use standard search.",
                    InterpretedFilters = new Dictionary<string, object>()
                };
            }

            var builder = new NaturalLanguageSearchPromptBuilder();
            var aiResponse = await _aiProvider.CompleteAsync(new Common.AiCompletionRequest
            {
                SystemPrompt = builder.BuildSystemPrompt(),
                UserPrompt = builder.BuildUserPrompt(query)
            });

            if (!aiResponse.IsSuccess)
            {
                return new NaturalLanguageSearchResultDto
                {
                    IsAvailable = false,
                    Message = "Unable to process your search right now.",
                    InterpretedFilters = new Dictionary<string, object>()
                };
            }

            ParsedSearchIntent? intent = null;
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                // Sometimes the model might wrap JSON in markdown block like ```json
                var content = aiResponse.Content.Trim();
                if (content.StartsWith("```json")) content = content.Substring(7);
                if (content.StartsWith("```")) content = content.Substring(3);
                if (content.EndsWith("```")) content = content.Substring(0, content.Length - 3);
                content = content.Trim();
                
                intent = JsonSerializer.Deserialize<ParsedSearchIntent>(content, options);
            }
            catch
            {
                return HandleUnparseable();
            }

            if (intent == null || string.IsNullOrWhiteSpace(intent.module))
            {
                return HandleUnparseable();
            }

            var allowedModules = new[] { "Cargo", "Voyage", "Ship", "Incident" };
            var moduleMatch = allowedModules.FirstOrDefault(m => string.Equals(m, intent.module, StringComparison.OrdinalIgnoreCase));
            if (moduleMatch == null)
            {
                return HandleUnparseable();
            }

            // Map and Validate
            var validatedFilters = new Dictionary<string, object>();
            object? results = null;

            switch (moduleMatch)
            {
                case "Cargo":
                    var cargoQuery = BuildQueryDto<CargoQueryDto>(intent.filters, validatedFilters);
                    cargoQuery.PageSize = 20; // Default
                    results = await _cargoService.GetCargoItemsAsync(cargoQuery);
                    break;

                case "Voyage":
                    var voyageQuery = BuildQueryDto<VoyageQueryDto>(intent.filters, validatedFilters);
                    voyageQuery.PageSize = 20;
                    results = await _voyageService.GetVoyagesAsync(voyageQuery);
                    break;

                case "Ship":
                    var shipQuery = BuildQueryDto<ShipQueryDto>(intent.filters, validatedFilters);
                    shipQuery.PageSize = 20;
                    results = await _shipService.GetShipsAsync(shipQuery);
                    break;

                case "Incident":
                    var incidentQuery = BuildQueryDto<IncidentQueryDto>(intent.filters, validatedFilters);
                    incidentQuery.PageSize = 20;
                    results = await _incidentService.GetIncidentsAsync(incidentQuery);
                    break;
            }

            return new NaturalLanguageSearchResultDto
            {
                IsAvailable = true,
                InterpretedModule = moduleMatch,
                InterpretedFilters = validatedFilters,
                Results = results
            };
        }

        private NaturalLanguageSearchResultDto HandleUnparseable()
        {
            return new NaturalLanguageSearchResultDto
            {
                IsAvailable = true, // Call succeeded, but couldn't understand
                Message = "I couldn't understand that query - try being more specific.",
                InterpretedFilters = new Dictionary<string, object>()
            };
        }

        private T BuildQueryDto<T>(Dictionary<string, JsonElement>? rawFilters, Dictionary<string, object> validatedFilters) where T : new()
        {
            var dto = new T();
            if (rawFilters == null) return dto;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var kvp in rawFilters)
            {
                var prop = properties.FirstOrDefault(p => string.Equals(p.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));
                if (prop == null || !prop.CanWrite) continue; // Property doesn't exist on DTO

                try
                {
                    object? parsedValue = null;
                    var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (type == typeof(string))
                    {
                        parsedValue = kvp.Value.GetString();
                    }
                    else if (type == typeof(Guid))
                    {
                        if (Guid.TryParse(kvp.Value.GetString(), out var g)) parsedValue = g;
                    }
                    else if (type == typeof(DateTime))
                    {
                        if (kvp.Value.TryGetDateTime(out var dt)) parsedValue = dt;
                    }
                    else if (type == typeof(int))
                    {
                        if (kvp.Value.TryGetInt32(out var i)) parsedValue = i;
                    }
                    else if (type == typeof(decimal))
                    {
                        if (kvp.Value.TryGetDecimal(out var d)) parsedValue = d;
                    }
                    else if (type == typeof(bool))
                    {
                        parsedValue = kvp.Value.GetBoolean();
                    }
                    else if (type.IsEnum)
                    {
                        var strVal = kvp.Value.GetString();
                        if (strVal != null && Enum.TryParse(type, strVal, true, out var eVal))
                        {
                            parsedValue = eVal;
                        }
                    }

                    if (parsedValue != null)
                    {
                        prop.SetValue(dto, parsedValue);
                        validatedFilters.Add(prop.Name, parsedValue); // Use exact property name for transparency
                    }
                }
                catch
                {
                    // Discard this filter if it fails conversion
                }
            }

            return dto;
        }
    }
}
