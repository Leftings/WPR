namespace WPR.Repository;

public interface IVehicleRepository
{
    Task<string> GetVehiclePlateAsync(int frameNr);
    Task<string> GetVehicleNameAsync(int frameNr);
    Task<string> GetVehicleColorAsync(int frameNr);
    public Task<List<string>> GetFrameNumbersAsync();
    public Task<List<Dictionary<object, string>>> GetVehicleDataAsync(string frameNr);
    public Task<List<string>> GetFrameNumberSpecifiekTypeAsync(string type);
}