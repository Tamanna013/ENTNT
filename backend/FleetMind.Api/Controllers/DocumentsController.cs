using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Documents;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FleetMind.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ICurrentUserService _currentUserService;

    public DocumentsController(IDocumentService documentService, ICurrentUserService currentUserService)
    {
        _documentService = documentService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<DocumentDto>>> GetDocuments([FromQuery] DocumentQueryDto query)
    {
        return Ok(await _documentService.GetDocumentsAsync(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentDto>> GetDocumentById(Guid id)
    {
        return Ok(await _documentService.GetDocumentByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrFleetManager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DocumentDto>> CreateDocument([FromForm] CreateDocumentDto dto, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Title = "A file must be uploaded" });
        }

        var result = await _documentService.CreateDocumentAsync(dto, file, _currentUserService.UserId ?? Guid.Empty);
        return CreatedAtAction(nameof(GetDocumentById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOrFleetManager")]
    public async Task<ActionResult<DocumentDto>> UpdateDocument(Guid id, [FromBody] UpdateDocumentDto dto)
    {
        return Ok(await _documentService.UpdateDocumentAsync(id, dto));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOrFleetManager")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        await _documentService.DeleteDocumentAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/versions")]
    [Authorize(Policy = "AdminOrFleetManager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DocumentVersionDto>> UploadNewVersion(Guid id, [FromForm] IFormFile file, [FromForm] string? changeNotes)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Title = "A file must be uploaded" });
        }

        var result = await _documentService.UploadNewVersionAsync(id, file, changeNotes, _currentUserService.UserId ?? Guid.Empty);
        // Returns 201 Created but the location header doesn't point to a specific version GET by ID as we only have the list
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("{id}/versions")]
    public async Task<ActionResult<List<DocumentVersionDto>>> GetVersions(Guid id)
    {
        return Ok(await _documentService.GetVersionsAsync(id));
    }

    [HttpGet("{id}/versions/{versionNumber}/download")]
    public async Task<IActionResult> DownloadVersion(Guid id, int versionNumber)
    {
        var (stream, contentType, fileName) = await _documentService.DownloadVersionAsync(id, versionNumber);
        return File(stream, contentType, fileName);
    }
}
