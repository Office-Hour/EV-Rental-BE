namespace WebAPI.Requests.StationManagement;

public class ViewStationRequest
{
    public string? Name { get; init; }
    public string? Address { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}