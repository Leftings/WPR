import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './IndividualSalePage.css';

function CarDetailPage() {
    const location = useLocation();
    const navigate = useNavigate();
    const vehicle = location.state?.vehicle;

    const handleBuyNow = () => {
        navigate("/buy", { state: { vehicle } });
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
                    <h1 className="car-name">{`${vehicle.brand || "Unknown"} ${vehicle.type || "Unknown Model"}`}</h1>
                    <p className="car-price">{`$${vehicle.price || "N/A"}`}</p>
                </div>

                <div className="car-detail-content">
                    <div className="car-image-container">
                        <img
                            src={`data:image/jpeg;base64,${vehicle.image || ""}`}
                            alt={`${vehicle.brand || "Unknown"} ${vehicle.type || ""}`}
                            className="car-detail-image"
                        />
                    </div>
                    <div className="car-detail-info">
                        <h3>Description</h3>
                        <p>{vehicle.description || "No description available."}</p>
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
