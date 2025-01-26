import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ToastContainer, toast } from 'react-toastify';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import '../index.css';

// Achtergrond-URL voor API-aanroepen, standaard 'http://localhost:5165' als niet opgegeven in de omgeving
const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

/**
 * CarRentalOverview is een component die alle gehuurde voertuigen toont voor de gebruiker.
 * Het ondersteunt het annuleren van een huur en het wijzigen van huurgegevens.
 *
 * @component
 * @example
 * <CarRentalOverview />
 */
function CarRentalOverview() {
    // State variabelen voor huurgegevens, foutmeldingen en de geselecteerde huur
    const [rentals, setRentals] = useState([]);  // Bewaar de huurgegevens
    const [error, setError] = useState(null);     // Bewaar foutmeldingen
    const [chosenRental, setChosenRental] = useState(null); // Bewaar de geselecteerde huur voor annulering
    const currDate = new Date(); // Huidige datum voor verdere logica (indien nodig)
    const navigate = useNavigate(); // Gebruik navigate om te navigeren naar andere pagina's
    var modal = document.getElementById("myModal"); // Het modale venster voor annulering

    // Functie om het annuleren van een huur te starten (toon het modaal venster)
    const cancellation = (index) => {
        modal.style.display = "block";  // Toon het modale venster
        let rental = rentals[index];    // Haal de geselecteerde huur op basis van de index
        setChosenRental(rental);        // Zet de gekozen huur in de state
    };

    // Functie om foutmeldingen weer te geven afhankelijk van het type
    const settingsError = (type) => {
        if (type === "cancel") {
            toast.error("Huurcontract is al van toepassing, annuleren is niet beschikbaar.");
        }
        if (type === "change") {
            toast.error("Huurcontract is al van toepassing, wijzigingen zijn niet beschikbaar.");
        }
    };

    // Sluit het annuleringsmodaal venster
    const closeCancellation = () => {
        modal.style.display = "none";  // Zet de modal weer op "none" om deze te verbergen
    };

    // Als er op het scherm buiten de modal wordt geklikt, sluit de modal
    window.onclick = function(event) {
        if (event.target === modal) {
            modal.style.display = "none";  // Sluit de modal
        }
    };

    // Functie om een huur te annuleren
    const handleCancellation = async () => {
        try {
            // Verstuur verzoek naar backend om de huur te annuleren
            const response = await fetch(`${BACKEND_URL}/api/Rental/CancelRental?rentalId=${chosenRental.id}&frameNr=${chosenRental.frameNrCar}`, {
                method: 'DELETE',  // De DELETE-methode wordt gebruikt voor annuleren
                credentials: 'include', // Zorg ervoor dat cookies worden meegestuurd
            });

            const data = await response.json();
            if (response.ok) {
                window.location.reload();  // Herlaad de pagina na succesvolle annulering
            } else {
                setError(data.message);  // Zet de foutmelding als de annulering mislukt
            }
        } catch (error) {
            setError('Huur annuleren mislukt');  // Fout bij het annuleren
        }
    };

    // Functie om huurdetails te wijzigen
    const handleWijziging = (rental) => {
        console.log('Navigating with rental:', rental);
        // Navigeer naar de pagina voor huurwijzigingen met de huurgegevens
        navigate("/changeRental", { state: { rental } });
    };

    // Laad de huurgegevens bij het laden van de component
    useEffect(() => {
        const fetchData = async () => {
            try {
                let url = `${BACKEND_URL}/api/Rental/GetAllUserRentals`; // API URL om alle huurgegevens op te halen

                const response = await fetch(url, {
                    credentials: 'include',  // Stuur cookies mee voor authenticatie
                });
                if (!response.ok) {
                    throw new Error(`Error fetching rentals: ${response.statusText}`);
                }

                const data = await response.json();
                setRentals(data);  // Zet de huurgegevens in de state
                setError(null);     // Reset eventuele foutmeldingen
                console.log(data);
            } catch (error) {
                console.error('Failed to fetch rentals:', error);
                setError('Failed to load rentals');  // Zet de foutmelding bij falen
            }
        };

        fetchData();
    }, []); // Laad huurgegevens alleen bij het eerste renderen van de component

    // Controleer de geldigheid van de gebruiker op basis van cookies en haal gebruikers-ID op
    useEffect(() => {
        fetch(`${BACKEND_URL}/api/Cookie/GetUserId`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include', // Zorg ervoor dat cookies worden meegestuurd
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('No Cookie');  // Als er geen geldige cookie is, gooi een fout
                }
                return response.json();  // Haal de gegevens op
            })
            .then(data => {
                const id = data?.message;  // Verkrijg gebruikers-ID
            })
            .catch(() => {
                alert("Cookie was niet geldig");  // Toon een foutmelding als de cookie ongeldig is
                navigate('/');  // Navigeer naar de homepagina
            });
    }, [navigate]);  // Dit effect wordt uitgevoerd wanneer de component geladen is
    
return (
        <>
            <GeneralHeader />
            <main>
                <section className="hero">
                    <h1>Mijn auto's</h1>
                </section>

                <div id="myModal" className="modal">
                    <div className="modal-content">
                        <span className="close" onClick={closeCancellation}>&times;</span>
                        <p>Contract van volgende voertuig wordt geannuleerd:</p>
                        {chosenRental ? (
                            <>
                                <p>{`Naam - ${chosenRental.carName}`}</p>
                                <p>{`Kenteken - ${chosenRental.licensePlate}`}</p>
                            </>
                        ) : (
                            <p>Geen keuze gemaakt</p>
                        )}
                        <button className="cta-button" onClick={handleCancellation}>Annuleer</button>
                    </div>
                </div>

                <div className="container">
                    {error && <div className="error-message">{error}</div>}

                    <div className="heads">
                        <div>Voertuig</div>
                        <div>Kenteken</div>
                        <div>Startdatum</div>
                        <div>Einddatum</div>
                        <div>Prijs</div>
                        <div>Status</div>
                        <div>Instellingen</div>
                    </div>

                    {rentals.length > 0 ? (
                        rentals.map((rental, index) => {
                            const isPastDate = new Date() > new Date(rental.endDate);
                            return isPastDate ? null : (
                                <div className="rows" key={index}>
                                    <div>{rental.carName}</div>
                                    <div>{rental.licensePlate}</div>
                                    <div>{rental.startDate}</div>
                                    <div>{rental.endDate}</div>
                                    <div>{`€${rental.price}`}</div>
                                    <div>{rental.status}</div>
                                    {currDate < new Date(rental.startDate) ? (
                                        <div className="rental-config">
                                            <button className="cta-button" onClick={() => cancellation(index)}>Annuleer</button>
                                            <button className="cta-button" onClick={() => handleWijziging(rental)}>Wijzig</button>
                                        </div>
                                    ) : (
                                        <div className="rental-config">
                                            <button className="cta-button-unavailable" onClick={() => settingsError("cancel")}>Annuleer</button>
                                            <button className="cta-button-unavailable" onClick={() => settingsError("change")}>Wijzig</button>
                                        </div>
                                    )}
                                </div>
                            );
                        })
                    ) : (
                        <div>Geen huur informatie gevonden</div>
                    )}
                </div>
            </main>
            <GeneralFooter />
        </>
    );
}

export default CarRentalOverview;
