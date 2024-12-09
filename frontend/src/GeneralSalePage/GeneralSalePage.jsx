import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import ShowImage from './ShowImage.jsx';

import './GeneralSalePage.css'

/*function WelcomeUser(setWelcome) {
    fetch('http://localhost:5165/api/Cookie/GetUserName', {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
@@ -27,10 +26,9 @@ import './GeneralSalePage.css'
        .catch(error => {
            console.error('Error:', error);
        });
}*/

/*const carsForSale = [
    {
        id: 1,
        name: 'Tesla Model 3',
@@ -101,83 +99,42 @@ import './GeneralSalePage.css'
        description: 'A powerful and iconic sports car with timeless style.',
        image: 'https://via.placeholder.com/150',
    },
];*/

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function GeneralSalePage() {
    const [chosenType, setChosenType] = useState(0);
    const [type, setType] = useState('');
    const [welcomeMessage, setWelcomeMessage] = useState('');
    const [vehicles, setVehicles] = useState([]);
    const [error, setError] = useState(null);
    const fetchVehicles= async () => {
        try {
            const response = await fetch(`${BACKEND_URL}/api/vehicle/GetAllVehicles`);
            if (!response.ok) {
                throw new Error(`Error fetching vehicles: ${response.statusText}`)
            }
            const data = await response.json();
            setVehicles(data);
            console.log(data);
        } catch (e) {
            console.error(e);
            setError('Failed to load vehicles')
        }
    };

    { /*const fetchTypeVehicles= async () => {
        try {
            const response = await fetch(`${BACKEND_URL}/api/vehicle/GetTypeOfVehicles?vehicleType=${chosenType}`);
            if (!response.ok) {
                throw new Error('Error')
            }
        }
    } */}

    useEffect(() => {
        fetchVehicles();
        /*WelcomeUser(setWelcomeMessage);*/
    }, []);

    useEffect(() => {
        console.log(vehicles)
        /*WelcomeUser(setWelcomeMessage);*/
    }, [vehicles]);
    return (
        <>
            <GeneralHeader />

            <div className="general-sale-page">
                {/*<h1 className="welcome-message">{welcomeMessage}</h1>*/}

                <div className="car-sale-section">
                    <h1 className="title">Cars for Sale</h1>
                    <div className="filter-box">
                        <label for="filter">Filter: </label>
                        <select name="filter">
                            <option value="0">All</option>
                            <option value="1">Car</option>
                            <option value="2">Camper</option>
                            <option value="3">Caravan</option>
                        </select>
                    </div>
                    <div className="car-grid">
                        {vehicles.map((vehicle) => (
                            <div key={vehicle.frameNr} className="car-card">
                                <div className="car-blob">
                                    {vehicle.image ? (
                                        <img
                                            src={`data:image/jpeg;base64,${vehicle.image}`}
                                            alt={`${vehicle.brand || 'Unknown'} ${vehicle.Type || ''}`}
                                        />
                                    ) : (
                                        <p>Image not available</p>
                                    )}
                                </div>
                                <div className="car-info">
                                    <h2 className="car-name">{`${vehicle.brand || 'Unknown'} ${vehicle.type || ''}`}</h2>
                                    <p className="car-price">{`$${vehicle.price}`}</p>
                                    <p className="car-description">Vroom Vroom</p>
                                    <Link
                                        to={`/vehicle/${vehicle.frameNr}`}
                                        state={{ vehicle }}
                                        className="view-details-link"
                                    >
                                        View Details
                                    </Link>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>

            <GeneralFooter/>
        </>
    );
}

export default GeneralSalePage;