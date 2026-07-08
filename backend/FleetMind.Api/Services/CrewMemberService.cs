using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Crew;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Services
{
    public class CrewMemberService : ICrewMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAttachmentService _attachmentService;

        public CrewMemberService(IUnitOfWork unitOfWork, IMapper mapper, IAttachmentService attachmentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _attachmentService = attachmentService;
        }

        public async Task<PagedResultDto<CrewMemberDto>> GetCrewMembersAsync(CrewMemberQueryDto query)
        {
            query.PageSize = Math.Clamp(query.PageSize, 1, 100);
            var (items, totalCount) = await _unitOfWork.CrewMembers.GetPagedAsync(query);
            
            return new PagedResultDto<CrewMemberDto>
            {
                Items = _mapper.Map<List<CrewMemberDto>>(items),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<CrewMemberDto> GetCrewMemberByIdAsync(Guid id)
        {
            var crewMember = await _unitOfWork.CrewMembers.GetByIdWithShipAsync(id);
            if (crewMember == null || crewMember.IsDeleted)
            {
                throw new NotFoundException($"Crew member with ID {id} not found.");
            }

            return _mapper.Map<CrewMemberDto>(crewMember);
        }

        public async Task<CrewMemberDto> CreateCrewMemberAsync(CreateCrewMemberDto dto)
        {
            if (await _unitOfWork.CrewMembers.ExistsByLicenseNumberAsync(dto.LicenseNumber))
            {
                throw new ConflictException("A crew member with this license number already exists.");
            }

            var crewMember = _mapper.Map<CrewMember>(dto);
            crewMember.ShipId = null;
            crewMember.Status = CrewStatus.Unassigned;

            await _unitOfWork.CrewMembers.AddAsync(crewMember);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CrewMemberDto>(crewMember);
        }

        public async Task<CrewMemberDto> UpdateCrewMemberAsync(Guid id, UpdateCrewMemberDto dto)
        {
            var crewMember = await _unitOfWork.CrewMembers.GetByIdWithShipAsync(id);
            if (crewMember == null || crewMember.IsDeleted)
            {
                throw new NotFoundException($"Crew member with ID {id} not found.");
            }

            crewMember.FirstName = dto.FirstName;
            crewMember.LastName = dto.LastName;
            crewMember.Rank = dto.Rank;
            crewMember.Status = dto.Status;
            crewMember.Nationality = dto.Nationality;
            crewMember.ContactEmail = dto.ContactEmail;
            crewMember.ContactPhone = dto.ContactPhone;

            _unitOfWork.CrewMembers.Update(crewMember);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CrewMemberDto>(crewMember);
        }

        public async Task DeactivateCrewMemberAsync(Guid id)
        {
            var crewMember = await _unitOfWork.CrewMembers.GetByIdAsync(id);
            if (crewMember == null || crewMember.IsDeleted)
            {
                throw new NotFoundException($"Crew member with ID {id} not found.");
            }

            // Standard soft-delete handled via IsDeleted (possibly configured via interceptor or manual)
            // The user requested soft delete, IGenericRepository doesn't natively do soft delete automatically unless done in DbContext, 
            // wait, we implemented soft delete logic in UnitOfWork or GenericRepository earlier.
            crewMember.IsDeleted = true;
            _unitOfWork.CrewMembers.Update(crewMember);
            
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<CrewMemberDto> AssignToShipAsync(Guid id, Guid shipId)
        {
            var crewMember = await _unitOfWork.CrewMembers.GetByIdWithShipAsync(id);
            if (crewMember == null || crewMember.IsDeleted)
            {
                throw new NotFoundException($"Crew member with ID {id} not found.");
            }

            // Ship existence was already validated by AssignToShipDtoValidator
            crewMember.ShipId = shipId;
            crewMember.Status = CrewStatus.Active;

            _unitOfWork.CrewMembers.Update(crewMember);
            await _unitOfWork.SaveChangesAsync();

            // Re-fetch to get the included Ship reference mapped properly
            crewMember = await _unitOfWork.CrewMembers.GetByIdWithShipAsync(id);

            return _mapper.Map<CrewMemberDto>(crewMember);
        }

        public async Task<CrewMemberDto> UnassignFromShipAsync(Guid id)
        {
            var crewMember = await _unitOfWork.CrewMembers.GetByIdWithShipAsync(id);
            if (crewMember == null || crewMember.IsDeleted)
            {
                throw new NotFoundException($"Crew member with ID {id} not found.");
            }

            crewMember.ShipId = null;
            crewMember.Status = CrewStatus.Unassigned;

            _unitOfWork.CrewMembers.Update(crewMember);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CrewMemberDto>(crewMember);
        }

        public async Task<PagedResultDto<CrewMemberDto>> GetCrewForShipAsync(Guid shipId, CrewMemberQueryDto query)
        {
            query.ShipId = shipId;
            return await GetCrewMembersAsync(query);
        }

        public async Task<List<CrewCertificationDto>> GetCertificationsAsync(Guid crewMemberId)
        {
            var crewMember = await _unitOfWork.CrewMembers.GetByIdAsync(crewMemberId);
            if (crewMember == null || crewMember.IsDeleted)
                throw new NotFoundException($"Crew member with ID {crewMemberId} not found.");

            var certifications = await _unitOfWork.Repository<CrewCertification>()
                .FindAsync(c => c.CrewMemberId == crewMemberId && !c.IsDeleted);

            var dtos = new List<CrewCertificationDto>();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            foreach (var cert in certifications)
            {
                var attachment = await _unitOfWork.Repository<Attachment>().GetByIdAsync(cert.AttachmentId);
                dtos.Add(new CrewCertificationDto
                {
                    Id = cert.Id,
                    CrewMemberId = cert.CrewMemberId,
                    CertificationName = cert.CertificationName,
                    ExpiryDate = cert.ExpiryDate,
                    IsExpired = cert.ExpiryDate < today,
                    DownloadUrl = $"/api/v1/attachments/{cert.AttachmentId}/download",
                    FileName = attachment?.FileName ?? string.Empty,
                    CreatedAt = cert.CreatedAt
                });
            }

            return dtos;
        }

        public async Task<CrewCertificationDto> UploadCertificationAsync(Guid crewMemberId, Microsoft.AspNetCore.Http.IFormFile file, string certificationName, DateOnly expiryDate, Guid uploadedByUserId)
        {
            var crewMember = await _unitOfWork.CrewMembers.GetByIdAsync(crewMemberId);
            if (crewMember == null || crewMember.IsDeleted)
                throw new NotFoundException($"Crew member with ID {crewMemberId} not found.");

            var attachmentDto = await _attachmentService.UploadAsync(file, AttachmentEntityType.Crew, crewMemberId, uploadedByUserId);

            var cert = new CrewCertification
            {
                CrewMemberId = crewMemberId,
                AttachmentId = attachmentDto.Id,
                CertificationName = certificationName,
                ExpiryDate = expiryDate
            };

            await _unitOfWork.Repository<CrewCertification>().AddAsync(cert);
            await _unitOfWork.SaveChangesAsync();

            return new CrewCertificationDto
            {
                Id = cert.Id,
                CrewMemberId = cert.CrewMemberId,
                CertificationName = cert.CertificationName,
                ExpiryDate = cert.ExpiryDate,
                IsExpired = cert.ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow),
                DownloadUrl = $"/api/v1/attachments/{cert.AttachmentId}/download",
                FileName = attachmentDto.FileName,
                CreatedAt = cert.CreatedAt
            };
        }

        public async Task DeleteCertificationAsync(Guid crewMemberId, Guid certificationId)
        {
            var cert = await _unitOfWork.Repository<CrewCertification>().GetByIdAsync(certificationId);
            if (cert == null || cert.IsDeleted)
                throw new NotFoundException($"Certification with ID {certificationId} not found.");

            if (cert.CrewMemberId != crewMemberId)
                throw new AppValidationException("Certification does not belong to the specified crew member.");

            await _attachmentService.DeleteAsync(cert.AttachmentId);
            
            _unitOfWork.Repository<CrewCertification>().Remove(cert);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
