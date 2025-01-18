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
                    <h2>Car not found!</h2>
                    <p>Please go back and select a vehicle.</p>
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
                    <h1 className="car-name">{`${vehicle.Brand || "Unknown"} ${vehicle.Type || "Unknown Model"}`}</h1>
                    <p className="car-price">{`$${vehicle.Price || "N/A"}`}</p>
                </div>

                <div className="car-detail-content">
                    <div className="car-image-container">
                        <img
                            src={`data:image/jpeg;base64,${vehicle.VehicleBlob || ""}`}
                            alt={`${vehicle.Brand || "Unknown"} ${vehicle.Type || ""}`}
                            className="car-detail-image"
                        />
                    </div>
                    <div className="car-detail-info">
                        <h3>Description</h3>
                        <p>{vehicle.Description || "No description available."}</p>
                        <div className="car-actions">
                            <button className="buy-button" onClick={handleBuyNow}>Buy Now</button>
                        </div>
                    </div>
                </div>
            </div>

            <GeneralFooter />
        </>
    );
}

export default CarDetailPage;
