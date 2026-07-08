using System;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Cargo;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Ai;

namespace FleetMind.Api.Services
{
    public interface ICargoService
    {
        Task<PagedResultDto<CargoDto>> GetCargoItemsAsync(CargoQueryDto query);
        Task<CargoDto> GetCargoByIdAsync(Guid id);
        Task<CargoDto> CreateCargoAsync(CreateCargoDto dto);
        Task<CargoDto> UpdateCargoAsync(Guid id, UpdateCargoDto dto);
        Task DeleteCargoAsync(Guid id);
        Task<PagedResultDto<CargoDto>> GetCargoForVoyageAsync(Guid voyageId, CargoQueryDto query);
        Task<AiRecommendationResultDto> GetAiRiskAssessmentAsync(Guid cargoId);
    }
}
