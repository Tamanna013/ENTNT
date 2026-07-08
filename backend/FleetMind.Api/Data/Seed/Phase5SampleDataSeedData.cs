using System;
using System.Collections.Generic;
using FleetMind.Api.Models;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Data.Seed
{
    public static class Phase5SampleDataSeedData
    {
        public static List<Incident> GetSampleIncidents(List<Guid> shipIds, List<Guid> voyageIds, Guid reporterId)
        {
            var incidents = new List<Incident>();
            if (shipIds.Count == 0) return incidents;

            incidents.Add(new Incident
            {
                Id = Guid.NewGuid(),
                ShipId = shipIds[0],
                VoyageId = voyageIds.Count > 0 ? voyageIds[0] : null,
                Title = "Engine Power Fluctuation",
                Description = "Starboard main engine experienced sudden 15% power drop during heavy weather. Power restored after 10 minutes, but underlying cause requires investigation.",
                Severity = IncidentSeverity.High,
                Status = IncidentStatus.UnderInvestigation,
                ReportedByUserId = reporterId,
                OccurredAt = DateTime.UtcNow.AddDays(-2)
            });

            incidents.Add(new Incident
            {
                Id = Guid.NewGuid(),
                ShipId = shipIds.Count > 1 ? shipIds[1] : shipIds[0],
                Title = "Minor Oil Spill on Deck",
                Description = "Hydraulic line burst on deck crane during cargo operations. Approximately 5 liters of hydraulic fluid spilled. Contained immediately, no environmental release.",
                Severity = IncidentSeverity.Medium,
                Status = IncidentStatus.Resolved,
                ReportedByUserId = reporterId,
                OccurredAt = DateTime.UtcNow.AddDays(-10)
            });

            incidents.Add(new Incident
            {
                Id = Guid.NewGuid(),
                ShipId = shipIds.Count > 2 ? shipIds[2] : shipIds[0],
                Title = "Communication System Failure",
                Description = "Primary VSAT system lost connection for 6 hours. Backup Iridium system was used for critical communications. Resolved by remote reboot from vendor.",
                Severity = IncidentSeverity.Low,
                Status = IncidentStatus.Closed,
                ReportedByUserId = reporterId,
                OccurredAt = DateTime.UtcNow.AddDays(-30)
            });

            incidents.Add(new Incident
            {
                Id = Guid.NewGuid(),
                ShipId = shipIds[0],
                Title = "Hull Breach - Forward Cargo Hold",
                Description = "Ship struck a submerged object during transit, causing a minor hull breach in the forward cargo hold. Water ingress is currently contained by bilge pumps, but immediate dry docking is required.",
                Severity = IncidentSeverity.Critical,
                Status = IncidentStatus.Reported, // CRITICAL AND OPEN
                ReportedByUserId = reporterId,
                OccurredAt = DateTime.UtcNow.AddHours(-1) // VERY RECENT
            });

            return incidents;
        }

        public static (List<Document>, List<DocumentVersion>, List<Attachment>) GetSampleDocuments(Guid uploaderId, List<Guid> shipIds)
        {
            var documents = new List<Document>();
            var versions = new List<DocumentVersion>();
            var attachments = new List<Attachment>();

            if (shipIds.Count == 0) return (documents, versions, attachments);

            // Document 1: Fleet Safety Manual (2 versions)
            var doc1Id = Guid.NewGuid();
            documents.Add(new Document
            {
                Id = doc1Id,
                Title = "Fleet Safety Operations Manual",
                Category = "Safety",
                Description = "Standard operating procedures for safety across all vessels.",
                CurrentVersionNumber = 2
            });

            var doc1Att1Id = Guid.NewGuid();
            attachments.Add(new Attachment
            {
                Id = doc1Att1Id,
                EntityName = AttachmentEntityType.DocumentVersion,
                // EntityId will be updated later
                FileName = "Safety_Manual_v1.pdf",
                StoredFileName = "seed_doc1_v1.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 1048576,
                UploadedByUserId = uploaderId
            });

            var doc1Ver1Id = Guid.NewGuid();
            versions.Add(new DocumentVersion
            {
                Id = doc1Ver1Id,
                DocumentId = doc1Id,
                VersionNumber = 1,
                AttachmentId = doc1Att1Id,
                UploadedByUserId = uploaderId,
                ChangeNotes = "Initial release"
            });
            attachments[attachments.Count - 1].EntityId = doc1Ver1Id; // Set EntityId to VersionId

            var doc1Att2Id = Guid.NewGuid();
            attachments.Add(new Attachment
            {
                Id = doc1Att2Id,
                EntityName = AttachmentEntityType.DocumentVersion,
                FileName = "Safety_Manual_v2.pdf",
                StoredFileName = "seed_doc1_v2.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 1150000,
                UploadedByUserId = uploaderId
            });

            var doc1Ver2Id = Guid.NewGuid();
            versions.Add(new DocumentVersion
            {
                Id = doc1Ver2Id,
                DocumentId = doc1Id,
                VersionNumber = 2,
                AttachmentId = doc1Att2Id,
                UploadedByUserId = uploaderId,
                ChangeNotes = "Added new confined space entry protocols."
            });
            attachments[attachments.Count - 1].EntityId = doc1Ver2Id;

            // Document 2: Ship Registration Certificate (1 version)
            var doc2Id = Guid.NewGuid();
            documents.Add(new Document
            {
                Id = doc2Id,
                Title = "Ship Registration Certificate",
                Category = "Legal",
                Description = "Official registration document for the vessel.",
                EntityName = "Ship",
                EntityId = shipIds[0],
                CurrentVersionNumber = 1
            });

            var doc2Att1Id = Guid.NewGuid();
            attachments.Add(new Attachment
            {
                Id = doc2Att1Id,
                EntityName = AttachmentEntityType.DocumentVersion,
                FileName = "ShipReg_001.pdf",
                StoredFileName = "seed_doc2_v1.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 500000,
                UploadedByUserId = uploaderId
            });

            var doc2Ver1Id = Guid.NewGuid();
            versions.Add(new DocumentVersion
            {
                Id = doc2Ver1Id,
                DocumentId = doc2Id,
                VersionNumber = 1,
                AttachmentId = doc2Att1Id,
                UploadedByUserId = uploaderId,
                ChangeNotes = "Initial upload of registration."
            });
            attachments[attachments.Count - 1].EntityId = doc2Ver1Id;

            return (documents, versions, attachments);
        }
    }
}
