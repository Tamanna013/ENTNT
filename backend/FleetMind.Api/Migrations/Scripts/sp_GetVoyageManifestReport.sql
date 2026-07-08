CREATE OR ALTER PROCEDURE sp_GetVoyageManifestReport
    @VoyageId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.Id AS CargoId,
        c.Description,
        c.Type,
        c.Status,
        c.WeightKg,
        c.DeclaredValue,
        c.ConsigneeName,
        cont.ContainerNumber
    FROM 
        Cargo c
    LEFT JOIN 
        ContainerCargoItems cci ON cci.CargoId = c.Id
    LEFT JOIN 
        Containers cont ON cont.Id = cci.ContainerId AND cont.IsDeleted = 0
    WHERE 
        c.VoyageId = @VoyageId AND c.IsDeleted = 0;
END;
