import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './GeneralSalePage.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function GeneralSalePage() {
    const [vehicles, setVehicles] = useState([]);
    const [isEmployee, setIsEmployee] = useState(null);
    const [error, setError] = useState(null);
    const [filter, setFilter] = useState('');

    useEffect(() => {
        const checkIfEmployee = async () => {
            try {
                const response = await fetch(`${BACKEND_URL}/api/Employee/IsUserEmployee`, { credentials: 'include' });
                if (!response.ok) {
                    throw new Error('Error validating user type');
                }
                const data = await response.text();
                setIsEmployee(data === 'true');

                if (data === 'true') {
                    setFilter('Car'); // Default filter for employees
                }
            } catch (error) {
                console.error(error.message);
                setIsEmployee(false);
            }
        };

        checkIfEmployee();
    }, []);

    useEffect(() => {
        const fetchData = async () => {
            try {
                let url;

                if (isEmployee) {
                    // Employees always fetch cars
                    url = `${BACKEND_URL}/api/vehicle/GetTypeOfVehicles?vehicleType=Car`;
                } else if (!filter || filter === 'All') {
                    // Non-employees fetch all vehicles if no filter is applied
                    url = `${BACKEND_URL}/api/vehicle/GetAllVehicles`;
                } else {
                    // Non-employees fetch filtered vehicles
                    url = `${BACKEND_URL}/api/vehicle/GetTypeOfVehicles?vehicleType=${encodeURIComponent(filter)}`;
                }

                const response = await fetch(url);
                if (!response.ok) {
                    throw new Error(`Error fetching vehicles: ${response.statusText}`);
                }

                const data = await response.json();

                // Display vehicles one by one with a delay
                setVehicles([]); // Clear previous vehicles
                for (let vehicle of data) {
                    setVehicles((prev) => [...prev, vehicle]);
                    await new Promise((resolve) => setTimeout(resolve, 25));
                }
            } catch (error) {
                console.error('Failed to fetch vehicles:', error);
                setError('Failed to load vehicles');
            }
        };

        if (isEmployee !== null) {
            fetchData();
        }
    }, [isEmployee, filter]); // Trigger fetching when `isEmployee` or `filter` changes

    return (
        <>
            <GeneralHeader />
            <div className="general-sale-page">
                {!isEmployee && (
                    <div className="filter-section">
                        <label htmlFor="filter">Filter by Vehicle Type:</label>
                        <select
                            id="filter"
                            value={filter}
                            onChange={(e) => setFilter(e.target.value.trim())}
                        >
                            <option value="All">All</option>
                            <option value="Car">Car</option>
                            <option value="Camper">Camper</option>
                            <option value="Caravan">Caravan</option>
                        </select>
                    </div>
                )}

                <div className="car-sale-section">
                    <h1 className="title">Vehicles for Sale</h1>
                    <div className="car-grid">
                        {vehicles.length > 0 ? (
                            vehicles.map(vehicle => (
                                <div key={vehicle.frameNr} className="car-card">
                                    <div className="car-blob">
                                        {vehicle.image ? (
                                            <img
                                                className="car-blob"
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
