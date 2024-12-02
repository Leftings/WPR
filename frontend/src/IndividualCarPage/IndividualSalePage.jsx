import React from 'react';
import { useLocation } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './IndividualSalePage.css';

function CarDetailPage() {
    const location = useLocation();
    const { car } = location.state;

    if (!car) {
        return <div>Car not found!</div>;
    }

    return (
        <>
            <GeneralHeader />

            <div className="car-detail-page">
                <div className="car-detail-header">
                    <h1 className="car-name">{car.name}</h1>
                    <p className="car-price">{car.price}</p>
                </div>

                <div className="car-detail-content">
                    <div className="car-image-container">
                        <img src={car.image} alt={car.name} className="car-detail-image" />
                    </div>
                    <div className="car-detail-info">
                        <h3>Description</h3>
                        <p>{car.description}</p>
                        <div className="car-actions">
                            <button className="buy-button">Buy Now</button>
                            <button className="add-to-cart-button">Add to Cart</button>
                        </div>
                    </div>
                </div>
            </div>

            <GeneralFooter />
        </>
    );
}

export default CarDetailPage;
