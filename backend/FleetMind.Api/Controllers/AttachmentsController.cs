using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using FleetMind.Api.DTOs.Attachments;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetMind.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;
        private readonly ICurrentUserService _currentUserService;

        public AttachmentsController(IAttachmentService attachmentService, ICurrentUserService currentUserService)
        {
            _attachmentService = attachmentService;
            _currentUserService = currentUserService;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<AttachmentDto>> Upload([FromForm] UploadAttachmentRequestDto request)
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var result = await _attachmentService.UploadAsync(request.File, request.EntityName, request.EntityId, userId);
            return CreatedAtAction(nameof(Download), new { version = "1.0", id = result.Id }, result);
        }

        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> Download(Guid id)
        {
            var (stream, contentType, fileName) = await _attachmentService.DownloadAsync(id);
            return File(stream, contentType, fileName);
        }

        [HttpGet]
        public async Task<ActionResult<List<AttachmentDto>>> GetByEntity([FromQuery] string entityName, [FromQuery] Guid entityId)
        {
            var result = await _attachmentService.GetByEntityAsync(entityName, entityId);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _attachmentService.DeleteAsync(id);
            return NoContent();
        }
    }
}
