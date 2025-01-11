import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './viewRentalData.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';
import { sorter, specific } from '../../utils/sorter.js'
import { loadList, loadSingle } from '../../utils/backendLoader.js';


const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL_EMPLOYEE ?? 'http://localhost:5276';

function ViewRentalData() {
  const navigate = useNavigate();
  const [error, setError] = useState(null);
  const [rentalData, setRentalData] = useState([]);
  const [filterType, setFilterType] = useState('Price');
  const [filterHow, setFilterHow] = useState('Low');
  const [loadingRequests, setLoadingRequests] = useState({}); 
  const [loading, setLoading] = useState(true);
  const [allData, setAllData] = useState([]);

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
                  setRentalData((prevRequest) => sorter([...prevRequest, review.message], filterType, filterHow).reverse());
                  setAllData((prevRequest) => sorter([...prevRequest, review.message], filterType, filterHow).reverse());
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
          setRentalData((prevRequest) => [...sorter(prevRequest, filterType, filterHow).reverse()]);
          setAllData((prevRequest) => [...sorter(prevRequest, filterType, filterHow).reverse()]);
        }
    }

    fetchData(filterType, filterHow);
    }, []);

    useEffect(() => {
      const sortedData = sorter(rentalData, filterType, filterHow)
      setRentalData(sortedData);
    }, [filterType, filterHow, rentalData])

    useEffect(() => {
      setFilterHow({ type: "Low" });
      if (filterType === "VMStatus")
      {
        const VMData = specific([...rentalData], filterType, "Low");
        setRentalData(VMData);
      }
      else
      {
        const sortedData = sorter(allData, filterType, "Low").reverse();
        setRentalData(sortedData);
      }
    }, [filterType]);

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
                <option value="Price">Totaal Prijs</option>
                <option value="StartDate">Start Datum</option>
                <option value="EndDate">Eind Datum</option>
                <option value="Status">Status</option>
                <option value="VMStatus">Wagenpark Beheerder Status</option>
                <option value="OrderId">Order Id</option>
            </select>

            <select name="Sorteren" id="filter" onChange={(e) => {const newFilterHow = e.target.value; setFilterHow(newFilterHow);}}>
              {filterType === 'Price' && (
                <>
                  <option value="Low">Laag - Hoog </option>
                  <option value="High">Hoog - Laag </option>
                </>
              )}

              {filterType === 'StartDate' && (
                <>
                  <option value="Low">Oudste - Recenste </option>
                  <option value="High">Recenste - Oudste </option>
                </>
              )}

              {filterType === 'EndDate' && (
                <>
                  <option value="Low">Oudste - Recenste </option>
                  <option value="High">Recenste - Oudste </option>
                </>
              )}

              {filterType === 'Status' && (
                <>
                  <option value="Low">Gesloten - Open </option>
                  <option value="High">Open - Gesloten </option>
                </>
              )}

              {filterType === 'VMStatus' && (
                <>
                  <option value="Low">Gesloten - Open </option>
                  <option value="High">Open - Gesloten </option>
                </>
              )}

              {filterType === 'OrderId' && (
                <>
                  <option value="Low">Laag - Hoog </option>
                  <option value="High">Hoog - Laag </option>
                </>
              )}
            </select>
        </div>
        <div className="requests-box">
          {rentalData.length > 0 ? (
            <div className="requests-grid">
              {rentalData.map((data, index) => {
                const isLoading = loadingRequests[rentalData.ID];

                return (
                  <div key={index} className="request-card">
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
      </div>
      <GeneralFooter />
    </>
  );
}

export default ViewRentalData;
