import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './reviewHireRequest.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL_EMPLOYEE ?? 'http://localhost:5276';

function ReviewHireRequest() {
  const [newRequests, setNewRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Fetch data for new requests
  const fetchNewRequests = async () => {
    setLoading(true);  // Set loading to true before making the request
    setError(null);    // Clear previous errors

    try {
      const response = await fetch(`${BACKEND_URL}/api/AcceptHireRequest/getAllRequests`, {
        method: 'GET',
        credentials: 'include',
      });

      console.log('Response status:', response.status); // Debug response status

      if (!response.ok) {
        throw new Error('Failed to fetch new requests');
      }

      const data = await response.json();
      console.log('Fetched data:', data); // Debug the fetched data

      if (data?.message) {
        setNewRequests(data.message);  // Assuming the backend returns data in the "message" field
      } else {
        setNewRequests([]); // Handle case where message is missing
      }
    } catch (error) {
      console.error('Error fetching new requests:', error);
      setError(error.message || 'An unexpected error occurred');
    } finally {
      setLoading(false);  // Set loading to false after the request is complete
    }
  };

  useEffect(() => {
    fetchNewRequests();
  }, []);  // Empty dependency array ensures this runs only once when the component mounts

  if (loading) {
    return (
      <div className="loading-screen">
        <p>Loading new requests...</p>
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
        <h1>New Requests</h1>
        <div className="requests-box">
          {newRequests.length > 0 ? (
            <div className="requests-grid">
              {newRequests.map((request, index) => (
                <div key={index} className="request-card">
                  <p><strong>Naam:</strong> {request.FirstName} {request.LastName}</p>
                  <p><strong>Adres:</strong> {request.Adres}</p>
                  <p><strong>Email:</strong> {request.Email}</p>
                  <p><strong>Tel Num:</strong> {request.TelNum}</p>
                  <p><strong>Voertuig:</strong> {request.Brand} {request.Type}</p>
                  <p><strong>Nr Plaat:</strong> {typeof request.LicensePlate === 'string' && request.LicensePlate !== '' ? request.LicensePlate : 'N/A'}</p>
                  <p><strong>Start Datum:</strong> {new Date(request.StartDate).toLocaleDateString()}</p>
                  <p><strong>Eind Datum:</strong> {new Date(request.EndDate).toLocaleDateString()}</p>
                  <p><strong>Totaal Prijs:</strong> €{request.Price}</p>
                  <p><strong>Status:</strong> {request.Status}</p>
                  <div id="buttons"> 
                    <button className="accept">Accepteren</button>
                    <button className="deny">Weigeren</button>
                  </div>
                </div>
              ))}
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

export default ReviewHireRequest;
