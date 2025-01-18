using System.Data;
using System.Diagnostics;
using WPR.Controllers.AddBusiness;
using WPR.Controllers.signUpStaff;
namespace WPR.Repository;

/// <summary>
/// Interface defining methods voor user gerelateerde database operaties,
/// validatie, user management, data retrieval.
///
/// Het Repository Pattern abstraheert de logica voor data toegang en zorgt voor gecentraliseerde
/// en consitente interface voor interactie met de database.
/// Dit is goed voor: testbaarheid, onderhoudbaarheid en scheiding van verantwoordelijkheden binnen de applicatie.
/// </summary>
public interface IEmployeeRepository
{
    Task<(bool status, string message)> AddVehicleAsync(int yop, string brand, string type, string licensPlate, string color, string sort, double price, string description, byte[] vehicleBlob, int places);
    Task<(bool status, string message)> AddStaff(SignUpStaffRequest request);
    Task<(bool status, string message)> checkUsageEmailAsync(string email);
    Task<(bool status, List<string> ids)> GetReviewIdsAsync(string user, string userId);
    Task<(bool status, List<Dictionary<string, object>> data)> GetReviewAsync(string id);
    Task<(bool status, string message)> SetStatusAsync(string id, string status, string employee, string userType);
    Task<(bool status, string message)> AddBusiness(AddBusinessRequest request);
    public (int StatusCode, string Message, IList<int> KvK) ViewBusinessRequests();
    Task<(int StatusCode, string Message, Dictionary<string, object> data)> ViewBusinessRequestDetailed(int kvk);
    public (int StatusCode, string Message) BusinessAccepted(int kvk);
    public (int StatusCode, string Message) BusinessDenied(int kvk);
    public (bool Status, string Message, Dictionary<string, object> Data) GetBusinessInfo(int kvk);
}