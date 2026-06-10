-- =============================================================
-- Wishlist stored procedures
-- The API (CarWishlistService) calls these via EXEC. They were
-- missing from the database, so every toggle/fetch threw and the
-- wishlist never persisted. Re-runnable (CREATE OR ALTER).
-- =============================================================

-- -------------------------------------------------------------
-- Toggle a car in a user's wishlist (add if absent, remove if present).
-- Returns a single row: IsWishlisted (new state) + Message.
-- Shape must match DTOs.Cars.WishlistToggleResultDto.
-- -------------------------------------------------------------
CREATE OR ALTER PROCEDURE [dbo].[ToggleCarWishlist]
    @UserDetailID INT,
    @CarDetailID  INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.UserWishlist
               WHERE UserDetailID = @UserDetailID AND CarDetailID = @CarDetailID)
    BEGIN
        DELETE FROM dbo.UserWishlist
        WHERE UserDetailID = @UserDetailID AND CarDetailID = @CarDetailID;

        SELECT CAST(0 AS BIT) AS IsWishlisted, 'Removed from wishlist' AS Message;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.UserWishlist (UserDetailID, CarDetailID)
        VALUES (@UserDetailID, @CarDetailID);

        SELECT CAST(1 AS BIT) AS IsWishlisted, 'Added to wishlist' AS Message;
    END
END
GO

-- -------------------------------------------------------------
-- Return every car a user has wishlisted.
-- Column shape MUST match Models.CarFilterRaw (same as GetCars),
-- because the service maps the result set to that entity.
-- -------------------------------------------------------------
CREATE OR ALTER PROCEDURE [dbo].[GetWishlistCarsByUser]
    @UserDetailID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) OVER() AS TotalCount,
        cd.ID,
        cd.DealersID,
        cd.Brand,
        cd.Model,
        cd.Varient,
        cd.price              AS Price,
        cd.RegistrationNo,
        cd.RegistrationDate,
        cd.KMDriven,
        cd.Fuel,
        cd.Transmission,
        cd.NoOfOwner,
        cd.IsAccidental,
        cd.adjustableStaring  AS AdjustableStaring,
        cd.AlloyWheels,
        cd.AntiTheftSystem,
        cd.MusicSystem,
        cd.Aux,
        cd.bluetooth          AS Bluetooth,
        cd.InsuranceType,
        cd.PowerStaring,
        cd.PowerWindow,
        cd.RegistrationState,
        cd.ServiceHistory,
        cd.EnginCapacity,
        cd.AirCondition,
        cd.AirBag,
        cd.ABS,
        cd.EBD,
        cd.BSD,
        cd.HillHold,
        cd.BodyType,
        cd.IsNegotiable,
        cd.ReverseCamera,
        cd.Sunroof,
        CASE WHEN cd.carlocation IS NOT NULL THEN cd.carlocation.Lat  ELSE NULL END AS CarLat,
        CASE WHEN cd.carlocation IS NOT NULL THEN cd.carlocation.Long ELSE NULL END AS CarLon,
        cd.CreatedDate,
        ci.Image1, ci.Image2, ci.Image3, ci.Image4, ci.Image5,
        ci.Image6, ci.Image7, ci.Image8, ci.Image9, ci.Image10,
        ci.Image11, ci.Image12, ci.Image13, ci.Image14, ci.Image15,
        ci.Image16, ci.Image17, ci.Image18, ci.Image19, ci.Image20,
        CAST(1 AS BIT)    AS IsWishlisted,
        CAST(NULL AS FLOAT) AS DistanceKm
    FROM dbo.UserWishlist uw
    INNER JOIN CarDetail cd ON cd.ID = uw.CarDetailID
    LEFT  JOIN CarImage  ci ON ci.CarDetailID = cd.ID
    WHERE uw.UserDetailID = @UserDetailID
    ORDER BY cd.ID DESC;
END
GO
