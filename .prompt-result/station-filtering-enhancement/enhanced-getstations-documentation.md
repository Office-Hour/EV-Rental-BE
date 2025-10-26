# Enhanced GetStations Endpoint Documentation

## Overview
The `GetStations` endpoint has been enhanced to support filtering capabilities from `FilterStationQueryHandler`, allowing users to search and filter stations by name and address while maintaining pagination.

## Changes Made

### 1. Updated `ViewStationRequest.cs`
Added optional filter parameters:
- `Name` (string?, optional) - Filter stations by name (case-insensitive, partial match)
- `Address` (string?, optional) - Filter stations by address (case-insensitive, partial match)

### 2. Updated `StationsController.GetStations`
- Replaced `ViewStationQuery` with `FilterStationQuery`
- Added filtering capability by name and address
- Enhanced documentation with remarks about filtering
- Capacity field now shows current number of vehicles at each station (calculated from `VehicleAtStation` where `EndTime == null`)

## API Endpoint

**Route**: `GET /api/stations`

**Authorization**: Public (AllowAnonymous)

## Request Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| Name | string | No | null | Filter stations by name (case-insensitive, contains) |
| Address | string | No | null | Filter stations by address (case-insensitive, contains) |
| Page | int | No | 1 | Page number for pagination |
| PageSize | int | No | 20 | Number of items per page |

## Response

Returns `ApiResponse<PagedResult<StationDto>>`:

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "name": "Station Name",
        "address": "Station Address",
        "capacity": 15
      }
    ],
    "total": 50,
    "page": 1,
    "pageSize": 20
  },
  "message": "Stations retrieved successfully"
}
```

### StationDto Fields:
- `id` - Unique identifier of the station
- `name` - Name of the station
- `address` - Physical address of the station
- `capacity` - Current number of vehicles at the station (calculated in real-time)

## Usage Examples

### 1. Get all stations (default pagination)
```
GET /api/stations
```

### 2. Get all stations with custom page size
```
GET /api/stations?page=1&pageSize=50
```

### 3. Filter stations by name
```
GET /api/stations?name=central
```
Returns all stations containing "central" in their name (case-insensitive).

### 4. Filter stations by address
```
GET /api/stations?address=hanoi
```
Returns all stations with "hanoi" in their address (case-insensitive).

### 5. Combine name and address filters
```
GET /api/stations?name=central&address=hanoi
```
Returns stations matching both criteria.

### 6. Filter with pagination
```
GET /api/stations?name=central&page=2&pageSize=10
```
Returns the second page of stations with "central" in the name, 10 items per page.

## Technical Details

### Query Handler: `FilterStationQueryHandler`

**Location**: `Application/UseCases/StationManagement/Queries/FilterStation/FilterStationQueryHandler.cs`

**Process**:
1. Starts with all stations from the repository
2. Applies name filter if provided (case-insensitive contains)
3. Applies address filter if provided (case-insensitive contains)
4. Counts total matching records
5. Applies pagination (skip/take)
6. Maps stations to DTOs
7. For each station, calculates capacity by counting vehicles where `EndTime == null`

**Performance Considerations**:
- Capacity calculation runs a separate query for each station in the result set
- With default page size of 20, this means up to 20 additional queries per request
- Consider caching or query optimization for high-traffic scenarios

## Benefits

? **Unified endpoint** - Single endpoint for listing and filtering stations  
? **Flexible filtering** - Search by name, address, or both  
? **Case-insensitive** - User-friendly search experience  
? **Partial matching** - Returns stations containing the search term  
? **Real-time capacity** - Shows current number of vehicles at each station  
? **Backward compatible** - Works without any parameters (returns all stations)  
? **Paginated** - Prevents performance issues with large datasets  

## Migration Notes

### Before:
- Used `ViewStationQuery` which only supported pagination
- No filtering capability

### After:
- Uses `FilterStationQuery` with full filtering support
- Maintains all existing functionality
- Adds optional name and address filters
- Backward compatible with existing clients

## Testing Recommendations

1. **Unit Tests**:
   - Test filtering by name only
   - Test filtering by address only
   - Test filtering by both name and address
   - Test pagination with filters
   - Test case-insensitive matching

2. **Integration Tests**:
   - Test with real database queries
   - Verify capacity calculation accuracy
   - Test performance with large datasets

3. **API Tests**:
   - Verify response format
   - Test all parameter combinations
   - Verify backward compatibility (no parameters)
