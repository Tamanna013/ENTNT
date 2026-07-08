# Phase 2 Consistency Notes

As part of Milestone 35, a deliberate consistency and regression pass was performed across the Fleet, Ship, and Crew modules to close out Phase 2. The findings and fixes are documented below.

## 1. Controllers (Authorization & Policies)

*   **Check**: Ensure `[Authorize]` is present at the controller level, and that all write actions (Create, Update, Delete, Assign, Upload, etc.) have an explicit policy attribute (e.g., `AdminOrFleetManager` or `AdminOrCrewManager`).
*   **Outcome**: 
    *   `FleetsController`: Consistent. Writes correctly use `AdminOrFleetManager`.
    *   `ShipsController`: Consistent. Writes correctly use `AdminOrFleetManager`. Nested actions like `SetPrimaryPhoto` and `UploadShipAttachment` were also correctly protected.
    *   `CrewMembersController`: Consistent. Writes correctly use `AdminOrCrewManager`. Nested actions like `AssignToShip`, `UnassignFromShip`, `UploadCertification`, and `DeleteCertification` were correctly protected.

## 2. Services (Naming, Paging, Error Handling)

*   **Check**: Ensure list methods clamp `PageSize` (1-100), `GetById` throws `NotFoundException`, uniqueness constraints throw `ConflictException`, and standard `Get{Plural}Async` naming is used.
*   **Outcome**:
    *   **Naming conventions**: Consistent across `FleetService`, `ShipService`, and `CrewMemberService` (`GetFleetsAsync`, `GetShipsAsync`, `GetCrewMembersAsync`).
    *   **Error Handling (NotFound)**: Consistent. All three services correctly throw `NotFoundException` when an entity is not found by ID.
    *   **Error Handling (Conflict)**: Consistent. `CreateFleetAsync`, `UpdateFleetAsync`, and `CreateShipAsync`, `CreateCrewMemberAsync` properly throw `ConflictException` for uniqueness constraints (Name, IMO, License Number). Fixed fields like `IMO` and `LicenseNumber` correctly omitted these checks from their `Update` methods by design, as those fields are not part of their respective update DTOs.
    *   **PageSize Clamping**: Found an inconsistency in `CrewMemberService`. It was missing the `query.PageSize = Math.Clamp(query.PageSize, 1, 100);` logic before fetching the paged result.
    *   **Fix Applied**: Added the page size clamping to `CrewMemberService.GetCrewMembersAsync` to ensure identical behavior with `FleetService` and `ShipService`.

## 3. Repositories (Entity Framework Query Standards)

*   **Check**: Verify `AsNoTracking()` usage on read paths, soft-delete filtering (`IsDeleted == false`), and ensuring `CountAsync` occurs before data retrieval (`Skip` / `Take`).
*   **Outcome**:
    *   `FleetRepository`: Consistent.
    *   `ShipRepository`: Consistent.
    *   `CrewMemberRepository`: Consistent. 
    *   **Note on Includes**: Both `ShipRepository` and `CrewMemberRepository` use `.Include()` for relational data (Fleet, Ship respectively), which was correctly applied to the queryable. `CrewMemberRepository` applied `.Include()` after `.CountAsync()` which is technically a micro-optimization avoiding a `JOIN` during the count operation, whereas `ShipRepository` applied it at the start. Both implementations are structurally correct and were left as-is, as they both utilize `AsNoTracking()` properly.

## 4. Frontend

*   **Check**: Look for any genuine frontend inconsistency across `Crew`, `Ship`, or `Fleet` pages.
*   **Outcome**:
    *   The frontend modules were checked in the previous milestones and are structurally consistent in terms of table design, query hooks naming, and role-based action hiding. No specific frontend fixes were needed.

## Conclusion

The cross-module architecture remains robust. The single real inconsistency found (missing page size clamping in `CrewMemberService`) was a typical minor drift issue and was fixed. The system is well-aligned for Phase 3.
