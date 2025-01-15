import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
//import './reviewHireRequest.css';
import '../../index.css'; 
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function GetReview(id) {
  // Specifieke gegevens van een review opvragen
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
      // Een list van alle gegevens maken
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
  // Status zetten op accepteren of weigeren
  return fetch(`${BACKEND_URL}/api/AcceptHireRequest/answerHireRequest`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify({ Id: id, Status: status, UserType: 'vehicleManager' }), 
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
      setNewRequests((prevRequests) => prevRequests.filter((request) => request.ID !== id));
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
    // Authenticatie Check
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
    const fetchNewRequests = async () => {
      setLoading(true);
      setError(null);

      try {
        const response = await fetch(`${BACKEND_URL}/api/AcceptHireRequest/getReviewsIds?user=vehicleManager`, {
          method: 'GET',
          credentials: 'include',
        });

        if (!response.ok) {
          throw new Error('Failed to fetch new requests');
        }

        const data = await response.json();
        const requestsToLoad = data?.message || [];
        
        // Er wordt door elk id heen gegaan
        for (const id of requestsToLoad) {
          // Laden aanzetten voor review
          setLoadingRequests((prevState) => ({ ...prevState, [id]: true }));
          
          try {
            // review gegevens opvragen
            const review = await GetReview(id);

            if (review?.message) {
              // Review toevoegen aan reviews
              setNewRequests((prevRequests) => [...prevRequests, review.message]);
              // Review laden uitzetten
              setLoadingRequests((prevState) => ({ ...prevState, [id]: false }));
              // Algemeen laden uitzetten
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
                          <button className="accept" onClick={() => SetStatus(request.ID, 'accepted', setNewRequests)}>Accepteren</button>
                          <button className="deny" onClick={() => SetStatus(request.ID, 'denied', setNewRequests)}>Weigeren</button>
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
