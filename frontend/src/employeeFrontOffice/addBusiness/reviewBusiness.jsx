import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
//import './addBusiness.css';
import '../../index.css';
import GeneralHeader from '../../GeneralBlocks/header/header.jsx';
import GeneralFooter from '../../GeneralBlocks/footer/footer.jsx';
import { loadSingle, loadList } from '../../utils/backendLoader';
import { pushWithoutBodyKind } from '../../utils/backendPusher.js';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function ReviewBusiness() {
    const navigate = useNavigate();
    const [newRequests, setNewRequests] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [loadingRequests, setLoadingRequests] = useState({}); 

    function RemoveCard(response, kvk) {
      if (response === 'Accepted') {
        pushWithoutBodyKind(`${BACKEND_URL}/api/AddBusiness/businessAccepted?kvk=${kvk}`, 'PUT');
        setNewRequests((prevRequests) => prevRequests.filter((request) => request.KvK !== kvk));
      }
      else
      {
        pushWithoutBodyKind(`${BACKEND_URL}/api/AddBusiness/businessDenied?kvk=${kvk}`, 'DELETE');
        setNewRequests((prevRequests) => prevRequests.filter((request) => request.KvK !== kvk))
      }
    }
    
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
            const response = await loadSingle(`${BACKEND_URL}/api/AddBusiness/getNewBusinesses`)
            const data = await response.json();
            const requestsToLoad = data?.message || [];
    
            // Elke id wordt afzonderlijk geladen (async)
            for (const id of requestsToLoad) {
              // Aanzetten laden
              setLoadingRequests((prevState) => ({ ...prevState, [id]: true }));
              
              try {
                const review = await loadList(`${BACKEND_URL}/api/AddBusiness/getNewBusiness?kvk=${id}`);
                
                if (review?.data) {
                  console.log(review.data);
                  // Request toevoegen aan requests
                  setNewRequests((prevRequests) => [...prevRequests, review.data]);
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
            <p>Laden van niewe verzoeken...</p>
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
            <h1>Nieuwe verzoeken</h1>
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
                            <p><strong>Bedrijf:</strong> {request.BusinessName}</p>
                            <p><strong>Adres:</strong> {request.Adres}</p>
                            <p><strong>KvK:</strong> {request.KvK}</p>
                            <p><strong>Contact Email:</strong> {request.ContactEmail}</p>
                            <div id="buttons">
                                <button className="accept" onClick={() => {RemoveCard('Accepted', request.KvK)}}>Accepteren</button>
                                <button className="deny" onClick={() => {RemoveCard('Denied', request.KvK)}}>Weigeren</button>
                            </div>
                            </>
                        )}
                        </div>
                    );
                    })}
                </div>
                ) : (
                <p>Geen nieuwe verzoeken gevonden.</p>
                )}
            </div>
            </div>
            <GeneralFooter />
        </>
    );
}

export default ReviewBusiness;
