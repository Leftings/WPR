import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './ConfirmationPage.css';

function ConfirmationPage() {
    const location = useLocation();
    const navigate = useNavigate();
    const rentalDetails = location.state?.rental;

    const handleGoBack = () => {
        navigate('/');
    };

    if (!rentalDetails) {
        return (
            <div className="confirmation-page">
                <GeneralHeader />
                <div className="error-message">
                    <h2>Geen huurgegevens gevonden!</h2>
                    <p>Ga terug en probeer opnieuw.</p>
                </div>
                <GeneralFooter />
            </div>
        );
    }

    return (
        <div className="confirmation-page">
            <GeneralHeader />
            <div className="content">
                <h1 className="title">Bedankt voor uw huur!</h1>
                <div className="confirmation-details">
                    <h3>Huurgegevens:</h3>
                    <p><strong>Voertuig:</strong> {rentalDetails.vehicleBrand} {rentalDetails.vehicleType}</p>
                    <p><strong>Startdatum:</strong> {new Date(rentalDetails.startDate).toLocaleDateString()}</p>
                    <p><strong>Einddatum:</strong> {new Date(rentalDetails.endDate).toLocaleDateString()}</p>
                    <p><strong>Totaalprijs:</strong> €{rentalDetails.totalCost}</p>
                </div>
            </div>
            <div className="payment-section">
                <h3>Betaling:</h3>
                <p>Scan de QR-code hieronder om uw betaling te voltooien:</p>
                <div className="qr-code-container">
                    <img
                        src="https://upload.wikimedia.org/wikipedia/commons/2/2f/Rickrolling_QR_code.png" 
                        alt="Fake PayPal QR Code"
                        className="qr-code"
                    />
                </div>
                <p className="payment-note">Deze QR-code is alleen voor demonstratiedoeleinden en verwerkt geen echte betalingen.</p>
            </div>
            <GeneralFooter />
        </div>
    );
}

export default ConfirmationPage;
