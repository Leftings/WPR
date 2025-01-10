import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './viewRentalData.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL_EMPLOYEE ?? 'http://localhost:5276';

function ViewRentalData() {
  const navigate = useNavigate();
  const [error, setError] = useState(null);
  const [rentalData, setRentalData] = useState([]);
  const [filterType, setFilterType] = useState('Price');
  const [filterHow, setFilterHow] = useState('Low');

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
        setError(null);
        setRentalData([]);

        try
        {
            console.log(filterType, filterHow);
            // Alle ids worden opgehaald
            const response = await fetch(`${BACKEND_URL}/api/viewRentalData/GetReviews?sort=${filterType}&how=${filterHow}`, {
                method: 'GET',
                credentials: 'include',
            });

            if (!response.ok) {
                throw new Error('Failed to fetch data');
            }

            const data = await response.json();
            const { message } = data;

            setRentalData(message);
            console.log(data);
        }
        catch (error)
        {
            console.error(error);
        }
    }
    fetchData(filterType, filterHow);
    }, [filterType, filterHow]);

  return (
    <>
      <GeneralHeader />
      <div className="body">
        <h1>Overzicht Huur Aanvragen</h1>
        <div className="filters">
            <select name="Colums" id="filter" onChange={(e) => {const newFilterType = e.target.value; setFilterType(newFilterType);}}>
                <option value="Price">Totaal Prijs</option>
                <option value="StartDate">Start Datum</option>
                <option value="EndDate">Eind Datum</option>
                <option value="Status">Status</option>
                <option value="VMStatus">Wagenpark Beheerder Status</option>
                <option value="OrderId">Order Id</option>
            </select>

            <select name="Sorteren" id="filter" onChange={(e) => {const newFilterHow = e.target.value; setFilterHow(newFilterHow);}}>
                <option value="Low">Laag - Hoog</option>
                <option value="High">Hoog - Laag</option>
            </select>
        </div>
        <div className="requests-box">
          {rentalData.length > 0 ? (
            <div className="requests-grid">
              {rentalData.map((data, index) => {
                return (
                  <div key={index} className="request-card">
                      <>
                        <p><strong>Naam:</strong> {data.NameCustomer}</p>
                        <p><strong>Voertuig:</strong> {data.Vehicle}</p>
                        <p><strong>Start Datum:</strong> {new Date(data.StartDate).toLocaleDateString()}</p>
                        <p><strong>Eind Datum:</strong> {new Date(data.EndDate).toLocaleDateString()}</p>
                        <p><strong>Totaal Prijs:</strong> €{data.Price}</p>
                        <p><strong>Status: </strong>{data.Status}</p>
                        {data.Status === 'requested' ? null : (<p><strong>Beoordeeld Door: </strong>{data.NameEmployee}</p>)}
                        {data.VMStatus === 'X' ? null : (<p><strong>Oordeel Wagenpark Beheerder: </strong>{data.VMStatus}</p>)}
                      </>
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
