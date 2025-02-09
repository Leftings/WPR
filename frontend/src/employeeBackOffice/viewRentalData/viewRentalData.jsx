import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import '../../index.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';
import { sorter, specific } from '../../utils/sorter.js'
import { loadArray, loadList, loadSingle } from '../../utils/backendLoader.js';
import { placingItems } from '../../utils/gridPlacement.js';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function ViewRentalData() {
    const navigate = useNavigate();

    const [error, setError] = useState(null);  
    const [rentalData, setRentalData] = useState([]);  
    const [filterType, setFilterType] = useState('Price');  
    const [filterHow, setFilterHow] = useState('Low');  
    const [loadingRequests, setLoadingRequests] = useState({}); 
    const [loading, setLoading] = useState(true);  
    const [allData, setAllData] = useState([]);  
    const [selectedCard, setSelectedCard] = useState(null); 
    const [specificData, setSpecificData] = useState([]);  
    const [specificDataLoading, setSpecificDataLoading] = useState(false);  
    const [, updateState] = useState();
    const forceUpdate = useCallback(() => updateState({}), []);
    const gridRef = useRef(null);  

    // Controleert of de gebruiker geauthenticeerd is via cookies
    useEffect(() => {
        const validateCookie = async () => {
            try {
                const response = await fetch(`${BACKEND_URL}/api/Cookie/GetUserId`, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    credentials: 'include',  // Stuurt cookies mee in de verzoeken
                });

                if (!response.ok) {
                    throw new Error('No Cookie');
                }

                await response.json();  // Als alles goed gaat, wordt de response geparsed
            } catch {
                alert('Cookie was niet geldig');  // Als de cookie niet geldig is, wordt de gebruiker doorgestuurd naar de loginpagina
                navigate('/');
            }
        };

        validateCookie();  // Roep de cookie validatie functie aan
    }, [navigate]);

    // Laadt de verhuurdata op basis van het filtertype en de volgorde
    useEffect(() => {
        const fetchData = async (filterType, filterHow) => {
            setFilterType('Price');  // Zet het standaard filtertype op 'Price'
            setError(null);  // Reset de error status
            setRentalData([]);  // Reset de lijst van huurdata
            setLoading(true);  // Zet de laadstatus naar 'true'

            try {
                // Haalt de IDs van de reviews op
                const response = await loadSingle(`${BACKEND_URL}/api/viewRentalData/GetReviewsIds`);

                if (!response) {
                    console.error('Failed to load data');
                    return;
                }

                const data = await response.json();  // Parse de response data
                const requestsToLoad = data?.message || [];  // Haal de te laden aanvragen op

                // Laadt de data voor elk van de aanvragen
                for (const id of requestsToLoad) {
                    setLoadingRequests((prevState) => ({ ...prevState, [id]: true }));

                    try {
                        const review = await loadArray(`${BACKEND_URL}/api/viewRentalData/GetReviewData?id=${id}`);

                        if (review?.message) {
                            // Voeg de review data toe aan de lijst van rentalData en sorteer de data op basis van de filters
                            setRentalData((prevRequest) => sorter([...prevRequest, review.message], filterType, filterHow));
                            setAllData((prevRequest) => sorter([...prevRequest, review.message], filterType, filterHow));
                            setLoadingRequests((prevState) => ({ ...prevState, [id]: false }));
                            setLoading(false);  // Zet de laadstatus op 'false' zodra alles geladen is
                        }
                    }
                    catch (err) {
                        console.error(`Failed to fetch data from ID: ${id}: `, err);  // Foutafhandeling als het laden van data mislukt
                    }
                }
            }
            catch (error) {
                console.error(error);
            }
            finally {
                // Zorg ervoor dat de data gesorteerd is zodra alles geladen is
                setRentalData((prevRequest) => [...sorter(prevRequest, filterType, filterHow)]);
                setAllData((prevRequest) => [...sorter(prevRequest, filterType, filterHow)]);
            }
        };

        fetchData(filterType, filterHow);  // Haal de data op wanneer de component wordt geladen
    }, []);  // Dit effect wordt alleen uitgevoerd bij het laden van de component

    // Update de huurdata wanneer het filtertype of volgorde verandert
    useEffect(() => {
        const sortedData = sorter([...rentalData], filterType, filterHow);
        setRentalData(sortedData);  // Zet de gesorteerde data in de state
    }, [filterHow]);  // Dit effect wordt uitgevoerd wanneer filterHow (volgorde) verandert

    // Update de huurdata wanneer het filtertype verandert
    useEffect(() => {
        setFilterHow("Low");  // Zet de volgorde standaard op 'Low'
        if (filterType === "VMStatus") {
            const VMData = specific([...allData], filterType, "Low", "X");  // Specifieke filtering voor VMStatus
            setRentalData(VMData);
        } else {
            const sortedData = sorter([...allData], filterType, "Low");
            setRentalData(sortedData);  // Sorteer de data op basis van het filtertype en volgorde
        }
    }, [filterType]);  // Dit effect wordt uitgevoerd wanneer filterType verandert

    // Logging voor debugging (optioneel)
    useEffect(() => {
        console.log("filterHow changed to: ", filterHow);
    }, [filterHow]);

    useEffect(() => {
        console.log("rentalData changed to: ", rentalData);
    }, [rentalData]);

    // Functie om specifieke gegevens voor een geselecteerde huur op te halen
    async function collectSpecificData(id) {
        setSpecificDataLoading(true);  // Zet de laadstatus voor specifieke gegevens op true
        try {
            const data = await loadList(`${BACKEND_URL}/api/viewRentalData/GetFullReviewData?id=${id}`);
            console.log(data);
            setSpecificData([data.message]);  // Zet de specifieke data in de state
        }
        catch (error) {
            console.error(error);
            setSpecificData([]);  // Reset specifieke gegevens als er een fout optreedt
        }
        finally {
            setSpecificDataLoading(false);  // Zet de laadstatus voor specifieke gegevens weer op false
            forceUpdate();  // Forceer een update van de component
            console.log(specificData);
        }
    }

    // Toon een laadscherm als de gegevens nog niet zijn geladen
    if (loading) {
        return (
            <div className="loading-screen">
                <p>Laden van verzoeken...</p>
            </div>
        );
    }

    // Toon een foutmelding als er een fout optreedt
    if (error) {
        return (
            <div className="error-screen">
                <p>Error: {error}</p>
            </div>
        );
    }


return (
    <>
      <GeneralHeader />
      <main>
        <h1>Overzicht Huur Aanvragen</h1>
        <div className="filters">
            <select name="Colums" id="filter" onChange={(e) => {const newFilterType = e.target.value; setFilterType(newFilterType); setFilterHow("Low");}}>
                <option value="Price">Totaal Prijs</option>
                <option value="StartDate">Start Datum</option>
                <option value="EndDate">Eind Datum</option>
                <option value="Status">Status</option>
                <option value="VMStatus">Wagenpark Beheerder Status</option>
                <option value="OrderId">Order Id</option>
            </select>
            <select name="Sorteren" id="filter" value={filterHow} onChange={(e) => {const newFilterHow = e.target.value; setFilterHow(newFilterHow);}}>
              {['Price', 'OrderId'].includes(filterType) && (
                <>
                  <option value="Low">Laag - Hoog </option>
                  <option value="High">Hoog - Laag </option>
                </>
              )}
              {['StartDate', 'EndDate'].includes(filterType) && (
                <>
                  <option value="Low">Oudste - Recenste </option>
                  <option value="High">Recenste - Oudste </option>
                </>
              )}

              {['Status', 'VMStatus'].includes(filterType) && (
                <>
                  <option value="Low">Gesloten - Open </option>
                  <option value="High">Open - Gesloten </option>
                </>
              )}
            </select>
        </div>
        <div ref={gridRef} className="requests-box">
          {rentalData.length > 0 ? (
            <div className="requests-grid">
              {rentalData.map((data, index) => {
                const isLoading = loadingRequests[data.ID];

                return (
                  <div key={index} className="request-card" role="Button" tabIndex={0} onClick={async () => {setSelectedCard(data); await collectSpecificData(data.OrderId)}} onKeyDown={async (e) => { if (e.key === "Enter" || e.key === " "){setSelectedCard(data); await collectSpecificData(data.OrderId)}}}>
                    {isLoading ? (
                      <div className="loading-screen">
                        <p>Loading...</p>
                      </div>
                      ) : (
                      <>
                        {data.VMStatus === 'X' ? null : <p><strong>Oordeel Wagenpark Beheerder: </strong>{data.VMStatus}</p>}
                        {data.NameCustomer === null ? null : <p><strong>Naam:</strong> {data.NameCustomer}</p>}
                        <p><strong>Voertuig:</strong> {data.Vehicle}</p>
                        <p><strong>Start Datum:</strong> {new Date(data.StartDate).toLocaleDateString()}</p>
                        <p><strong>Eind Datum:</strong> {new Date(data.EndDate).toLocaleDateString()}</p>
                        <p><strong>Totaal Prijs:</strong> €{data.Price}</p>
                        <p><strong>Status: </strong>{data.Status}</p>
                        {data.Status === 'requested' ? null : (<p><strong>Beoordeeld Door: </strong>{data.NameEmployee}</p>)}
                      </>)
                    }
                  </div>
                );
              })}
            </div>
          ) : (
            <p>No new requests found.</p>
          )}
        </div>
        {selectedCard && (
          <div className="fullscreen-card-overlay">
            {specificDataLoading ? (
              <div className="loading-screen">
                <p>Loading detailed data...</p>
              </div>
            ) : (
              specificData.map((data, index) => (
                <div key={index} className="fullscreen-card">
                  <h1><span>Huur gegevens</span></h1>
                  <button className="close-btn" onClick={() => setSelectedCard(null)}>X</button>

                  <div className="person">
                    <h2>Persoons gegevens</h2>
                    {data.AccountType === 'Business' ? null : <p><strong>Naam:</strong> {data.LastName}, {data.FirstName}</p>}
                    <p><strong>Email:</strong> {data.Email}</p>
                    {data.AccountType === 'Business' ? null : <p><strong>Telefoon Nummer:</strong> {data.TelNum}</p>}
                    {data.AccountType === 'Business' ? null: <p><strong>Adres:</strong> {data.Adres}</p>}
                  </div>
                  <div className="vehicle">
                    <h2>Voertuig gegevens</h2>
                    <p><strong>Merk:</strong> {data.Brand}</p>
                    <p><strong>Type:</strong> {data.Type}</p>
                    <p><strong>Bouwjaar:</strong> {data.YoP}</p>
                    <p><strong>Kleur:</strong> {data.Color}</p>
                    <p><strong>Soort Voertuig:</strong> {data.Sort}</p>
                    <p><strong>Nummer Plaat:</strong> {data.LicensePlate}</p>
                  </div>
                  <div className="rental">
                    <h2>Overige huurgegevens</h2>
                    <p><strong>Start Datum:</strong> {new Date(data.StartDate).toLocaleDateString()}</p>
                    <p><strong>Eind Datum:</strong> {new Date(data.EndDate).toLocaleDateString()}</p>
                    <p><strong>Totaal Prijs:</strong> €{data.Price}</p>
                    <p><strong>Status: </strong>{data.Status}</p>
                    {selectedCard.Status !== 'requested' && (
                      <p><strong>Beoordeeld Door:</strong> {data.NameEmployee}</p>
                    )}
                  </div>
                  {selectedCard.VMStatus !== 'X' && (
                    <div className="business">
                      <h2>Bedrijf gegevens</h2>
                      <p><strong>Bedrijfs Naam:</strong> {data.BusinessName}</p>
                      <p><strong>KvK nummer:</strong> {data.KvK}</p>
                      <p><strong>Adres:</strong> {data.AdresBusiness}</p>
                      <p><strong>Oordeel Wagenpark Beheerder:</strong> {data.VMStatus}</p>
                    </div>
                  )}
                  <div className={`blob ${selectedCard.VMStatus === 'X' ? 'blob-right' : ''}`}>
                    <img src={`data:image/jpeg;base64,${data.VehicleBlob}`} alt="vehicle" />
                  </div>
                </div>
              ))
            )}
          </div>
        )}
      </main>
      <GeneralFooter />
    </>
  );
}

export default ViewRentalData;
