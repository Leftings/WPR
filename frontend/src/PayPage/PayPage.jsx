// PayPage.js
import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
//import './PayPage.css';
import '../index.css';

// Constante voor backend URL
const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

// Functie om te controleren of de sessie actief is
async function checkSession() {
    try {
        const response = await fetch(`${BACKEND_URL}/api/Login/CheckSession`, {
            credentials: 'include',
        });

        if (!response.ok) {
            throw new Error('Sessiewaarschuwing mislukt');
        }

        const data = await response.json();
        console.log('Sessie Actief: ', data);
        return data;
    } catch (error) {
        console.error('Fout: ', error.message);
        return null;
    }
}

// Component voor de betaalpagina
function PayPage() {
    const location = useLocation(); // Verkrijg locatie-object voor state
    const navigate = useNavigate(); // Voor navigatie na succesvolle acties
    const vehicle = location.state?.vehicle; // Voertuigdetails doorgegeven via state

    console.log('Voertuiggegevens:', vehicle);

    // State voor gebruikersgegevens, foutmeldingen, totaal kosten en huurperiode
    const [userDetails, setUserDetails] = useState({
        email: "",
        address: "",
        rentalDates: [null, null],
    });

    const [errorMessage, setErrorMessage] = useState(""); // Foutmelding als state
    const [totalCost, setTotalCost] = useState(0); // Totale kosten
    const [rentalDays, setRentalDays] = useState(0); // Aantal huurdagen

    // Behandel datumwijziging en bereken huurperiode en totale kosten
    const handleDateChange = (dates) => {
        const [start, end] = dates;
        setUserDetails((prevDetails) => ({ ...prevDetails, rentalDates: [start, end] }));

        if (start && end) {
            const days = Math.ceil((end - start) / (1000 * 60 * 60 * 24)); // Bereken dagen
            const pricePerDay = parseFloat(vehicle?.Price.replace(',', '.') || "0"); // Prijs per dag

            if (!isNaN(pricePerDay) && days > 0) {
                setRentalDays(days); // Stel huurperiode in
                setTotalCost(days * pricePerDay); // Bereken totale kosten
            } else {
                setRentalDays(0);
                setTotalCost(0);
            }
        } else {
            setRentalDays(0);
            setTotalCost(0);
        }
    };

    // Behandel huurkoop (controleer sessie en valideer formulier)
    const handlePurchase = async () => {
        // Controleer of de sessie actief is voordat we verdergaan
        const session = await checkSession();

        if (!session) {
            // Toon foutmelding als de sessie niet actief is
            toast.error("Log in om verder te gaan.");
            return;
        }

        // Valideer of alle vereiste velden ingevuld zijn
        if (!userDetails.email || !userDetails.address || !userDetails.rentalDates[0] || !userDetails.rentalDates[1]) {
            // Toon waarschuwing als vereiste velden ontbreken
            toast.warn("Vul alle verplichte velden in.");
            return;
        }

        // Zorg ervoor dat het voertuig een framenummer heeft
        if (!vehicle?.FrameNr) {
            // Toon foutmelding als het voertuig geen framenummer heeft
            toast.error("Voertuig heeft geen framenummer. Kies een geldig voertuig.");
            return;
        }

        // Bereid huurgegevens voor de API-aanroep voor
        const rentalData = {
            FrameNrCar: String(vehicle.FrameNr), // Framenummer
            StartDate: userDetails.rentalDates[0].toISOString(), // Startdatum
            EndDate: userDetails.rentalDates[1].toISOString(), // Einddatum
            Price: totalCost, // Totale kosten
            Email: userDetails.email, // E-mail
            Address: userDetails.address, // Adres
        };

        console.log("Huurgegevens verzonden:", rentalData);

        try {
            const response = await fetch(`${BACKEND_URL}/api/Rental/CreateRental`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(rentalData), // Verstuur huurgegevens
                credentials: "include",
            });

            if (!response.ok) {
                const data = await response.json();
                console.error("Fout in reactie:", data);
                // Toon foutmelding als de transactie mislukt
                toast.error(`Fout: ${data.message}`);
                return;
            }

            const data = await response.json();
            console.log("Huur aangemaakt:", data.message);
            // Toon succesmelding als de transactie succesvol is
            toast.success("Huur succesvol verwerkt!");

            navigate("/confirmationPage", {
                state: {
                    rental: {
                        vehicleBrand: vehicle.Brand,
                        vehicleType: vehicle.Type,
                        startDate: userDetails.rentalDates[0],
                        endDate: userDetails.rentalDates[1],
                        totalCost: totalCost.toFixed(2),
                    },
                    vehicle,
                },
            });

        } catch (error) {
            console.error("Fout bij het aanmaken van de huur:", error);
            // Toon foutmelding voor netwerk-/serverproblemen
            toast.error("Er is een fout opgetreden bij het verwerken van je huur. Probeer het opnieuw.");
        }
    };

    // Als er geen voertuig is geselecteerd, toon foutmelding
    if (!vehicle) {
        return (
            <div className="buy-page">
                <GeneralHeader />
                <div className="error-message">
                    <h2>Auto niet gevonden!</h2>
                    <p>Ga terug en selecteer een voertuig.</p>
                </div>
                <GeneralFooter />
            </div>
        );
    }

    // Hoofdcomponent renderen
    return (
        <div className="buy-page">
            <GeneralHeader />
            <div className="content">
                <h1 className="title">Bevestig Huur</h1>
                <div className="buy-details">
                    <div className="car-info">
                        <h2 className="car-title">{`${vehicle.Brand || "Onbekend"} ${vehicle.Type || "Model"}`}</h2>
                        <p className="car-price">{`Prijs: €${vehicle.Price} per dag`}</p>

                        <div className="car-image-container">
                            {vehicle.VehicleBlob ? (
                                <img
                                    src={`data:image/jpeg;base64,${vehicle.VehicleBlob}`}
                                    alt={`${vehicle.Brand || "Onbekend"} ${vehicle.Type || ""}`}
                                    className="car-image"
                                />
                            ) : (
                                <p>Afbeelding niet beschikbaar</p>
                            )}
                        </div>
                    </div>
                    <div className="user-info">
                        <h3 className="user-info-title">Factuuradres en Huurperiode</h3>

                        <input
                            type="email"
                            name="email"
                            placeholder="E-mailadres"
                            value={userDetails.email}
                            onChange={(e) => setUserDetails({ ...userDetails, email: e.target.value })}
                            className="input-field"
                        />

                        <input
                            type="text"
                            name="address"
                            placeholder="Voer afleveradres in"
                            value={userDetails.address}
                            onChange={(e) => setUserDetails({ ...userDetails, address: e.target.value })}
                            className="input-field"
                        />

                        <h3>Huurperiode</h3>
                        <DatePicker
                            selected={userDetails.rentalDates[0]}
                            onChange={handleDateChange}
                            startDate={userDetails.rentalDates[0]}
                            endDate={userDetails.rentalDates[1]}
                            selectsRange
                            inline
                            dateFormat="yyyy/MM/dd"
                            placeholderText="Selecteer start- en einddatum"
                        />

                        <button
                            className="buy-button"
                            onClick={handlePurchase}
                        >
                            Bevestig Huur
                        </button>

                        {errorMessage && <p className="error-message">{errorMessage}</p>}
                    </div>
                </div>

                {totalCost > 0 && (
                    <div className="total-cost">
                        <h3>Totaalbedrag:</h3>
                        <p>€{totalCost.toFixed(2)}</p>
                    </div>
                )}
            </div>
            <GeneralFooter />
        </div>
    );
}

export default PayPage;
