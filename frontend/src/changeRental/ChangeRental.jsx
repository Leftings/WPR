import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import '../index.css';
import CarRentalOverview from "../CarRentalOverview/CarRentalOverview.jsx";
import DatePicker from "react-datepicker";

// Achtergrond-URL voor API-aanroepen, standaard 'http://localhost:5165' als niet opgegeven in de omgeving
const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function ChangeRental() {
    // State variabelen voor de foutmelding, datums, basisprijs, totale kosten en huurperiode
    const [errorMessage, setErrorMessage] = useState("");  // Foutmelding bij mislukte acties
    const [dates, setDates] = useState([null, null]);      // Bewaar geselecteerde datums (start- en einddatum)
    const [basePrice, setBasePrice] = useState(null);      // Bewaar de basisprijs per dag voor het voertuig
    const [totalCost, setTotalCost] = useState(0);         // Totale kosten voor de huurperiode
    const [rentalDays, setRentalDays] = useState(0);       // Aantal dagen voor de huurperiode
    const navigate = useNavigate();                        // Gebruik navigate om naar andere pagina's te navigeren
    const location = useLocation();                        // Haal de huurgegevens uit de huidige locatie
    const rental = location.state?.rental;                 // Haal de huurgegevens op uit de locatie (doorgegeven via state)

    // Functie om de datums te verwerken wanneer de gebruiker een datum kiest
    const handleDateChange = (dates) => {
        setDates(dates);  // Update de gekozen datums
        const [start, end] = dates;  // Haal de start- en einddatum op

        if (start && end) {
            // Bereken het aantal dagen tussen de gekozen datums
            const days = Math.ceil((end - start) / (1000 * 60 * 60 * 24));
            const pricePerDay = parseFloat(basePrice.replace(',', '.') || "0");  // Zet de prijs per dag om naar een numerieke waarde

            // Als de prijs per dag geldig is en het aantal dagen groter is dan 0
            if (!isNaN(pricePerDay) && days > 0) {
                setRentalDays(days);  // Zet het aantal huurdag(en)
                setTotalCost(days * pricePerDay);  // Bereken de totale kosten
            } else {
                setRentalDays(0);  // Zet het aantal huurdag(en) op 0 als de invoer niet geldig is
                setTotalCost(0);    // Zet de totale kosten op 0
            }
        } else {
            setRentalDays(0);  // Zet de huurperiode op 0 als er geen geldige datums zijn geselecteerd
            setTotalCost(0);    // Zet de totale kosten op 0
        }

        console.log(rental.id);  // Log de ID van de huur voor debugging
    };

    // Functie om de wijziging van de huur aan te vragen (de gegevens worden naar de backend gestuurd)
    const handleWijziging = async () => {
        try {
            // Verstuur de gewijzigde huurgegevens naar de backend
            const response = await fetch(`${BACKEND_URL}/api/Rental/ChangeRental`, {
                method: 'PUT',  // Gebruik de PUT-methode om gegevens te updaten
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    Id: rental.id,  // De ID van de huur
                    StartDate: dates[0].toISOString(),  // Startdatum in ISO formaat
                    EndDate: dates[1].toISOString(),    // Einddatum in ISO formaat
                    Price: totalCost.toFixed(2),        // Totale kosten (geformatteerd naar 2 decimalen)
                }),
            });

            if (!response.ok) {
                const textResponse = await response.text();
                console.error("Error response:", textResponse);  // Toon de foutmelding van de backend
                setErrorMessage(`Error: ${textResponse}`);  // Toon de foutmelding in de UI
                return;
            }

            // Bij succesvolle wijziging
            setErrorMessage('Rental updated successfully!');  // Toon een succesmelding
            navigate("/overviewRental");  // Navigeer naar de overzichtspagina van huuren
        } catch (error) {
            // Bij een fout tijdens het verzoek
            console.error("Error when updating rental", error);
            setErrorMessage("Error when updating rental, please try again later.");  // Toon algemene foutmelding
        }
    };

    // Als er geen huurgegevens zijn doorgegeven, geef een foutmelding
    if (!rental) {
        return <div>Geen rental meegegeven :(</div>;
    }

    // Laad de basisprijs van het voertuig bij het laden van de component
    useEffect(() => {
        const fetchBasePrice = async () => {
            try {
                // Haal de basisprijs op van het voertuig via de API
                const response = await fetch(`${BACKEND_URL}/api/Vehicle/GetVehiclePriceAsync?frameNr=${rental.frameNrVehicle}`);
                const data = await response.text();
                if (response.ok) {
                    setBasePrice(data);  // Zet de basisprijs in de state
                } else {
                    setErrorMessage("Failed to fetch base price.");  // Toon een foutmelding als ophalen mislukt
                }
            } catch (error) {
                setErrorMessage("Error fetching base price.");  // Toon een foutmelding bij netwerkfouten
                console.error(error);
            }
        };
        fetchBasePrice();
    }, []);  // Dit effect wordt alleen uitgevoerd bij het laden van de component

    // Controleer of de gebruiker een geldige cookie heeft om toegang te krijgen tot de huur
    useEffect(() => {
        fetch(`${BACKEND_URL}/api/Cookie/GetUserId`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',  // Stuur cookies mee voor authenticatie
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('No Cookie');  // Gooi een fout als de cookie niet geldig is
                }
                return response.json();  // Haal de gebruikersgegevens op
            })
            .then(data => {
                const id = data?.message;  // Verkrijg de gebruikers-ID
            })
            .catch(() => {
                alert("Cookie was niet geldig");  // Toon een foutmelding als de cookie ongeldig is
                navigate('/');  // Navigeer naar de homepagina
            });
    }, [navigate]);  // Dit effect wordt uitgevoerd wanneer de component geladen is

    return (

        <>
            <GeneralHeader />
            <div className="buy-details">
                <div className="car-info">
                    <h2 className="car-title">{`${rental.carName}`}</h2>
                    <p className="car-price">{`Prijs: €${basePrice} per dag`}</p>
                </div>
                <div className="user-info">
                    <h3 className="user-info-title">Huurperiode</h3>

                    <h3>Huurperiode</h3>
                    <div className="date-picker-container">
                        <p className="date-picker-label">Selecteer datumbereik:</p>
                        <DatePicker
                            selected={new Date()}
                            onChange={handleDateChange}
                            startDate={dates[0]}
                            endDate={dates[1]}
                            selectsRange
                            inline
                            dateFormat="yyyy/MM/dd"
                            placeholderText="Selecteer start- en einddatum"
                            minDate={new Date()}
                        />
                    </div>

                    <button
                        className="buy-button"
                        onClick={handleWijziging}
                    >
                        Bevestig wijziging
                    </button>

                    {errorMessage && <p className="error-message">{errorMessage}</p>}
                </div>
                {totalCost > 0 && (
                    <div className="total-cost">
                        <h3>Totaalbedrag:</h3>
                        <p>€{totalCost.toFixed(2)}</p>
                    </div>
                )}
            </div>
        </>
    );
}

export default ChangeRental;