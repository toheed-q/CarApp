ALTER PROCEDURE [dbo].[GetCars]
(
    @ByBrand          VARCHAR(100)  = NULL,
    @ByModel          VARCHAR(100)  = NULL,
    @BySearch         VARCHAR(100)  = NULL,
    @ByFuel           VARCHAR(50)   = NULL,
    @ByTransmission   VARCHAR(50)   = NULL,
    @ByOwners         INT           = 0,
    @ByPriceMoreThen  INT           = 0,
    @ByPriceLessThen  INT           = 0,
    @ByDrivenMoreThen INT           = 0,
    @ByDrivenLessThen INT           = 0,
    @ByAge            INT           = 0,
    @ByDealersID      INT           = 0,
    @ByIsActive       INT           = 0,
    @UserDetailID     INT           = 0,
    @Page             INT           = 1,
    @PageSize         INT           = 10,
    @SortBy           VARCHAR(50)   = 'price',
    @SortDir          VARCHAR(4)    = 'asc',
    @BuyerLat         FLOAT         = NULL,
    @BuyerLon         FLOAT         = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset       INT       = (@Page - 1) * @PageSize;
    DECLARE @BuyerLocation GEOGRAPHY = NULL;

    IF @BuyerLat IS NOT NULL AND @BuyerLon IS NOT NULL
        SET @BuyerLocation = geography::Point(@BuyerLat, @BuyerLon, 4326);

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
        CASE WHEN cd.carlocation IS NOT NULL THEN cd.carlocation.Lat  ELSE NULL END AS CarLat,
        CASE WHEN cd.carlocation IS NOT NULL THEN cd.carlocation.Long ELSE NULL END AS CarLon,
        ci.Image1, ci.Image2, ci.Image3, ci.Image4, ci.Image5,
        ci.Image6, ci.Image7, ci.Image8, ci.Image9, ci.Image10,
        ci.Image11, ci.Image12, ci.Image13, ci.Image14, ci.Image15,
        ci.Image16, ci.Image17, ci.Image18, ci.Image19, ci.Image20,
        CASE WHEN uw.CarDetailID IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsWishlisted,
        CASE
            WHEN @BuyerLocation IS NOT NULL AND cd.carlocation IS NOT NULL
            THEN cd.carlocation.STDistance(@BuyerLocation) / 1000.0
            ELSE NULL
        END AS DistanceKm
    FROM CarDetail cd
    LEFT JOIN CarImage ci ON ci.CarDetailID = cd.ID
    LEFT JOIN UserWishlist uw ON uw.CarDetailID = cd.ID AND uw.UserDetailID = @UserDetailID
    WHERE 1 = 1
        AND (@ByBrand         IS NULL OR cd.Brand        = @ByBrand)
        AND (@ByModel         IS NULL OR cd.Model        LIKE '%' + @ByModel + '%')
        AND (@BySearch        IS NULL OR cd.Brand        LIKE '%' + @BySearch + '%' OR cd.Model LIKE '%' + @BySearch + '%')
        AND (@ByFuel          IS NULL OR cd.Fuel         = @ByFuel)
        AND (@ByTransmission  IS NULL OR cd.Transmission = @ByTransmission)
        AND (@ByOwners        = 0     OR cd.NoOfOwner    = @ByOwners)
        AND (@ByPriceMoreThen = 0     OR cd.price        > @ByPriceMoreThen)
        AND (@ByPriceLessThen = 0     OR cd.price        < @ByPriceLessThen)
        AND (@ByDrivenMoreThen= 0     OR cd.KMDriven     > @ByDrivenMoreThen)
        AND (@ByDrivenLessThen= 0     OR cd.KMDriven     < @ByDrivenLessThen)
        AND (@ByAge           = 0     OR DATEDIFF(YEAR, cd.RegistrationDate, GETDATE()) <= @ByAge)
        AND (@ByDealersID     = 0     OR cd.DealersID    = @ByDealersID)
    ORDER BY
        CASE WHEN @SortBy = 'price'    AND @SortDir = 'asc'  THEN cd.price            END ASC,
        CASE WHEN @SortBy = 'price'    AND @SortDir = 'desc' THEN cd.price            END DESC,
        CASE WHEN @SortBy = 'km'       AND @SortDir = 'asc'  THEN cd.KMDriven         END ASC,
        CASE WHEN @SortBy = 'km'       AND @SortDir = 'desc' THEN cd.KMDriven         END DESC,
        CASE WHEN @SortBy = 'date'     AND @SortDir = 'asc'  THEN cd.RegistrationDate END ASC,
        CASE WHEN @SortBy = 'date'     AND @SortDir = 'desc' THEN cd.RegistrationDate END DESC,
        CASE WHEN @SortBy = 'distance' AND @BuyerLocation IS NOT NULL AND cd.carlocation IS NOT NULL THEN 0 ELSE 1 END ASC,
        CASE WHEN @SortBy = 'distance' AND @BuyerLocation IS NOT NULL AND cd.carlocation IS NOT NULL
             THEN cd.carlocation.STDistance(@BuyerLocation)
        END ASC,
        cd.ID ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO
