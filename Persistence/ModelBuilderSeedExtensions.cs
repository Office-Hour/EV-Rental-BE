using Domain.Entities.StationManagement;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Persistence;
public static class ModelBuilderSeedExtensions
{
    public static void SeedInitialData(this ModelBuilder modelBuilder)
    {
        // ======= Fixed timestamps for HasData =======
        var T0 = new DateTime(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

        // ======= Deterministic IDs (change if you like) =======
        // Stations
        var stHcmc = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var stHanoi = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var stDanang = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Vehicles
        var vTesla3 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var vTeslaY = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var vLeaf = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var vKiaEV6 = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        // VehicleAtStation (history rows)
        var vasTesla3 = Guid.Parse("01010101-0101-0101-0101-010101010101");
        var vasTeslaY = Guid.Parse("02020202-0202-0202-0202-020202020202");
        var vasLeaf = Guid.Parse("03030303-0303-0303-0303-030303030303");
        var vasKiaEV6 = Guid.Parse("04040404-0404-0404-0404-040404040404");

        // Pricing rows
        var pTesla3_v1 = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var pTeslaY_v1 = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var pLeaf_v1 = Guid.Parse("10000000-0000-0000-0000-000000000003");
        var pKiaEV6_v1 = Guid.Parse("10000000-0000-0000-0000-000000000004");

        // ======= Stations =======
        modelBuilder.Entity<Station>().HasData(
            new Station
            {
                StationId = stHcmc,
                Name = "HCMC Central Station",
                Address = "01 Nguyen Hue, District 1, Ho Chi Minh City",
                Latitude = 10.776889,
                Longitude = 106.700806
            },
            new Station
            {
                StationId = stHanoi,
                Name = "Hanoi Old Quarter Station",
                Address = "10 Hang Dao, Hoan Kiem, Hanoi",
                Latitude = 21.033781,
                Longitude = 105.850176
            },
            new Station
            {
                StationId = stDanang,
                Name = "Da Nang Riverside Station",
                Address = "02 Bach Dang, Hai Chau, Da Nang",
                Latitude = 16.067789,
                Longitude = 108.220739
            }
        );

        // ======= Vehicles =======
        modelBuilder.Entity<Vehicle>().HasData(
            new Vehicle
            {
                VehicleId = vTesla3,
                Make = "Tesla",
                Model = "Model 3",
                ModelYear = 2024,
                BatteryCapacityKwh = 57.0,
                RangeKm = 438.0
            },
            new Vehicle
            {
                VehicleId = vTeslaY,
                Make = "Tesla",
                Model = "Model Y",
                ModelYear = 2024,
                BatteryCapacityKwh = 75.0,
                RangeKm = 455.0
            },
            new Vehicle
            {
                VehicleId = vLeaf,
                Make = "Nissan",
                Model = "Leaf",
                ModelYear = 2023,
                BatteryCapacityKwh = 40.0,
                RangeKm = 270.0
            },
            new Vehicle
            {
                VehicleId = vKiaEV6,
                Make = "Kia",
                Model = "EV6",
                ModelYear = 2024,
                BatteryCapacityKwh = 77.4,
                RangeKm = 500.0
            }
        );

        // ======= Initial vehicle placements (history) =======
        modelBuilder.Entity<VehicleAtStation>().HasData(
            new VehicleAtStation
            {
                VehicleAtStationId = vasTesla3,
                VehicleId = vTesla3,
                StationId = stHcmc,
                StartTime = T0,
                EndTime = null,
                CurrentBatteryCapacityKwh = 57.0,
                Status = VehicleAtStationStatus.Available
            },
            new VehicleAtStation
            {
                VehicleAtStationId = vasTeslaY,
                VehicleId = vTeslaY,
                StationId = stHanoi,
                StartTime = T0,
                EndTime = null,
                CurrentBatteryCapacityKwh = 75.0,
                Status = VehicleAtStationStatus.Available
            },
            new VehicleAtStation
            {
                VehicleAtStationId = vasLeaf,
                VehicleId = vLeaf,
                StationId = stDanang,
                StartTime = T0,
                EndTime = null,
                CurrentBatteryCapacityKwh = 40.0,
                Status = VehicleAtStationStatus.Available
            },
            new VehicleAtStation
            {
                VehicleAtStationId = vasKiaEV6,
                VehicleId = vKiaEV6,
                StationId = stHcmc,
                StartTime = T0,
                EndTime = null,
                CurrentBatteryCapacityKwh = 77.4,
                Status = VehicleAtStationStatus.Available
            }
        );

        // ======= Pricing (v1 per vehicle) =======
        modelBuilder.Entity<Pricing>().HasData(
            new Pricing
            {
                PricingId = pTesla3_v1,
                VehicleId = vTesla3,
                PricePerHour = 180_000m,   // VND
                PricePerDay = 1_500_000m, // VND
                EffectiveFrom = T0,
                EffectiveTo = null
            },
            new Pricing
            {
                PricingId = pTeslaY_v1,
                VehicleId = vTeslaY,
                PricePerHour = 220_000m,
                PricePerDay = 1_800_000m,
                EffectiveFrom = T0,
                EffectiveTo = null
            },
            new Pricing
            {
                PricingId = pLeaf_v1,
                VehicleId = vLeaf,
                PricePerHour = 120_000m,
                PricePerDay = 950_000m,
                EffectiveFrom = T0,
                EffectiveTo = null
            },
            new Pricing
            {
                PricingId = pKiaEV6_v1,
                VehicleId = vKiaEV6,
                PricePerHour = 200_000m,
                PricePerDay = 1_650_000m,
                EffectiveFrom = T0,
                EffectiveTo = null
            }
        );
    }
}