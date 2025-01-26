import React, { useState, useEffect } from 'react';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import '../index.css';

// URL voor de backend, standaard 'http://localhost:5165' als dit niet is opgegeven in de omgevingsvariabelen
const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

/// <summary>
/// Deze component is verantwoordelijk voor het ophalen, weergeven en beheren van abonnementgegevens. 
/// Het controleert ook of de gebruiker een stafflid is en welk type kantoor ze behoren (Front of Back Office).
/// </summary>
function AbonementUitlegPage() {
    // States om abonnementgegevens, staff-informatie, foutmeldingen en laadtoestand op te slaan
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
                // Als de response niet goed is, gooi een foutmelding
                if (!response.ok) {
                    return response.json().then(data => {
                        throw new Error(data?.message);
                    });
                }
                return response.json();
            })
            .then((data) => {
                // Combineer de data in één object
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
        // Controleer de sessie voor staff-informatie om de toegang van de gebruiker te bepalen
        fetch(`${BACKEND_URL}/api/Login/CheckSessionStaff`, {
            credentials: 'include',
            method: 'GET',
        })
            .then((response) => {
                // Als de sessie ongeldig is of de gebruiker geen staff is, reset de staff-flags
                if (!response.ok) {
                    throw new Error('Niet een stafflid of sessie verlopen');
                }
                return response.json();
            })
            .then((data) => {
                // Zet de staff-gerelateerde flags op basis van het type kantoor (Front of Back office)
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
                // Verwerk het geval waarin de staff-sessie ongeldig of verlopen is
                setIsStaff(false);
                setIsFrontOffice(false);
            });
    }, []);
    
    const fetchSubscriptions = async () => {
        try {
            setLoading(true);  // Zet de loading-toestand op true voordat gegevens worden opgehaald
            setError(null);     // Maak eventuele bestaande foutmeldingen leeg

            // Haal de abonnement-ID's op van de backend
            const response = await fetch(`${BACKEND_URL}/api/Subscription/GetSubscriptionIds`, {
                method: 'GET',
                credentials: 'include',
            });

            // Controleer of het ophalen van abonnement-ID's succesvol was
            if (!response.ok) {
                throw new Error('Mislukt om abonnement-ID\'s op te halen');
            }

            const data = await response.json();
            const subscriptionIds = data?.message || [];  // Standaard op een lege array als er geen ID's zijn

            // Haal de details op van elk abonnement
            const subscriptionsData = await Promise.all(
                subscriptionIds.map(async (id) => {
                    try {
                        const response = await fetch(`${BACKEND_URL}/api/Subscription/GetSubscriptionData?id=${id}`, {
                            method: 'GET',
                            headers: { 'Content-Type': 'application/json' },
                            credentials: 'include',
                        });

                        // Controleer of het ophalen van abonnementgegevens succesvol was
                        if (!response.ok) {
                            throw new Error('Mislukt om abonnementgegevens op te halen');
                        }

                        const subscription = await response.json();
                        return subscription.message;
                    } catch (error) {
                        console.error(`Fout bij het ophalen van abonnement ${id}:`, error.message);
                        return null;
                    }
                })
            );

            // Werk de state bij met geldige abonnementgegevens
            setSubscriptions(subscriptionsData.filter(Boolean));
        } catch (error) {
            setError(error.message);  // Zet de foutmelding in de state als het ophalen mislukt
        } finally {
            setLoading(false);  // Zet de loading-toestand op false als het ophalen klaar is
        }
    };

    // Haal abonnementen op bij het laden van de component
    useEffect(() => {
        fetchSubscriptions();
    }, []);

    // Verwijder abonnement
    const handleDelete = async (id) => {
        console.log("Verwijderen abonnement met ID: ", id);
        if (!id) {
            alert("Geen geldig abonnement ID");
            return;
        }

        // Bevestig de verwijdering
        if (!window.confirm('Wil je dit abonnement verwijderen?')) return;

        try {
            setLoading(true);  // Zet de loading-toestand op true tijdens de verwijdering

            // Verstuur de delete-aanroep naar de backend
            const response = await fetch(`${BACKEND_URL}/api/Subscription/DeleteSubscription?id=${id}`, {
                method: 'DELETE',
                credentials: 'include',
            });

            // Als de response niet goed is, gooi een foutmelding
            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Mislukt om abonnement te verwijderen');
            }

            // Verwijder het abonnement uit de state
            setSubscriptions(subscriptions.filter((subscription) => subscription.ID !== id));
            alert('Abonnement is succesvol verwijderd.');
            fetchSubscriptions();  // Haal de abonnementen opnieuw op om de wijzigingen weer te geven
        } catch (error) {
            console.error(error.message);
            alert('Er is een fout opgetreden bij het verwijderen van het abonnement.');
        } finally {
            setLoading(false);  // Zet de loading-toestand op false na de verwijdering
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
