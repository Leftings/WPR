import React, { useState, useEffect, useCallback } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import DatePicker from "react-datepicker";
import { ToastContainer, toast } from 'react-toastify';
import { sorter, sorterArray, sorterOneItem, sorterOneItemNumber } from '../utils/sorter.js';
import 'react-toastify/dist/ReactToastify.css';
import "react-datepicker/dist/react-datepicker.css";

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

/**
 * Fetches detailed data about a vehicle by frame number.
 * @param {string} id - The frame number of the vehicle.
 * @returns {Promise<object|null>} - The vehicle data or null on error.
 */
function GetVehicle(id) {
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
            // Combine key-value pairs for easier access
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

/**
 * Main component for managing the general sales page with vehicle filtering.
 */
function GeneralSalePage() {
    // State variables for managing vehicles and user-related data
    const [vehicles, setVehicles] = useState([]); // List of vehicles
    const [isEmployee, setIsEmployee] = useState(null); // Employee status
    const [error, setError] = useState(null); // API or UI errors
    const [loading, setLoading] = useState(false); // General loading state
    const [loadingRequests, SetLoadingRequests] = useState({}); // Tracks ongoing requests
    const [isFiltersOpen, setIsFiltersOpen] = useState(false); // Toggles filter dropdown
    const [filterOptions, setFilterOptions] = useState({}); // Dynamic filter options
    const [isStaff, setIsStaff] = useState(false); // Staff role
    const [officeType, setOfficeType] = useState(null); // Office type (front or back)
    const [isFrontOffice, setIsFrontOffice] = useState(false); // Front-office flag
    const navigate = useNavigate(); // Navigation hook
    const [cars, setCars] = useState([]); // Subset of vehicles: cars
    const [rentals, setRentals] = useState([]); // Rental data
    const [campers, setCampers] = useState([]); // Subset of vehicles: campers
    const [caravans, setCaravans] = useState([]); // Subset of vehicles: caravans

    // State variables for managing individual filter dropdowns
    const [showColorFilters, setShowColorFilters] = useState(false);
    const [showBrandFilters, setShowBrandFilters] = useState(false);
    const [showTypesFilters, setShowTypesFilters] = useState(false);
    const [showSeatsFilters, setShowSeatsFilters] = useState(false);

    // Toggles the entire filter section visibility
    const toggleFilters = () => {
        setIsFiltersOpen(!isFiltersOpen);
    };

    // State for active filters
    const [filters, setFilters] = useState({
        startDate: new Date(new Date().setDate(new Date().getDate() + 1)), // Default to tomorrow
        endDate: null, // No default end date
        vehicleTypes: [], // Selected vehicle types
        brand: [], // Selected brands
        color: [], // Selected colors
        seat: [], // Selected seat counts
    });

    /**
     * Updates the date range filters based on user input.
     * @param {Array<Date>} dates - Selected start and end dates.
     */
    const handleDateFilterChange = (dates) => {
        const [start, end] = dates;
        setFilters(prevFilters => ({
            ...prevFilters,
            startDate: start,
            endDate: end
        }));
    };

    /**
     * Extracts unique options for filters from the list of vehicles.
     * @param {Array} vehicles - Array of vehicle objects.
     */
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
    };

    /**
     * Filters the list of vehicles based on the active filters and rental availability.
     * @returns {Array} - List of vehicles that match the filters.
     */
    const filteredVehicles = vehicles.filter(vehicle => {
        console.log(`Evaluating Vehicle ${vehicle.FrameNr}`);

        // Check if vehicle matches the active filters
        const matchesVehicleTypes = filters.vehicleTypes.length === 0 || filters.vehicleTypes.includes(vehicle.Sort);
        const matchesColor = filters.color.length === 0 || filters.color.includes(vehicle.Color);
        const matchesBrand = filters.brand.length === 0 || filters.brand.includes(vehicle.Brand);
        const matchesSeat = filters.seat.length === 0 || filters.seat.includes(vehicle.Seats);

        // Parse date filters
        const startDate = filters.startDate ? new Date(filters.startDate) : null;
        const endDate = filters.endDate ? new Date(filters.endDate) : null;

        // Check rental availability for the current vehicle
        const vehicleRentals = rentals.filter(rental => String(rental.frameNrVehicle) === String(vehicle.FrameNr));
        console.log(`Vehicle ${vehicle.FrameNr} Rentals:`, vehicleRentals);

        const isRentedDuringSelectedDates = vehicleRentals.some(rental => {
            if (!rental.startDate || !rental.endDate) {
                console.log(`Rental for Vehicle ${vehicle.FrameNr} has invalid dates`);
                return false;
            }

            const rentalStart = new Date(rental.startDate);
            const rentalEnd = new Date(rental.endDate);

            console.log(`Rental Start: ${rentalStart}, Rental End: ${rentalEnd}`);
            console.log(`Selected Start: ${startDate}, Selected End: ${endDate}`);

            return (
                (startDate && endDate && startDate <= rentalEnd && endDate >= rentalStart) ||  // Full overlap
                (startDate && !endDate && startDate < rentalEnd) ||  // Start date overlap
                (!startDate && endDate && endDate > rentalStart) // End date overlap
            );
        });

        console.log(`Vehicle ${vehicle.FrameNr} ${isRentedDuringSelectedDates ? 'is' : 'is not'} rented during selected dates`);

        // Ensure the vehicle is not under repair
        const isNotInRepair = vehicle.InRepair === "False";

        // Return true if the vehicle passes all filters
        return matchesVehicleTypes && matchesBrand && matchesColor && matchesSeat && !isRentedDuringSelectedDates && isNotInRepair;
    });

    // Haal de lijst van beschikbare merken op op basis van de geselecteerde voertuigtypefilters
    const availableBrands = sorterOneItem(
        [
            ...new Set(
                vehicles
                    .filter(
                        vehicle => filters.vehicleTypes.length === 0 || filters.vehicleTypes.includes(vehicle.Sort)
                    )
                    .map(vehicle => vehicle.Brand)
            )
        ],
        'Low' // Sorteer in oplopende volgorde
    );

// Werk de merkfilteropties dynamisch bij wanneer de voertuigtypefilters of voertuiglijst wijzigen
    useEffect(() => {
        const updatedAvailableBrands = sorterOneItem(
            [
                ...new Set(
                    vehicles
                        .filter(
                            vehicle => filters.vehicleTypes.length === 0 || filters.vehicleTypes.includes(vehicle.Sort)
                        )
                        .map(vehicle => vehicle.Brand)
                )
            ],
            'Low' // Sorteer in oplopende volgorde
        );

        setFilterOptions(prev => ({
            ...prev,
            Brand: updatedAvailableBrands // Werk filteropties bij met de nieuwe lijst van merken
        }));
    }, [filters.vehicleTypes, vehicles]); // Afhankelijkheden: voertuigtypefilters en voertuigenlijst

// Weergave voor voertuigtypes in een meer gebruiksvriendelijk formaat
    const display = {
        Car: 'Auto', // Car wordt weergegeven als "Auto"
        Camper: 'Camper', // Camper wordt weergegeven als "Camper"
        Caravan: 'Caravan' // Caravan wordt weergegeven als "Caravan"
    };

// Verwerk wijzigingen in een filtercategorie (bijv. voertuigtypes, merken, kleuren)
    const handleFilterChange = (category, value) => {
        setFilters(prevFilters => {
            let updatedCategory;

            if (category === "vehicleTypes") {
                // Als een voertuigtype wordt geschakeld, reset of stel de categorie in
                updatedCategory = prevFilters.vehicleTypes.includes(value) ? [] : [value];
            } else {
                // Schakel de geselecteerde waarde in de filtercategorie
                updatedCategory = prevFilters[category].includes(value)
                    ? prevFilters[category].filter(v => v !== value)
                    : [...prevFilters[category], value];
            }

            // Als de categorie "vehicleTypes" is, reset de merkfilter
            if (category === "vehicleTypes") {
                return { ...prevFilters, vehicleTypes: updatedCategory, brand: [] };
            }

            // Werk de specifieke categorie filter bij
            return { ...prevFilters, [category]: updatedCategory };
        });
    };

// Zorg ervoor dat filteropties uniek en bijgewerkt blijven wanneer de voertuiglijst of voertuigtypefilters wijzigen
    useEffect(() => {
        getUniqueFilterOptions(vehicles);
    }, [filters.vehicleTypes, vehicles]); // Afhankelijkheden: voertuigtypefilters en voertuigenlijst

// Controleer of de gebruiker een werknemer is en stel hun toegang dienovereenkomstig in
    useEffect(() => {
        const checkIfEmployee = async () => {
            try {
                const response = await fetch(`${BACKEND_URL}/api/Employee/IsUserEmployee`, { credentials: 'include' });
                if (!response.ok) {
                    throw new Error('Fout bij het valideren van gebruikerstype');
                }
                const data = await response.text();
                setIsEmployee(data === 'true');

                if (data === 'true') {
                    // Standaard "Auto"-type voor werknemers
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
    }, []); // Voert uit bij component mount

// Controleer of de huidige gebruikerssessie toebehoort aan een medewerker (Front of Back Office)
    useEffect(() => {
        fetch('http://localhost:5165/api/Login/CheckSessionStaff', {
            credentials: 'include',
            method: 'GET',
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Niet een medewerker of sessie verlopen');
                }
                return response.json();
            })
            .then(data => {
                console.log('Backend Response:', data);

                if (data.officeType === 'Front') {
                    setIsStaff(true); // Markeer als medewerker
                    setIsFrontOffice(true); // Markeer als Front Office
                } else if (data.officeType === 'Back') {
                    setIsStaff(true); // Markeer als medewerker
                    setIsFrontOffice(false); // Markeer als Back Office
                } else {
                    setIsStaff(false); // Onverwacht geval
                }
            })
            .catch(error => {
                console.error('Fout bij het ophalen van sessie-informatie:', error);
                setIsStaff(false);
                setIsFrontOffice(false); // Standaard naar geen medewerker
            });
    }, []); // Voert uit bij component mount

// Verwerk het verwijderen van een voertuig
    const handleDelete = async frameNr => {
        const confirmDelete = window.confirm('Weet je zeker dat je dit voertuig wilt verwijderen?');
        if (!confirmDelete) return;

        try {
            const response = await fetch(`${BACKEND_URL}/api/vehicle/DeleteVehicle?frameNr=${frameNr}`, {
                method: 'DELETE',
                credentials: 'include'
            });

            if (!response.ok) {
                throw new Error('Fout bij het verwijderen van voertuig');
            }

            const data = await response.json();

            if (data.Status) {
                // Verwijder het voertuig uit de lokale state
                setVehicles(vehicles.filter(vehicle => vehicle.FrameNr !== frameNr));
                alert('Voertuig is verwijderd.'); // Geef succesmelding
                navigate('/vehicles'); // Redirect naar voertuigenpagina
            } else {
                alert(data.message); // Toon backend foutmelding
            }
        } catch (error) {
            console.error(error.message);
            alert('Fout bij het verwijderen van voertuig'); // Geef foutmelding
        }
    };

// Haal alle voertuigen op en vul de state op basis van gebruikerstype
    useEffect(() => {
        if (isEmployee === null) return; // Wacht tot de werknemersstatus is bepaald

        const fetchVehicles = async () => {
            try {
                setLoading(true); // Toon laadstatus
                console.log(isEmployee);
                setVehicles([]); // Wis voertuigenlijst
                let url = `${BACKEND_URL}/api/vehicle/GetFrameNumbers`;

                const response = await fetch(url, {
                    method: 'GET',
                    credentials: 'include'
                });

                if (!response.ok) {
                    throw new Error('Fout bij het ophalen van nieuw verzoek');
                }

                const data = await response.json();
                const requestsToLoad = data?.message || [];

                // Loop door elk voertuig-ID en haal de details op
                requestsToLoad.forEach(async id => {
                    SetLoadingRequests(prevState => ({ ...prevState, [id]: true })); // Start laden voor dit ID

                    try {
                        const vehicle = await GetVehicle(id);

                        if (vehicle?.message) {
                            // Voeg het voertuig toe aan de state en sorteer ze
                            setVehicles(prevVehicles => {
                                const updatedVehicles = [...prevVehicles, vehicle.message];
                                return sorterArray(updatedVehicles, 'Sort'); // Sorteer voertuigen
                            });
                            SetLoadingRequests(prevState => ({ ...prevState, [id]: false })); // Stop laden
                            setLoading(false); // Algemeen laden voltooid
                        }
                    } catch (err) {
                        console.error(`Fout bij het ophalen van voertuig voor ID ${id}:`, err);
                    }
                });
            } catch (error) {
                setError(error.message || 'Er is een onverwachte fout opgetreden'); // Geef foutmelding
            } finally {
                setLoading(false); // Laadstatus uit
                setVehicles(sorter(vehicles, 'Sort', 'Low')); // Sorteer voertuigen na voltooiing
            }
        };

        fetchVehicles();
    }, [isEmployee]); // Afhankelijkheid: werknemersstatus

    async function fetchAndLogRentals() {
        try {
            const response = await fetch(`${BACKEND_URL}/api/Rental/GetAllUserRentalsWithDetails`, {
                method: 'GET',
                headers: {'Content-Type': 'application/json'},
                credentials: 'include',
            });

            if (!response.ok) {
                throw new Error('Failed to fetch rentals');
            }

            const data = await response.json();
            setRentals(data); 
        } catch (error) {
            console.error('Error fetching rentals:', error);
        }
    }


    useEffect(() => {
        fetchAndLogRentals();
    }, []);

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
                    <span className="filter-bar-exit" onClick={toggleFilters}><i className="fas fa-times"/></span></h2>
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
                                        <label htmlFor={vehicleType}>{display[vehicleType]}</label>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                    <hr/>
                    </>
                    )}


                {filters.vehicleTypes.length > 0 && availableBrands.length > 0 && (
                    <>
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
                        <hr /> 
                    </>
                )}


                <div className="filter-section">
                    <p onClick={() => setShowColorFilters(!showColorFilters)}>Kleur
                        <span className={`toggle-icon ${showColorFilters ? 'rotated' : ''}`}>+</span>
                    </p>
                    {filterOptions.Sort && filterOptions.Sort.length > 0 && (
                        <div className={`filter-types ${showColorFilters ? 'show' : ''}`}>
                        {filterOptions.Color.map((color) => (
                            <div key={color} className="checkbox-item">
                                <input
                                    type="checkbox"
                                    id={color}
                                    value={color}
                                    checked={filters.color.includes(color)}
                                    onChange={() => handleFilterChange('color', color)}
                                />
                                <label htmlFor={color}>{color}</label>
                            </div>
                        ))}
                    </div>
                    )}
                </div>
                <hr/>
                {/* Aantal passagiers Filter */}
                <div className="filter-section">
                    <p onClick={() => setShowSeatsFilters(!showSeatsFilters)}>Aantal passagiers
                        <span className={`toggle-icon ${showSeatsFilters ? 'rotated' : ''}`}>+</span>
                    </p>
                    {filterOptions.Sort && filterOptions.Sort.length > 0 && (
                        <div className={`filter-types ${showSeatsFilters ? 'show' : ''}`}>
                        {filterOptions.Seats.map((seat)=> (
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
            </div>

            {isFiltersOpen && <div className="overlay" onClick={toggleFilters}></div>}

            <div className="general-sale-page">
                <GeneralHeader/>
                <div className="car-sale-section">
                    <h1 className="title-text">Voertuigen</h1>
                    
                    {!isStaff && (
                        <div className="date-picker-container">
                            <p className="date-picker-label">Selecteer datumbereik:</p>
                            <DatePicker
                                selected={filters.startDate}
                                onChange={(dates) => {
                                    const [start, end] = dates || [];
                                    setFilters((prevFilters) => ({
                                        ...prevFilters,
                                        startDate: start || null,
                                        endDate: end || null,
                                    }));
                                }}
                                startDate={filters.startDate}
                                endDate={filters.endDate}
                                selectsRange
                                inline
                                dateFormat="yyyy/MM/dd"
                                placeholderText="Selecteer start- en einddatum"
                                minDate={new Date()} 
                            />
                        </div>
                    )}

                    <button htmlFor="filter" onClick={toggleFilters} className="filter-button">
                        <i className="fas fa-filter"></i> Filter
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

                                        {isStaff && !isFrontOffice ? (
                                            <button
                                                onClick={() => handleDelete(vehicle.FrameNr)}
                                                className="cta-button"
                                            >
                                                Verwijder
                                            </button>
                                        ) : !isStaff ? (
                                            <Link
                                                to={`/vehicle/${vehicle.FrameNr}`}
                                                state={{
                                                    vehicle,
                                                    rentalDates: [filters.startDate, filters.endDate],
                                                }}
                                                className={`cta-button`}
                                                onClick={(e) => {
                                                    if (!filters.startDate || !filters.endDate) {
                                                        e.preventDefault();
                                                        toast.error('Selecteer alstublieft een begin- en einddatum voordat u een voertuig huurt.', {
                                                            position: "top-center",
                                                            autoClose: 3000,
                                                            hideProgressBar: false,
                                                            closeOnClick: true,
                                                            pauseOnHover: true,
                                                            draggable: true,
                                                            progress: undefined,
                                                        });
                                                    }
                                                }}
                                            >
                                                Huur
                                            </Link>
                                        ) : null}
                                    </div>
                                ))
                            ) : (
                                <p>Geen voertuigen gevonden...</p>
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