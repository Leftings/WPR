import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './reviewHireRequest.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';
import { loadList, loadSingle } from '../../utils/backendLoader';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function GetReview(id) {
  // Data wordt opgehaald van een specifieke review
  return fetch(`${BACKEND_URL}/api/AcceptHireRequest/getReview?id=${id}`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
  })
    .then((response) => {
      if (!response.ok) {
        return response.json().then(data => {
          throw new Error(data?.message); 
        });
      }
      return response.json();
    })
    .then((data) => {
      // Alle gegevens owrden omgezet naar 1 list
      const combinedData = data?.message.reduce((acc, item) => {
        return { ...acc, ...item };
      }, {});
      return { message: combinedData }; 
    })
    .catch((error) => {
      console.error(error);
      return null;
    });
}

function SetStatus(id, status, setNewRequests) {
  // Status van een aanvraag kan worden aangepast
  return fetch(`${BACKEND_URL}/api/AcceptHireRequest/answerHireRequest`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify({ Id: id, Status: status, UserType: 'frontOffice'}), 
  })
  .then(response => {
      if (!response.ok) {
        return response.json().then(data => {
          console.error('error response: ', data);
          throw new Error(data.message || 'Failed to process the request');
        });
      }
      return response.json();
    })
  .then(() => {
      // Aanvraag wordt weggehaald
      setNewRequests((prevRequests) => prevRequests.filter((request) => request.OrderId !== id));
    })  
  .catch((error) => {
    console.error(error);
    return null;
  });
}


function ReviewHireRequest() {
  const navigate = useNavigate();
  const [newRequests, setNewRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [loadingRequests, setLoadingRequests] = useState({}); 

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
    // Aanvragen worden opgehaald
    const fetchNewRequests = async () => {
      setLoading(true);
      setError(null);

      try {
        // Alle ids worden opgehaald
        const response = await loadSingle(`${BACKEND_URL}/api/AcceptHireRequest/getReviewsIds?user=frontOffice`) /*fetch(`${BACKEND_URL}/api/AcceptHireRequest/getReviewsIds?user=frontOffice`, {
          method: 'GET',
          credentials: 'include',
        });

        if (!response.ok) {
          throw new Error('Failed to fetch new requests');
        }
          */

        const data = await response.json();
        const requestsToLoad = data?.message || [];

        // Elke id wordt afzonderlijk geladen (async)
        for (const id of requestsToLoad) {
          // Aanzetten laden
          setLoadingRequests((prevState) => ({ ...prevState, [id]: true }));
          
          try {
            const review = await loadList(`${BACKEND_URL}/api/AcceptHireRequest/getReview?id=${id}`);//await GetReview(id);
            
            if (review?.message) {
              // Request toevoegen aan requests
              setNewRequests((prevRequests) => [...prevRequests, review.message]);
              // Laden uitzetten requests
              setLoadingRequests((prevState) => ({ ...prevState, [id]: false }));
              // Algemeen laatscherm uitzetten
              setLoading(false);
            }
          } catch (err) {
            console.error(`Failed to fetch review for ID ${id}:`, err);
          }
        }
      } catch (error) {
        setError(error.message || 'An unexpected error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchNewRequests();
  }, []);

  useEffect(() => {
    console.log('New Requests:', newRequests, 'size:', newRequests.length);
  }, [newRequests]);
  

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
              {newRequests.map((request, index) => {
                const isLoading = loadingRequests[request.ID];

                return (
                  <div key={index} className="request-card">
                    {isLoading ? (
                      <div className="loading-screen">
                        <p>Loading...</p>
                      </div>
                    ) : (
                      <>
                        <p><strong>Naam:</strong> {request.FirstName} {request.LastName}</p>
                        <p><strong>Adres:</strong> {request.Adres}</p>
                        <p><strong>Email:</strong> {request.Email}</p>
                        <p><strong>Tel Num:</strong> {request.TelNum}</p>
                        <p><strong>Voertuig:</strong> {request.Brand} {request.Type}</p>
                        <p><strong>Nr Plaat:</strong> {typeof request.LicensePlate === 'string' && request.LicensePlate !== '' ? request.LicensePlate : 'N/A'}</p>
                        <p><strong>Start Datum:</strong> {new Date(request.StartDate).toLocaleDateString()}</p>
                        <p><strong>Eind Datum:</strong> {new Date(request.EndDate).toLocaleDateString()}</p>
                        <p><strong>Totaal Prijs:</strong> €{request.Price}</p>
                        <div id="buttons">
                          <button className="accept" onClick={() => SetStatus(request.OrderId, 'accepted', setNewRequests)}>Accepteren</button>
                          <button className="deny" onClick={() => SetStatus(request.OrderId, 'denied', setNewRequests)}>Weigeren</button>
                        </div>
                      </>
                    )}
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

export default ReviewHireRequest;
