import React, { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './GeneralSalePage.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function GetVehicle(id)
{
    // Individueel voertuig laden
    return fetch(`${BACKEND_URL}/api/Vehicle/GetVehicelData?frameNr=${id}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    })
    .then((response) => {
        if (!response.ok) {
          return response.json().then(data => {
            throw new Error(data?.message); 
          });
        }
        return response.json();
      })
      .then((data) => {
        // Voertuig data omzetten naar een list
        const combinedData = data?.message?.reduce((acc, item) => {
            const [key, value] = Object.entries(item)[0];
            acc[key] = value;
            return acc;
        }, {});

        return { message: combinedData };
    })
    .catch((error) => {
        console.error(error);
        return null;
      });
}

function GeneralSalePage() {
    const [vehicles, setVehicles] = useState([]);
    const [isEmployee, setIsEmployee] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);
    const [filter, setFilter] = useState('');
    const [loadingRequests, SetLoadingRequests] = useState({});

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
                    setFilter('Car');
                }
            } catch (error) {
                console.error(error.message);
                setIsEmployee(false);
            }
        };

        checkIfEmployee();
    }, []);

    /*
    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
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
            } finally {
                setLoading(false);
            }
        };

        if (isEmployee !== null) {
            fetchData();
        }
    }, [isEmployee, filter]); // Trigger fetching when `isEmployee` or `filter` changes
    */
    

    useEffect(() => {
        if (isEmployee === null) return; 
        const fetchVehicles = async () => {
            try
            {
                setLoading(true)
                console.log(isEmployee);
                setVehicles([]);
                let url;

                if (isEmployee) {
                    // Gebruiker kan alleen auto's zien
                    url = `${BACKEND_URL}/api/vehicle/GetFrameNumbersSpecificType?type=Car`;
                } else if (!filter || filter === 'All') {
                    // Alle voertuigen worden geladen voor de gebruiker
                    url = `${BACKEND_URL}/api/vehicle/GetFrameNumbers`;
                } else {
                    // Alleen de voertuigen van het geselecteerde type worden voor de gebruiker geladen
                    url = `${BACKEND_URL}/api/vehicle/GetFrameNumbersSpecificType?type=${encodeURIComponent(filter)}`;
                }

                const response = await fetch(url, {
                    method: 'GET',
                    credentials: 'include'
                });

                if (!response.ok)
                {
                    throw new Error('Failed to fetch new request');
                }

                const data = await response.json();
                const requestsToLoad = data?.message || [];
                
                // Er wordt door elk voertuig id heen gegaan
                requestsToLoad.forEach(async (id, index) => {
                    // Laden voor voertuig wordt aangezet
                    SetLoadingRequests((prevState) => ({ ...prevState, [id]: true }));
                
                    try {
                        const vehicle = await GetVehicle(id);
                
                        if (vehicle?.message) {
                            // Voertuig wordt toegevoegd aan voertuigen
                            setVehicles((prevRequest) => [...prevRequest, vehicle.message]);
                            // Laden voor voertuig wordt uitgezet
                            SetLoadingRequests((prevState) => ({ ...prevState, [id]: false }));
                            // Algemene laadpagina wordt uigezet
                            setLoading(false);
                        }
                    } catch (err) {
                        console.error(`Failed to fetch vehicle for ID ${id}:`, err);
                    }
                });
            } catch (error) {
                setError(error.message || 'An unexpected error occurred');
            } finally {
                setLoading(false);
            }
        };

        if (isEmployee !== null) {
            fetchVehicles();
        }
    }, [isEmployee, filter])
    

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
                    <h1 className="title-text">Voertuigen</h1>
                    {loading ? (
                        <div className="loading-spinner"></div>
                    ) : (
                        <div className="car-grid">
                            {vehicles.length > 0 ? (
                                vehicles.map(vehicle => (
                                    <div key={vehicle.FrameNr} className="car-card">
                                        <div className="car-blob">
                                            {vehicle.VehicleBlob ? (
                                                <img
                                                    className="car-blob"
                                                    src={`data:image/jpeg;base64,${vehicle.VehicleBlob}`}
                                                    alt={`${vehicle.Brand || 'Unknown'} ${vehicle.Type || ''}`}
                                                />
                                            ) : (
                                                <p>Image not available</p>
                                            )}
                                        </div>
                                        <div className="car-info">
                                            <h2 className="car-name">{`${vehicle.Brand || 'Unknown'} ${vehicle.Type || ''}`}</h2>
                                            <p className="car-price">{`$${vehicle.Price}`}</p>
                                            <p className="car-description">{vehicle.Description || 'No description available'}</p>
                                        </div>
                                        <Link
                                            to={`/vehicle/${vehicle.FrameNr}`}
                                            state={{ vehicle }}
                                            className="huur-link"
                                        >
                                            View Details
                                        </Link>
                                    </div>
                                ))
                            ) : (
                                <p>No vehicles found for the selected filter.</p>
                            )}
                        </div>
                    )}
                </div>
            </div>
            <GeneralFooter />
        </>
    );

}

export default GeneralSalePage;
