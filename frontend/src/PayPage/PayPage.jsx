import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './PayPage.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

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

function PayPage() {
    const location = useLocation();
    const navigate = useNavigate();
    const vehicle = location.state?.vehicle;
    const rentalDates = location.state?.rentalDates || [null, null];

    const [userDetails, setUserDetails] = useState({ email: "", address: "" });
    const [totalCost, setTotalCost] = useState(0);
    const [rentalDays, setRentalDays] = useState(0);

    useEffect(() => {
        if (rentalDates[0] && rentalDates[1]) {
            const start = new Date(rentalDates[0]);
            const end = new Date(rentalDates[1]);
            const days = Math.ceil((end - start) / (1000 * 60 * 60 * 24));
            const pricePerDay = parseFloat(vehicle?.Price.replace(',', '.') || "0");

            if (!isNaN(pricePerDay) && days > 0) {
                setRentalDays(days);
                setTotalCost(days * pricePerDay);
            }
        }
    }, [rentalDates, vehicle]);

    const handlePurchase = async () => {
        const session = await checkSession();

        if (!session) {
            toast.error("Log in om verder te gaan.");
            return;
        }

        if (!userDetails.email || !userDetails.address || !rentalDates[0] || !rentalDates[1]) {
            toast.warn("Vul alle verplichte velden in.");
            return;
        }

        if (!vehicle?.FrameNr) {
            toast.error("Voertuig heeft geen framenummer. Kies een geldig voertuig.");
            return;
        }

        const rentalData = {
            FrameNrCar: String(vehicle.FrameNr),
            StartDate: new Date(rentalDates[0]).toISOString(),
            EndDate: new Date(rentalDates[1]).toISOString(),
            Price: totalCost,
            Email: userDetails.email,
            Address: userDetails.address,
        };

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
                toast.error(`Fout: ${data.message}`);
                return;
            }

            const data = await response.json();
            toast.success("Huur succesvol verwerkt!");
            navigate("/confirmationPage", {
                state: {
                    rental: {
                        vehicleBrand: vehicle.Brand,
                        vehicleType: vehicle.Type,
                        startDate: rentalDates[0],
                        endDate: rentalDates[1],
                        totalCost: totalCost.toFixed(2),
                    },
                    vehicle,
                },
            });
        } catch (error) {
            toast.error("Er is een fout opgetreden bij het verwerken van je huur. Probeer het opnieuw.");
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
