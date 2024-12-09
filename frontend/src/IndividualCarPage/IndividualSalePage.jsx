import React from 'react';
import { useLocation } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import GeneralSalePage from "../GeneralSalePage/GeneralSalePage.jsx";
import {Link, useNavigate} from 'react-router-dom';
import './IndividualSalePage.css';

function CarDetailPage() {
    const location = useLocation();
    const vehicle = location.state?.vehicle; 

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
                <Link to="/GeneralSalePage"><p className="backToSale">{"< Back"}</p></Link>

                <div className="car-detail-content">
                    <div className="car-image-container">
                        <img
                            src={`data:image/jpeg;base64,${vehicle.image}`}
                            alt={`${vehicle.brand || "Unknown"} ${vehicle.type || ""}`}
                            className="car-detail-image"
                        />
                    </div>
                    <div className="car-detail-info">
                        <h2 className ="car-name">{`${vehicle.brand || "Unknown"} ${vehicle.type || "Unknown Model"}`}</h2>
                        <p className="car-price">{`$${vehicle.price || "N/A"}`}</p>
                        <div className="car-actions">
                            <button className="buy-button">Huur</button>
                        </div>
                        <p className="car-description">{'Hele cool auto i love it!'}</p>
                        <br></br>
                        <p className="car-description">{`Merk: ${vehicle.brand}`}</p>
                        <p className="car-description">{`Kleur: ${vehicle.color || "N/A"}`}</p>
                    </div>
                </div>
            </div>

            <GeneralFooter />
        </>
    );
}

export default CarDetailPage;
