using System;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Fuel;

namespace FleetMind.Api.Services
{
    public interface IFuelLogService
    {
        Task<PagedResultDto<FuelLogDto>> GetFuelLogsAsync(FuelLogQueryDto query);
        Task<PagedResultDto<FuelLogDto>> GetFuelLogsForShipAsync(Guid shipId, FuelLogQueryDto query);
        Task<FuelLogDto> GetFuelLogByIdAsync(Guid id);
        Task<FuelLogDto> CreateFuelLogAsync(CreateFuelLogDto dto);
        Task<FuelLogDto> UpdateFuelLogAsync(Guid id, UpdateFuelLogDto dto);
        Task DeleteFuelLogAsync(Guid id);
    }
}
