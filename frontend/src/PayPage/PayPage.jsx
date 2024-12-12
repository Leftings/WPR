import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import './PayPage.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

// Functie om te controleren of de inlogsessie actief is
async function checkSession() {
    try {
        const response = await fetch(`${BACKEND_URL}/api/Login/CheckSession`, {
            credentials: 'include',
        });

        if (!response.ok) {
            throw new Error('Sessiecontrole mislukt');
        }

        const data = await response.json();
        console.log('Sessie Actief: ', data);
        return data;
    } catch (error) {
        console.error('Fout: ', error.message);
        return null;
    }
}

function PayPage() {
    const location = useLocation();
    const navigate = useNavigate();
    const vehicle = location.state?.vehicle;

    console.log('Voertuig Gegevens:', vehicle);

    const [userDetails, setUserDetails] = useState({
        email: "",
        address: "",
        rentalDates: [null, null],
    });

    const [errorMessage, setErrorMessage] = useState("");
    const [totalCost, setTotalCost] = useState(0);
    const [rentalDays, setRentalDays] = useState(0);

    const handleDateChange = (dates) => {
        const [start, end] = dates;
        setUserDetails((prevDetails) => ({ ...prevDetails, rentalDates: [start, end] }));

        if (start && end) {
            const days = Math.ceil((end - start) / (1000 * 60 * 60 * 24));
            const pricePerDay = parseFloat(vehicle?.price.replace(',', '.') || "0");

            if (!isNaN(pricePerDay) && days > 0) {
                setRentalDays(days);
                setTotalCost(days * pricePerDay);
            } else {
                setRentalDays(0);
                setTotalCost(0);
            }
        } else {
            setRentalDays(0);
            setTotalCost(0);
        }
    };

    const handlePurchase = async () => {
        // Controleert of de sessie actief is voordat de huur wordt voortgezet
        const session = await checkSession();

        if (!session) {
            setErrorMessage("Log in om door te gaan.");
            return;
        }

        // Valideerd of alle vereiste velden zijn ingevuld
        if (!userDetails.email || !userDetails.address || !userDetails.rentalDates[0] || !userDetails.rentalDates[1]) {
            setErrorMessage("Vul alle verplichte velden in.");
            return;
        }

        // Zorgt ervoor dat FrameNrCar is ingesteld op basis van voertuiggegevens
        if (!vehicle?.frameNr) {
            setErrorMessage("Voertuig heeft geen framenummer. Kies een geldig voertuig.");
            return;
        }

        const rentalData = {
            FrameNrCar: String(vehicle.frameNr),
            StartDate: userDetails.rentalDates[0].toISOString(),
            EndDate: userDetails.rentalDates[1].toISOString(),
            Price: totalCost,
            Email: userDetails.email,
            Address: userDetails.address,
        };

        console.log("Huurgegevens verzonden:", rentalData);

        try {
            const response = await fetch(`${BACKEND_URL}/api/Rental/CreateRental`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(rentalData),
                credentials: "include",
            });

            if (!response.ok) {
                const data = await response.json();
                console.error("Fout bij reactie:", data);
                setErrorMessage(`Fout: ${data.message}`);
                return;
            }

            const data = await response.json();
            console.log("Huur gemaakt:", data.message);
            navigate("/confirmation", { state: { rental: data } });
        } catch (error) {
            console.error("Fout bij het maken van de huur:", error);
            setErrorMessage("Er is een fout opgetreden bij het verwerken van uw huur. Probeer het opnieuw.");
        }
    };

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
                <h1 className="title">Bevestig Rental</h1>
                <div className="buy-details">
                    <div className="car-info">
                        <h2 className="car-title">{`${vehicle.brand || "Onbekend"} ${vehicle.type || "Model"}`}</h2>
                        <p className="car-price">{`Prijs: €${vehicle.price} per dag`}</p>

                        <div className="car-image-container">
                            {vehicle.image ? (
                                <img
                                    src={`data:image/jpeg;base64,${vehicle.image}`}
                                    alt={`${vehicle.brand || "Onbekend"} ${vehicle.type || ""}`}
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
                            placeholder="Vul uw email in voor de confirmatie"
                            value={userDetails.email}
                            onChange={(e) => setUserDetails({ ...userDetails, email: e.target.value })}
                            className="input-field"
                        />

                        <input
                            type="text"
                            name="address"
                            placeholder="Vul uw leveradres in"
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
                            Bevestig Rental
                        </button>

                        {errorMessage && <p className="error-message">{errorMessage}</p>}
                    </div>
                </div>

                {totalCost > 0 && (
                    <div className="total-cost">
                        <h3>Totaal Kosten:</h3>
                        <p>€{totalCost.toFixed(2)}</p>
                    </div>
                )}
            </div>
            <GeneralFooter />
        </div>
    );
}

export default PayPage;
