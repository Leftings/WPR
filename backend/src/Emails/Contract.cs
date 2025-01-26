using WPR.Repository;

namespace WPR.Email
{
    /// <summary>
    /// De Contract klasse beheert de contractdetails voor een order.
    /// </summary>
    public class Contract : IDetails
    {
        private Dictionary<string, object> _details { get; set; }
        private readonly IContractRepository _contractRepository;

        /// <summary>
        /// Constructor van de Contract klasse. Vereist een repository om contractinformatie op te halen.
        /// </summary>
        /// <param name="contractRepository">De repository die de contractgegevens levert.</param>
        public Contract(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
        }

        /// <summary>
        /// Haalt de details van een contract op voor een specifieke order.
        /// </summary>
        /// <param name="orderId">Het ID van de order waarvoor de contractdetails opgehaald moeten worden.</param>
        /// <returns>Asynchroon resultaat van het ophalen van contractdetails.</returns>
        /// <exception cref="OverflowException">Wordt gegooid als het orderId niet kan worden geconverteerd naar een geldige integer.</exception>
        /// <exception cref="Exception">Algemene fouten die kunnen optreden tijdens het ophalen van gegevens.</exception>
        public async Task SetDetailsAsync(object orderId)
        {
            try
            {
                _details = await _contractRepository.GetContractInfoAsync(Convert.ToInt32(orderId));
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
        /// Haalt de opgeslagen contractdetails op.
        /// </summary>
        /// <returns>Een dictionary met de contractdetails.</returns>
        public async Task<Dictionary<string, object>> GetDetailsAsync()
        {
            return _details;
        }
    }
}
