import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

async function checkSession() {
    try {
        // Controleer of de sessie actief is via een API-call
        const response = await fetch(`${BACKEND_URL}/api/Login/CheckSession`, {
            credentials: 'include', // Stuurt cookies mee met het verzoek
        });

        if (!response.ok) {
            throw new Error('Sessiewaarschuwing mislukt');
        }

        const data = await response.json();
        console.log('Sessie Actief: ', data);
        return data;
    } catch (error) {
        console.error('Fout: ', error.message);
        return null; // Retourneer null bij een fout
    }
}

function PayPage() {
    const location = useLocation(); // Toegang tot state die via routing wordt doorgegeven
    const navigate = useNavigate(); // Mogelijkheid om de gebruiker te navigeren naar andere routes
    const vehicle = location.state?.vehicle; // Haal het voertuigobject op uit de doorgestuurde state
    const rentalDates = location.state?.rentalDates || [null, null]; // Huurdatums ophalen of standaard leeg laten

    const [userDetails, setUserDetails] = useState({ email: "", address: "" }); // Houdt gebruikersgegevens bij
    const [totalCost, setTotalCost] = useState(0); // Totaalkosten voor de huur
    const [rentalDays, setRentalDays] = useState(0); // Aantal huurdagen
    
    useEffect(() => {
        if (rentalDates[0] && rentalDates[1]) {
            const start = new Date(rentalDates[0]);
            const end = new Date(rentalDates[1]);
            const days = Math.ceil((end - start) / (1000 * 60 * 60 * 24)) + 1; // Bereken het aantal dagen tussen de datums
            const pricePerDay = parseFloat(vehicle?.Price.replace(',', '.') || "0"); // Converteer de prijs naar een getal

            if (!isNaN(pricePerDay) && days > 0) {
                setRentalDays(days); // Zet het aantal huurdagen
                setTotalCost(days * pricePerDay); // Bereken de totale kosten
            }
        }
    }, [rentalDates, vehicle]);
    
    const handlePurchase = async () => {
        console.log('Rental Dates:', rentalDates);

        // Controleer of de gebruiker is ingelogd
        const session = await checkSession();

        if (!session) {
            toast.error("Log in om verder te gaan."); // Toon foutmelding als de gebruiker niet is ingelogd
            return;
        }

        // Controleer of verplichte velden correct zijn ingevuld
        if (!userDetails.email || !userDetails.address || !rentalDates[0] || !rentalDates[1] || !(rentalDates[0] instanceof Date) || !(rentalDates[1] instanceof Date)) {
            toast.warn("Vul alle verplichte velden in.");
            return;
        }

        // Controleer of het voertuig een geldig framenummer heeft
        if (!vehicle?.FrameNr) {
            toast.error("Voertuig heeft geen framenummer. Kies een geldig voertuig.");
            return;
        }

        // Bereid de huurdata voor om naar de backend te sturen
        const rentalData = {
            FrameNrVehicle: String(vehicle.FrameNr), // Framenummer van het voertuig
            StartDate: new Date(rentalDates[0]).toISOString(), // Startdatum van de huur
            EndDate: new Date(rentalDates[1]).toISOString(), // Einddatum van de huur
            Price: totalCost, // Totaalkosten
            Email: userDetails.email, // E-mailadres van de gebruiker
            Address: userDetails.address, // Adres van de gebruiker
        };

        try {
            // Verstuur het huurdata naar de backend
            const response = await fetch(`${BACKEND_URL}/api/Rental/CreateRental`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json", // Stel de contenttype-header in
                },
                body: JSON.stringify(rentalData), // Converteer de data naar JSON
                credentials: "include", // Stuurt cookies mee
            });

            if (!response.ok) {
                const data = await response.json();
                toast.error(`Fout: ${data.message}`); // Toon foutmelding als de backend een fout retourneert
                return;
            }

            const data = await response.json();
            toast.success("Huur succesvol verwerkt!"); // Toon succesmelding
            navigate("/confirmationPage", {
                state: {
                    rental: {
                        vehicleBrand: vehicle.Brand, // Merk van het voertuig
                        vehicleType: vehicle.Type, // Type van het voertuig
                        startDate: rentalDates[0], // Startdatum van de huur
                        endDate: rentalDates[1], // Einddatum van de huur
                        totalCost: totalCost.toFixed(2), // Totaalkosten (afgerond op 2 decimalen)
                    },
                    vehicle, // Voertuigobject
                },
            });
        } catch (error) {
            // Toon foutmelding bij een netwerk- of backendprobleem
            toast.error("Er is een fout opgetreden bij het verwerken van je huur. Probeer het opnieuw.");
        }
    };

    // Render foutbericht als er geen voertuig is
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
                            placeholder="E-mailadres"
                            value={userDetails.email}
                            onChange={(e) => setUserDetails({ ...userDetails, email: e.target.value })}
                            className="input-field"
                        />
                        <input
                            type="text"
                            placeholder="Voer afleveradres in"
                            value={userDetails.address}
                            onChange={(e) => setUserDetails({ ...userDetails, address: e.target.value })}
                            className="input-field"
                        />
                        <h3>Huurperiode</h3>
                        <p>
                            Startdatum: {new Date(rentalDates[0]).toLocaleDateString()} <br />
                            Einddatum: {new Date(rentalDates[1]).toLocaleDateString()}
                        </p>
                        <button className="buy-button" onClick={handlePurchase}>
                            Bevestig Huur
                        </button>
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
