using System.Threading.Tasks;
using FleetMind.Api.DTOs.Ai;

namespace FleetMind.Api.Services
{
    public interface INaturalLanguageSearchService
    {
        Task<NaturalLanguageSearchResultDto> ParseAndSearchAsync(string query);
    }
}
