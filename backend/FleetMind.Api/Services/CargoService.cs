using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Cargo;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.Services.Ai;
using FleetMind.Api.Services.Ai.PromptBuilders;
using System.Linq;

namespace FleetMind.Api.Services
{
    public class CargoService : ICargoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAiProvider _aiProvider;
        
        // This threshold acts as an illustrative baseline for a cargo weight sanity check.
        // A real implementation might instead derive this from the ship's actual capacity.
        private const decimal VOYAGE_WEIGHT_WARNING_THRESHOLD_KG = 50000m;

        public CargoService(IUnitOfWork unitOfWork, IMapper mapper, IAiProvider aiProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _aiProvider = aiProvider;
        }

        public async Task<PagedResultDto<CargoDto>> GetCargoItemsAsync(CargoQueryDto query)
        {
            var (items, totalCount) = await _unitOfWork.Cargo.GetPagedAsync(query);
            
            var dtos = _mapper.Map<List<CargoDto>>(items);
            
            return new PagedResultDto<CargoDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<CargoDto> GetCargoByIdAsync(Guid id)
        {
            var cargo = await _unitOfWork.Cargo.GetByIdAsync(id) 
                ?? throw new NotFoundException($"Cargo item with ID {id} not found.");
            
            // To flatten the VoyageNumber
            var voyage = await _unitOfWork.Voyages.GetByIdAsync(cargo.VoyageId);
            if (voyage != null)
            {
                cargo.Voyage = voyage;
            }

            return _mapper.Map<CargoDto>(cargo);
        }

        public async Task<CargoDto> CreateCargoAsync(CreateCargoDto dto)
        {
            var cargo = _mapper.Map<Cargo>(dto);
            cargo.Status = CargoStatus.Pending;

            await _unitOfWork.Cargo.AddAsync(cargo);
            await _unitOfWork.SaveChangesAsync();

            // Refetch to include navigation property for mapping
            var voyage = await _unitOfWork.Voyages.GetByIdAsync(cargo.VoyageId);
            if (voyage != null)
            {
                cargo.Voyage = voyage;
            }

            var resultDto = _mapper.Map<CargoDto>(cargo);

            // Post-creation soft warning check
            var cumulativeWeight = await _unitOfWork.Cargo.GetTotalWeightForVoyageAsync(dto.VoyageId);
            if (cumulativeWeight > VOYAGE_WEIGHT_WARNING_THRESHOLD_KG)
            {
                resultDto.Warnings.Add($"This voyage's total declared cargo weight now exceeds {VOYAGE_WEIGHT_WARNING_THRESHOLD_KG} kg.");
            }

            return resultDto;
        }

        public async Task<CargoDto> UpdateCargoAsync(Guid id, UpdateCargoDto dto)
        {
            var cargo = await _unitOfWork.Cargo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Cargo item with ID {id} not found.");

            // Conditional validation logic for HazardNotes 
            // We check this in the service because UpdateCargoDto doesn't contain the 'Type',
            // treating cargo type as immutable post-creation. Since the entity is loaded here,
            // we have the context needed to validate.
            if (cargo.Type == CargoType.Hazardous && string.IsNullOrWhiteSpace(dto.HazardNotes))
            {
                throw new AppValidationException("Hazard notes are required for hazardous cargo.");
            }

            cargo.Description = dto.Description;
            cargo.Status = dto.Status;
            cargo.WeightKg = dto.WeightKg;
            cargo.DeclaredValue = dto.DeclaredValue;
            cargo.ConsigneeName = dto.ConsigneeName;
            cargo.HazardNotes = dto.HazardNotes;

            _unitOfWork.Cargo.Update(cargo);
            await _unitOfWork.SaveChangesAsync();

            var voyage = await _unitOfWork.Voyages.GetByIdAsync(cargo.VoyageId);
            if (voyage != null)
            {
                cargo.Voyage = voyage;
            }

            return _mapper.Map<CargoDto>(cargo);
        }

        public async Task DeleteCargoAsync(Guid id)
        {
            var cargo = await _unitOfWork.Cargo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Cargo item with ID {id} not found.");

            cargo.IsDeleted = true;
            _unitOfWork.Cargo.Update(cargo);
            await _unitOfWork.SaveChangesAsync();
        }

        public Task<PagedResultDto<CargoDto>> GetCargoForVoyageAsync(Guid voyageId, CargoQueryDto query)
        {
            query.VoyageId = voyageId;
            return GetCargoItemsAsync(query);
        }

        public async Task<AiRecommendationResultDto> GetAiRiskAssessmentAsync(Guid cargoId)
        {
            const string disclaimer = "These observations are informational only and do NOT constitute a safety clearance, regulatory compliance determination, or substitute for qualified hazardous materials handling procedures. Always consult official safety documentation and qualified personnel for any actual handling decisions.";
            
            var cargo = await GetCargoByIdAsync(cargoId);
            
            if (!_aiProvider.IsAvailable)
            {
                return new AiRecommendationResultDto
                {
                    IsAvailable = false,
                    Recommendations = new List<string>(),
                    GeneratedAt = DateTime.UtcNow,
                    Disclaimer = disclaimer
                };
            }

            var builder = new CargoRiskAnalysisPromptBuilder();
            
            var voyage = await _unitOfWork.Voyages.GetByIdAsync(cargo.VoyageId);
            var voyageDto = voyage != null ? _mapper.Map<DTOs.Voyages.VoyageDto>(voyage) : null;

            var result = await _aiProvider.CompleteAsync(new Common.AiCompletionRequest
            {
                SystemPrompt = builder.BuildSystemPrompt(),
                UserPrompt = builder.BuildUserPrompt(cargo, voyageDto)
            });

            if (result.IsSuccess)
            {
                var recommendations = result.Content
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.TrimStart(' ', '-', '*', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.'))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                return new AiRecommendationResultDto
                {
                    IsAvailable = true,
                    Recommendations = recommendations,
                    GeneratedAt = DateTime.UtcNow,
                    Disclaimer = disclaimer
                };
            }

            return new AiRecommendationResultDto
            {
                IsAvailable = false,
                Recommendations = new List<string>(),
                GeneratedAt = DateTime.UtcNow,
                Disclaimer = disclaimer
            };
        }
    }
}
