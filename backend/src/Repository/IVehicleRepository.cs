namespace WPR.Repository;
using WPR.Controllers.Rental;

using System.Security.Policy;
using Microsoft.AspNetCore.Mvc;
using Mysqlx.Crud;
using WPR.Controllers.Rental;

public interface IVehicleRepository
{
    Task<string> GetVehiclePlateAsync(int frameNr);
    Task<string> GetVehicleNameAsync(int frameNr);
    Task<string> GetVehicleColorAsync(int frameNr);
    public Task<List<string>> GetFrameNumbersAsync();
    public Task<List<Dictionary<object, string>>> GetVehicleDataAsync(string frameNr);
    public Task<List<string>> GetFrameNumberSpecifiekTypeAsync(string type);
    public Task<(bool Status, string Message)> HireVehicle(RentalRequest requset, string userId);
    public (bool Status, int StatusCode, string Message) CancelRental(int rentalId, string userCookie);
    public Task<(bool Status, int StatusCode, string Message, IList<object> UserRentals)> GetAllUserRentals(string userCookie);
    public (bool Status, int StatusCode, string Message, IList<object> UserRentals) GetAllUserRentalsDetailed();
    public (bool Status, int StatusCode, string Message) ChangeRental(UpdateRentalRequest request);
}