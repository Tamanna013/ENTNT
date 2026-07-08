# FleetMind Indexing Strategy

This document catalogs every database index across the FleetMind schema, detailing its purpose. We follow a strict "evidence-based indexing" approach: we only add indexes that are genuinely justified by existing query patterns, avoiding speculative indexing bloat.

## Current Indexes

### Identity & Access
* **`User.Email`**: Unique index. Enforces the business rule that user login emails must be globally unique.
* **`Role.Name`**: Unique index. Enforces unique role names.
* **`RefreshToken.TokenHash`**: Supports rapid O(1) lookups during token refresh flows.
* **`PasswordResetToken.TokenHash`**: Supports secure, fast lookups during the password reset flow.
* **`EmailVerificationToken.TokenHash`**: Supports fast lookups during email verification.

### Core Domain (Phase 1 & 2)
* **`Fleet.Name`**: Optimizes list lookups and dropdown population by fleet name.
* **`Ship.IMO`**: Unique index. Enforces the business rule for real-world globally-unique vessel identifiers.
* **`Ship.FleetId`**: Foreign key index. Optimizes the standard "list ships for this fleet" query.
* **`CrewMember.LicenseNumber`**: Unique index. Enforces globally unique maritime license numbers.
* **`CrewMember.ShipId`**: Foreign key index. Optimizes fetching a ship's current crew roster.

### Operational Domain (Phase 3)
* **`Voyage.VoyageNumber`**: Unique index. Enforces unique voyage numbers and supports direct lookups.
* **`Voyage (ShipId, DepartureDate)`**: Composite index. Supports the common "this ship's voyage history in chronological order" query used heavily by the Ship detail page.
* **`Voyage.Status`**: *(Added in Phase 3)* Optimizes list and dashboard queries that frequently filter active/completed voyages.
* **`Cargo.VoyageId`**: Foreign key index. Optimizes loading the cargo manifest for a given voyage.
* **`Cargo.Status`**: *(Added in Phase 3)* Optimizes cargo management list endpoints that filter by transit/delivered status.
* **`Cargo.Type`**: *(Added in Phase 3)* Optimizes cargo management list endpoints that filter by specific cargo characteristics (e.g. Hazardous).
* **`Container.ContainerNumber`**: Unique index. Enforces real-world unique container identifiers.
* **`Container.Status`**: *(Added in Phase 3)* Optimizes container list endpoints tracking container disposition.
* **`ContainerTrackingEvent (ContainerId, Timestamp)`**: Composite index. Supports fetching a container's event timeline history in precise chronological order.

### Infrastructure
* **`Attachment (EntityName, EntityId)`**: Composite index. Optimizes the generic polymorphic attachment lookup used to fetch files related to any arbitrary domain entity.

## Query Optimizer Observations (Before & After)

* **Voyage Status Filtering**:
  * *Before*: Queries filtering by `Status` (e.g. fetching only 'InTransit' voyages) fell back to Clustered Index Scans on the `Voyages` table, reading the entire table even if only a few voyages matched.
  * *After*: With the new `Voyage.Status` index, the query optimizer selects an Index Seek, dramatically reducing logical reads.
* **Cargo Status & Type**:
  * *Before*: Filtering the manifest by `Type = 'Hazardous'` or `Status = 'Pending'` caused a full Clustered Index Scan.
  * *After*: The query optimizer uses an Index Seek against the newly added `Cargo.Status` and `Cargo.Type` indexes.
* **Container Status**:
  * *Before*: `Container.Status` filters resulted in full table scans.
  * *After*: `Container.Status` index provides an Index Seek access path.

*(Note: In the current tiny local sample dataset, clustered index scans appear inexpensively fast. However, execution plans confirm the query shape has shifted to Index Seeks, ensuring the queries remain performant as the dataset scales.)*

## Deliberately Excluded Indexes
* **`CrewMember.Rank`**: Not currently filtered on by any list endpoint's common usage pattern. Adding an index now would merely add write overhead without benefiting any existing read query.
* **`Cargo.WeightKg` / `Cargo.DeclaredValue`**: While present in the schema, we do not currently offer a UI or API endpoint to filter/sort directly by cargo weight or value ranges, so they remain unindexed.
