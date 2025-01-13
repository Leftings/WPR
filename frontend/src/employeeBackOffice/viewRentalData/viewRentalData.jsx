import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import './viewRentalData.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';
import { sorter, specific } from '../../utils/sorter.js'
import { loadList, loadSingle } from '../../utils/backendLoader.js';
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

  useEffect(() => {
    const handleResize = () => 
    {
      placingItems(gridRef.current, 350);
    }

    window.addEventListener('resize', handleResize);
    handleResize();
  }, []);

  useEffect(() => {
    // Authoristatie check
    const validateCookie = async () => {
      try {
        const response = await fetch(`${BACKEND_URL}/api/Cookie/GetUserId`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
          credentials: 'include',
        });

        if (!response.ok) {
          throw new Error('No Cookie');
        }

        await response.json();
      } catch {
        alert('Cookie was niet geldig');
        navigate('/');
      }
    };

    validateCookie();
  }, [navigate]);

  useEffect(() => {
    const fetchData = async (filterType, filterHow) => {
        setFilterType('Price');
        setError(null);
        setRentalData([]);
        setLoading(true);

        const tempData = [];
        try
        {
            // Alle ids worden opgehaald
            const response = await loadSingle(`${BACKEND_URL}/api/viewRentalData/GetReviewsIds`);
            
            if (!response)
            {
              console.error('Failed to load data');
              return;
            }

            const data = await response.json();
            const requestsToLoad = data?.message || [];

            for (const id of requestsToLoad)
            {
              setLoadingRequests((prevState) => ({...prevState, [id]: true}));

              try
              {
                const review = await loadList(`${BACKEND_URL}/api/viewRentalData/GetReviewData?id=${id}`);
                
                if (review?.message)
                {
                  setRentalData((prevRequest) => sorter([...prevRequest, review.message], filterType, filterHow));
                  setAllData((prevRequest) => sorter([...prevRequest, review.message], filterType, filterHow));
                  setLoadingRequests((prevState) => ({ ...prevState, [id]: false }));
                  setLoading(false);
                }
              }
              catch (err) {
                console.error(`Failed to fetch data from ID: ${id}: `, err);
              }
            }
        }
        catch (error)
        {
            console.error(error);
        }
        finally
        {
          setRentalData((prevRequest) => [...sorter(prevRequest, filterType, filterHow)]);
          setAllData((prevRequest) => [...sorter(prevRequest, filterType, filterHow)]);
        }
    }

    fetchData(filterType, filterHow);
    }, []);

    useEffect(() => {

      const sortedData = sorter([...rentalData], filterType, filterHow)
      setRentalData(sortedData);
    }, [filterHow])

    useEffect(() => {
      setFilterHow("Low");
      if (filterType === "VMStatus")
      {
        const VMData = specific([...allData], filterType, "Low", "X");
        setRentalData(VMData);
      }
      else
      {
        const sortedData = sorter([...allData], filterType, "Low");
        setRentalData(sortedData);
      }
    }, [filterType]);

    useEffect(() => {
      console.log("filterHow changed to: ", filterHow);
    }, [filterHow]);

    useEffect(() => {
      console.log("rentalData changed to: ", rentalData);
    }, [rentalData]);

    async function collectSpecificData(id)
    {
      setSpecificDataLoading(true);
      try
      {
        const data = await loadList(`${BACKEND_URL}/api/viewRentalData/GetFullReviewData?id=${id}`);
        console.log(data);
        setSpecificData([data.message]);
      }
      catch (error)
      {
        console.error(error);
        setSpecificData([]);
      }
      finally
      {
        setSpecificDataLoading(false);
        forceUpdate();
        console.log(specificData);
      }
    }

    if (loading) {
      return (
        <div className="loading-screen">
          <p>Loading requests...</p>
        </div>
      );
    }
  
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
      <div className="body">
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
        <div className="requests-box">
          {rentalData.length > 0 ? (
            <div ref={gridRef} className="requests-grid">
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
                        <p><strong>Naam:</strong> {data.NameCustomer}</p>
                        <p><strong>Voertuig:</strong> {data.Vehicle}</p>
                        <p><strong>Start Datum:</strong> {new Date(data.StartDate).toLocaleDateString()}</p>
                        <p><strong>Eind Datum:</strong> {new Date(data.EndDate).toLocaleDateString()}</p>
                        <p><strong>Totaal Prijs:</strong> €{data.Price}</p>
                        <p><strong>Status: </strong>{data.Status}</p>
                        {data.Status === 'requested' ? null : (<p><strong>Beoordeeld Door: </strong>{data.NameEmployee}</p>)}
                        {data.VMStatus === 'X' ? null : (<p><strong>Oordeel Wagenpark Beheerder: </strong>{data.VMStatus}</p>)}
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
                    <p><strong>Naam:</strong> {data.LastName}, {data.FirstName}</p>
                    <p><strong>Email:</strong> {data.Email}</p>
                    <p><strong>Telefoon Nummer:</strong> {data.Telnum}</p>
                    <p><strong>Adres:</strong> {data.Adres}</p>
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
      </div>
      <GeneralFooter />
    </>
  );
}

export default ViewRentalData;
