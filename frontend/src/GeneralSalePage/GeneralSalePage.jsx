import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";

import './GeneralSalePage.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function GeneralSalePage() {
    const [vehicles, setVehicles] = useState([]);
    const [filter, setFilter] = useState(''); // State to hold selected filter

    const fetchVehicles = async () => {
        try {
            const url = filter
                ? `${BACKEND_URL}/api/vehicle/GetTypeOfVehicles?vehicleType=${filter}`
                : `${BACKEND_URL}/api/vehicle/GetAllVehicles`;

            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(`Error fetching vehicles: ${response.statusText}`);
            }
            const data = await response.json();
            setVehicles(data);
        } catch (error) {
            console.error('Failed to fetch vehicles:', error);
        }
    };


    useEffect(() => {
        fetchVehicles();
    }, [filter]);  

    // Log the value of filter to check what's happening
    console.log('Current filter value:', filter);

    let filteredVehicles;

    if (filter) {
        console.log('Filtering vehicles with sort:', filter);
        filteredVehicles = vehicles.filter(vehicle => vehicle.Sort === filter);
    } else {
        filteredVehicles = vehicles;
        console.log("No filter applied. Showing all vehicles.");
    }

    return (
        <>
            <GeneralHeader />

            <div className="general-sale-page">
                <div className="filter-section">
                    <label htmlFor="filter" className="filter-label">Filter by Vehicle Type:</label>
                    <select
                        id="filter"
                        value={filter}
                        onChange={(e) => {
                            console.log("Selected filter:", e.target.value);  // Check what's selected
                            setFilter(e.target.value);  // Update the filter state
                        }} 
                        className="filter-dropdown"
                    >
                        <option value="">All</option>
                        <option value="Car">Car</option>
                        <option value="Camper">Camper</option>
                        <option value="Caravan">Caravan</option>
                    </select>
                </div>

                <div className="car-sale-section">
                    <h1 className="title">Vehicles for Sale</h1>
                    <div className="car-grid">
                        {filteredVehicles.map((vehicle) => (
                            <div key={vehicle.frameNr} className="car-card">
                                <div className="car-blob">
                                    {vehicle.image ? (
                                        <img
                                            src={`data:image/jpeg;base64,${vehicle.image}`}
                                            alt={`${vehicle.brand || 'Unknown'} ${vehicle.type || ''}`}
                                        />
                                    ) : (
                                        <p>Image not available</p>
                                    )}
                                </div>
                                <div className="car-info">
                                    <h2 className="car-name">{`${vehicle.brand || 'Unknown'} ${vehicle.type || ''}`}</h2>
                                    <p className="car-price">{`$${vehicle.price}`}</p>
                                    <p className="car-description">{vehicle.description || 'No description available'}</p>
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

            <GeneralFooter />
        </>
    );
}

export default GeneralSalePage;
