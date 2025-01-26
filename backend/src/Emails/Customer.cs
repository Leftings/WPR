using WPR.Repository;

namespace WPR.Email
{
    /// <summary>
    /// De Customer klasse beheert de klantgegevens.
    /// </summary>
    public class Customer : IDetails
    {
        private Dictionary<string, object> _details { get; set; }
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Constructor van de Customer klasse. Vereist een repository om klantgegevens op te halen.
        /// </summary>
        /// <param name="userRepository">De repository die klantgegevens levert.</param>
        public Customer(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Haalt de klantdetails op voor een specifiek klant-ID.
        /// </summary>
        /// <param name="id">Het ID van de klant waarvoor de details opgehaald moeten worden.</param>
        /// <returns>Asynchroon resultaat van het ophalen van klantgegevens.</returns>
        /// <exception cref="OverflowException">Wordt gegooid als het klant-ID niet kan worden geconverteerd naar een geldige integer.</exception>
        /// <exception cref="Exception">Algemene fouten die kunnen optreden tijdens het ophalen van gegevens.</exception>
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
        /// Haalt de opgeslagen klantdetails op.
        /// </summary>
        /// <returns>Een dictionary met klantgegevens.</returns>
        public async Task<Dictionary<string, object>> GetDetailsAsync()
        {
            return _details;
        }
    }
}
