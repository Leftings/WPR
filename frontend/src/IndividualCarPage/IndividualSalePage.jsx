import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
//import './IndividualSalePage.css';
import '../index.css';

function CarDetailPage() {
    const location = useLocation();
    const navigate = useNavigate();
    const vehicle = location.state?.vehicle;

    const handleBuyNow = () => {
        const rentalDates = location.state?.rentalDates || [null, null]; 
        navigate("/buy", { state: { vehicle, rentalDates } });
    };
    
    if (!vehicle) {
        return (
            <>
                <GeneralHeader />
                <div className="error-message">
                    <h2>Voertuig niet gevonden!</h2>
                    <p>Ga terug om een auto te selecteren.</p>
                </div>
                <GeneralFooter />
            </>
        );
    }

    return (
        <>
            <GeneralHeader />

            <div className="car-detail-page">
                <div className="car-detail-header">
                    <h1 className="car-name">{`${vehicle.Brand || "Onbekend"} ${vehicle.Type || ""}`}</h1>
                    <p className="car-price">{`$${vehicle.Price || "N/A"}`}</p>
                </div>

                <div className="car-detail-content">
                    <div className="car-image-container">
                        <img
                            src={`data:image/jpeg;base64,${vehicle.VehicleBlob || ""}`}
                            alt={`${vehicle.Brand || "Onbekend"} ${vehicle.Type || ""}`}
                            className="car-detail-image"
                        />
                    </div>
                    <div className="car-detail-info">
                        <h3>Omschrijving</h3>
                        <p>{vehicle.Description || "Geen omschrijving beschikbaar."}</p>
                        <div className="car-actions">
                            <button className="buy-button" onClick={handleBuyNow}>Huren</button>
                        </div>
                    </div>
                </div>
            </div>

            <GeneralFooter />
        </>
    );
}

export default CarDetailPage;
