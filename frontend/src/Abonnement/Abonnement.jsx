import React, { useState, useEffect } from 'react';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import '../index.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function AbonementUitlegPage() {
    const [subscriptions, setSubscriptions] = useState([]);
    const [isStaff, setIsStaff] = useState(false);
    const [isFrontOffice, setIsFrontOffice] = useState(false);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);

    
    
    function GetSubscription(id) {
        return fetch(`${BACKEND_URL}/api/Subscription/GetSubscriptionData=${id}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
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
                const combinedData = data?.message?.reduce((acc, item) => {
                    const [key, value] = Object.entries(item)[0];
                    acc[key] = value;
                    return acc;
                }, {});
                return { message: combinedData };
            })
            .catch((error) => {
                console.error(error);
                return null;
            });
    }
    
    useEffect(() => {
        // Check session for staff information
        fetch(`${BACKEND_URL}/api/Login/CheckSessionStaff`, {
            credentials: 'include',
            method: 'GET',
        })
            .then((response) => {
                if (!response.ok) {
                    throw new Error('Not a staff member or session expired');
                }
                return response.json();
            })
            .then((data) => {
                if (data.officeType === 'Front') {
                    setIsStaff(true);
                    setIsFrontOffice(true);
                } else if (data.officeType === 'Back') {
                    setIsStaff(true);
                    setIsFrontOffice(false);
                } else {
                    setIsStaff(false);
                }
            })
            .catch(() => {
                setIsStaff(false);
                setIsFrontOffice(false);
            });
    }, []);

    const fetchSubscriptions = async () => {
        try {
            setLoading(true);
            setError(null);

            // Fetch subscription IDs
            const response = await fetch(`${BACKEND_URL}/api/Subscription/GetSubscriptionIds`, {
                method: 'GET',
                credentials: 'include',
            });

            if (!response.ok) {
                throw new Error('Failed to fetch subscription IDs');
            }

            const data = await response.json();
            const subscriptionIds = data?.message || [];

            // Fetch subscription details
            const subscriptionsData = await Promise.all(
                subscriptionIds.map(async (id) => {
                    try {
                        const response = await fetch(`${BACKEND_URL}/api/Subscription/GetSubscriptionData?id=${id}`, {
                            method: 'GET',
                            headers: { 'Content-Type': 'application/json' },
                            credentials: 'include',
                        });

                        if (!response.ok) {
                            throw new Error('Failed to fetch subscription data');
                        }

                        const subscription = await response.json();
                        return subscription.message;
                    } catch (error) {
                        console.error(`Error fetching subscription ${id}:`, error.message);
                        return null;
                    }
                })
            );

            setSubscriptions(subscriptionsData.filter(Boolean));
        } catch (error) {
            setError(error.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchSubscriptions();
    }, []);

    const handleDelete = async (id) => {
        console.log("Deleting subscription with id: ", id);
        if (!id) {
            alert("Geen geldig abonnement ID");
            return;
        }
        
        if (!window.confirm('Wil je dit abonnement verwijderen?')) return;

        try {
            setLoading(true);
            
            const response = await fetch(`${BACKEND_URL}/api/Subscription/DeleteSubscription?id=${id}`, {
                method: 'DELETE',
                credentials: 'include',
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error( errorData.message || 'Failed to delete subscription');
            }

            setSubscriptions(subscriptions.filter((subscription) => subscription.ID !== id));
            alert('Abonnement is succesvol verwijderd.');
            fetchSubscriptions();
        } catch (error) {
            console.error(error.message);
            alert('Error tijdens verwijderen abonnement.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <>
            <GeneralHeader isLoggedIn={isStaff} />
            <main>
                {loading ? (
                    <div className="loading-spinner">Loading...</div>
                ) : error ? (
                    <div className="error-message">{error}</div>
                ) : (
                    <div className="registrateFormat">
                        {subscriptions.length > 0 ? (
                            subscriptions.map((subscription) => (
                                <div key={subscription.id} className="subscription-card">
                                    <div className="subscription-info">
                                        <h2 className="subscription-type">{subscription.type || 'Onbekend'}</h2>
                                        <p className="subscription-description">{subscription.description || 'Geen beschrijving'}</p>
                                        <h3 className="subscription-type">{'€ ' + subscription.price || 'Onbekend'}</h3>
                                        {isStaff && !isFrontOffice && (
                                            <button
                                                onClick={() => handleDelete(subscription.id)}
                                                className="cta-button"
                                            >
                                                Verwijder
                                            </button>
                                        )}
                                    </div>
                                </div>
                            ))
                        ) : (
                            <p>Geen abonnementen beschikbaar.</p>
                        )}
                    </div>
                )}
            </main>
            <GeneralFooter />
        </>
    );
}

export default AbonementUitlegPage;
