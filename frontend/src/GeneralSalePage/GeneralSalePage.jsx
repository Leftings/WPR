import React, { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
//import './GeneralSalePage.css';
import '../index.css';
import { sorter, sorterArray, sorterOneItem, sorterOneItemNumber } from '../utils/sorter.js';

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
    const [loadingRequests, SetLoadingRequests] = useState({});
    const [isFiltersOpen, setIsFiltersOpen] = useState(false);
    const [filterOptions, setFilterOptions] = useState({});
    const [cars, setCars] = useState([]);
    const [campers, setCampers] = useState([]);
    const [caravans, setCaravans] = useState([]);

    const [showColorFilters, setShowColorFilters] = useState(false);
    const [showBrandFilters, setShowBrandFilters] = useState(false);
    const [showTypesFilters, setShowTypesFilters] = useState(false);
    const [showSeatsFilters, setShowSeatsFilters] = useState(false);

    const toggleFilters = () => {
        setIsFiltersOpen(!isFiltersOpen);
    };
    
    const [filters, setFilters] = useState({
        vehicleTypes: [],
        color: [],
        brand: [],
        seat: []
    });

    const getUniqueFilterOptions = (vehicles) => {
        const uniqueSort = [...new Set(vehicles.map(vehicle => vehicle.Sort))];
        const uniqueBrand = sorterOneItem([...new Set(vehicles.map(vehicle => vehicle.Brand))], 'Low');
        const uniqueColor = sorterOneItem([...new Set(vehicles.map(vehicle => vehicle.Color))], 'Low');
        const uniqueSeats = sorterOneItemNumber([...new Set(vehicles.map(vehicle => vehicle.Seats))], 'Low');
        
        setFilterOptions({
           Sort: uniqueSort,
           Brand: uniqueBrand,
           Color: uniqueColor,
           Seats: uniqueSeats, 
        });
    }
    
    const filteredVehicles = vehicles.filter((vehicle) => {
        const matchesVehicleTypes =
            filters.vehicleTypes.length === 0 || filters.vehicleTypes.includes(vehicle.Sort);
        const matchesColor =
            filters.color.length === 0 || filters.color.includes(vehicle.Color);
        const matchesBrand =
            filters.brand.length === 0 || filters.brand.includes(vehicle.Brand);
        const matchesSeat =
            filters.seat.length === 0 || filters.seat.includes(vehicle.Seats);
    
        return matchesVehicleTypes && matchesBrand && matchesColor && matchesSeat;
    });
    
    const availableBrands = sorterOneItem([...new Set(vehicles
        .filter(vehicle => filters.vehicleTypes.length === 0 || filters.vehicleTypes.includes(vehicle.Sort))
        .map(vehicle => vehicle.Brand)
    )], 'Low');
    

    

    const handleFilterChange = (category, value) => {
        setFilters((prevFilters) => {
            if (category === "vehicleTypes") {
                // For vehicleTypes, allow toggling off the selected type
                return {
                    ...prevFilters,
                    vehicleTypes: prevFilters.vehicleTypes.includes(value)
                        ? [] // If already selected, deselect it
                        : [value] // Otherwise, select it
                };
            } else {
                // For other categories, toggle the filter
                const updatedCategory = prevFilters[category].includes(value)
                    ? prevFilters[category].filter((v) => v !== value) // Remove the value
                    : [...prevFilters[category], value]; // Add the value
                
                return { ...prevFilters, [category]: updatedCategory };
            }
        });
    };    

    useEffect(() => {
        getUniqueFilterOptions(vehicles);
    }, [filters.vehicleTypes, vehicles]);
    

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
                    setFilters(prevFilters => ({
                        ...prevFilters,
                        vehicleTypes: ['Car']
                    }));
                }
            } catch (error) {
                console.error(error.message);
                setIsEmployee(false);
            }
        };

        checkIfEmployee();
    }, []);

    useEffect(() => {
        if (isEmployee === null) return; 
        const fetchVehicles = async () => {
            try
            {
                setLoading(true)
                console.log(isEmployee);
                setVehicles([]);
                let url;
                
                url = `${BACKEND_URL}/api/vehicle/GetFrameNumbers`;

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
                            setVehicles((prevVehicles) => {
                                const updatedVehicles = [...prevVehicles, vehicle.message];
                                return sorterArray(updatedVehicles, 'Sort');});
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
                setVehicles(sorter(vehicles, 'Sort', 'Low'));
            }
        };

        if (isEmployee !== null) {
            fetchVehicles();
        }
    }, [isEmployee])

    useEffect(() => {
        if (isFiltersOpen) {
            document.body.style.overflow = 'hidden';
        } else {
            document.body.style.overflow = 'auto';
        }

        return () => {
            document.body.style.overflow = 'auto';
        };
    }, [isFiltersOpen]);

    return (
        <>
            <div className={`filter-bar ${isFiltersOpen ? 'open' : ''}`}>
                <h2 className="filter-bar-title">
                    Filters 
                    <span className="filter-bar-exit" onClick={toggleFilters}><i className="fas fa-times" /></span></h2>
                <hr/>

                {!isEmployee && (
                    <>
                    <div className="filter-section">
                        <p onClick={() => setShowTypesFilters(!showTypesFilters)}>Soort voertuig 
                            <span className={`toggle-icon ${showTypesFilters ? 'rotated' : ''}`}>+</span>
                        </p>
                        {filterOptions.Sort && filterOptions.Sort.length > 0 && (
                            <div className={`filter-types ${showTypesFilters ? 'show' : ''}`}>
                            {filterOptions.Sort.map((vehicleType) => (
                                    <div key={vehicleType} className="checkbox-item">
                                        <input
                                            type="checkbox"
                                            id={vehicleType}
                                            value={vehicleType}
                                            name={vehicleType}
                                            checked={filters.vehicleTypes.includes(vehicleType)}
                                            onChange={() => handleFilterChange("vehicleTypes", vehicleType)}
                                        />
                                        <label htmlFor={vehicleType}>{vehicleType}</label>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                    <hr/>
                    </>
                )}

                {filters.vehicleTypes.length > 0 && availableBrands.length > 0 && (
                    <div className="filter-section">
                        <div className="filter-section">
                            <p onClick={() => setShowBrandFilters(!showBrandFilters)}>
                                Merk
                                <span className={`toggle-icon ${showBrandFilters ? 'rotated' : ''}`}>+</span>
                            </p>
                            {showBrandFilters && (
                                <div className={`filter-types show`}>
                                    {availableBrands.map((brand) => (
                                        <div key={brand} className="checkbox-item">
                                            <input
                                                type="checkbox"
                                                id={brand}
                                                value={brand}
                                                checked={filters.brand.includes(brand)}
                                                name={brand}
                                                onChange={() => handleFilterChange("brand", brand)}
                                            />
                                            <label htmlFor={brand}>{brand}</label>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                        <hr/>
                    </div> 
                )}



                <div className="filter-section">
                    <p onClick={() => setShowColorFilters(!showColorFilters)}>Kleur
                        <span className={`toggle-icon ${showColorFilters ? 'rotated' : ''}`}>+</span>
                    </p>
                    {filterOptions.Color && filterOptions.Color.length > 0 && (
                        <div className={`filter-types ${showColorFilters ? 'show' : ''}`}>
                            {filterOptions.Color.map((color) => (
                                <div key={color} className="checkbox-item">
                                    <input
                                        type="checkbox"
                                        id={color}
                                        value={color}
                                        checked={filters.color.includes(color)}
                                        name={color}
                                        onChange={() => handleFilterChange("color", color)}
                                    />
                                    <label htmlFor={color}>{color}</label>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
                <hr/>

                <div className="filter-section">
                    <p onClick={() => setShowSeatsFilters(!showSeatsFilters)}>Aantal passagiers
                        <span className={`toggle-icon ${showSeatsFilters ? 'rotated' : ''}`}>+</span>
                    </p>
                    {filterOptions.Seats && filterOptions.Seats.length > 0 && (
                        <div className={`filter-types ${showSeatsFilters ? 'show' : ''}`}>
                            {filterOptions.Seats.map((seat) => (
                                <div key={seat} className="checkbox-item">
                                    <input
                                        type="checkbox"
                                        id={seat}
                                        value={seat}
                                        checked={filters.seat.includes(seat)}
                                        name={seat}
                                        onChange={() => handleFilterChange("seat", seat)}
                                    />
                                    <label htmlFor={seat}>{seat}</label>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
                <div className="filter-section">
                    <div className="filter-spacer"></div>
                </div>
            </div>

            {isFiltersOpen && <div className="overlay" onClick={toggleFilters}></div>}

            <GeneralHeader/>
            <div className="general-sale-page">

                <div className="car-sale-section">
                    <h1 className="title-text">Voertuigen</h1>
                    <button htmlFor="filter" onClick={toggleFilters} className="filter-button"><i
                        className="fas fa-filter"></i> Filter
                    </button>
                    {loading ? (
                        <div className="loading-spinner"></div>
                    ) : (
                        <div className="car-grid">
                            {filteredVehicles.length > 0 ? (
                                filteredVehicles.map(vehicle => (
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
                                            state={{vehicle}}
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
            <GeneralFooter/>
        </>
    );

}

export default GeneralSalePage;
