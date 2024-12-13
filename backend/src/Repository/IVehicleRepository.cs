namespace WPR.Repository;

public interface IVehicleRepository
{
    Task<string> GetVehiclePlateAsync(int frameNr);
    Task<string> GetVehicleNameAsync(int frameNr);
    Task<string> GetVehicleColorAsync(int frameNr);
}