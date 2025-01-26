using System.Diagnostics;
using System.Runtime.CompilerServices;
using WPR.Repository;

namespace WPR.Email;

public interface ICustomerDetails : IDetails{}
public class Customer : ICustomerDetails
{
    private Dictionary<string, object> _details { get; set; }
    private readonly IUserRepository _userRepository;

    public Customer(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task SetDetailsAsync(object id)
    {
        try
        {
            _details = await _userRepository.GetCustomerDetails(Convert.ToInt32(id));
        }
        catch (OverflowException ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<Dictionary<string, object>> GetDetailsAsync()
    {
        return _details;
    }
}