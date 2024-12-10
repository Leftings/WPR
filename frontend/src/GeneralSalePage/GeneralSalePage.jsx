import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './GeneralSalePage.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function GeneralSalePage() {
    const [vehicles, setVehicles] = useState([]);
    const [filter, setFilter] = useState('');

    // Fetch vehicles based on selected filter
    const fetchVehicles = async () => {
        try {
            let url;

            // If no filter is selected or "All" is selected, fetch all vehicles
            if (filter === '' || filter === 'All') {
                url = `${BACKEND_URL}/api/vehicle/GetAllVehicles`;  // Make sure this endpoint returns all vehicles
            } else {
                // If a filter is selected, fetch vehicles of that type
                url = `${BACKEND_URL}/api/vehicle/GetTypeOfVehicles?vehicleType=${encodeURIComponent(filter)}`;
            }

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
        fetchVehicles(); // Re-fetch vehicles whenever the filter changes
    }, [filter]);

    return (
        <>
            <GeneralHeader />
            <div className="general-sale-page">
                <div className="filter-section">
                    <label htmlFor="filter">Filter by Vehicle Type:</label>
                    <select
                        id="filter"
                        value={filter}
                        onChange={(e) => setFilter(e.target.value)} // Update filter state
                    >
                        <option value="All">All</option>  
                        <option value="Car">Car</option>
                        <option value="Camper">Camper</option>
                        <option value="Caravan">Caravan</option>
                    </select>
                </div>

                <div className="car-sale-section">
                    <h1 className="title">Vehicles for Sale</h1>
                    <div className="car-grid">
                        {vehicles.length > 0 ? (
                            vehicles.map(vehicle => (
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
                            ))
                        ) : (
                            <p>No vehicles found for the selected filter.</p>
                        )}
                    </div>
                </div>
            </div>
            <GeneralFooter />
        </>
    );
}

export default GeneralSalePage;
