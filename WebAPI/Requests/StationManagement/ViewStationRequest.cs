namespace WebAPI.Requests.StationManagement;

public class ViewStationRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}