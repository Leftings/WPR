using System.Diagnostics;
using System.Runtime.CompilerServices;
using WPR.Repository;

namespace WPR.Email;

public interface ICustomerDetails : IDetails{}

/// <summary>
/// In de Customer klasse worden alle gegevens van een specifieke klant verzameld voor het aanmaken van een email.
/// </summary>
public class Customer : ICustomerDetails
{
    private Dictionary<string, object> _details { get; set; }
    private readonly IUserRepository _userRepository;
    
    /// <summary>
    /// In de constructor worden alle benodigde klasses geset.
    /// </summary>
    /// <param name="userRepository"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Customer(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Alle gegevens worden in de dictionary van de klant geset.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Alle gegevens van de klant worden via deze methode opgehaald.
    /// </summary>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> GetDetailsAsync()
    {
        return _details;
    }
}