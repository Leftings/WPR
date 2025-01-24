import React, { useState, useEffect } from 'react';
import {useLocation, useNavigate} from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import '../index.css';
import CarRentalOverview from "../CarRentalOverview/CarRentalOverview.jsx";
import DatePicker from "react-datepicker";

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function ChangeRental() {
    const [errorMessage, setErrorMessage] = useState("");
    const [dates, setDates] = useState([null, null]);
    const [basePrice, setBasePrice] = useState(null);
    const [totalCost, setTotalCost] = useState(0);
    const [rentalDays, setRentalDays] = useState(0);
    const navigate = useNavigate();
    const location = useLocation();
    const rental = location.state?.rental;

    const handleDateChange = (dates) => {
        setDates(dates);
        const [start, end] = dates;

        if (start && end) {
            const days = Math.ceil((end - start) / (1000 * 60 * 60 * 24));
            const pricePerDay = parseFloat(basePrice.replace(',', '.') || "0");

            if (!isNaN(pricePerDay) && days > 0) {
                setRentalDays(days);
                setTotalCost(days * pricePerDay);
            } else {
                setRentalDays(0);
                setTotalCost(0);
            }
        } else {
            setRentalDays(0);
            setTotalCost(0);
        }
        
        console.log(rental.id);
    };

    const handleWijziging = async () => {
        try {
            const response = await fetch(`${BACKEND_URL}/api/Rental/ChangeRental`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    Id: rental.id,
                    StartDate: dates[0].toISOString(),
                    EndDate: dates[1].toISOString(),
                    Price: totalCost.toFixed(2),
                }),
            });

            if (!response.ok) {
                const textResponse = await response.text();
                console.error("Error response:", textResponse);
                setErrorMessage(`Error: ${textResponse}`);
                return;
            }
            
            setErrorMessage('Rental updated successfully!');
            navigate("/overviewRental")
        } catch (error) {
            console.error("Error when updating rental", error);
            setErrorMessage("Error when updating rental, please try again later.");
        }
    };
    
    if (!rental) {
        return <div>Geen rental meegegeven :(</div>
    }

    useEffect(() => {
        const fetchBasePrice = async () => {
            try {
                const response = await fetch(`${BACKEND_URL}/api/Vehicle/GetVehiclePriceAsync?frameNr=${rental.frameNrVehicle}`);
                const data = await response.text();
                if (response.ok) {
                    setBasePrice(data);
                } else {
                    setErrorMessage("Failed to fetch base price.");
                }
            } catch (error) {
                setErrorMessage("Error fetching base price.");
                console.error(error);
            }
        };
        fetchBasePrice();
    }, []);

    useEffect(() => {
        fetch(`${BACKEND_URL}/api/Cookie/GetUserId`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('No Cookie');
                }
                return response.json();
            })
            .then(data => {
                const id = data?.message;
            })
            .catch(() => {
                alert("Cookie was niet geldig");
                navigate('/');
            });
    }, [navigate]);
    
    return (

        <>
            <GeneralHeader />
            <div className="buy-details">
                <div className="car-info">
                    <h2 className="car-title">{`${rental.carName}`}</h2>
                    <p className="car-price">{`Prijs: €${basePrice} per dag`}</p>
                </div>
                <div className="user-info">
                    <h3 className="user-info-title">Huurperiode</h3>

                    <h3>Huurperiode</h3>
                    <div className="date-picker-container">
                        <p className="date-picker-label">Selecteer datumbereik:</p>
                        <DatePicker
                            selected={new Date()}
                            onChange={handleDateChange}
                            startDate={dates[0]}
                            endDate={dates[1]}
                            selectsRange
                            inline
                            dateFormat="yyyy/MM/dd"
                            placeholderText="Selecteer start- en einddatum"
                            minDate={new Date()}
                        />
                    </div>

                    <button
                        className="buy-button"
                        onClick={handleWijziging}
                    >
                        Bevestig wijziging
                    </button>

                    {errorMessage && <p className="error-message">{errorMessage}</p>}
                </div>
                {totalCost > 0 && (
                    <div className="total-cost">
                        <h3>Totaalbedrag:</h3>
                        <p>€{totalCost.toFixed(2)}</p>
                    </div>
                )}
            </div>
        </>
    );
}

export default ChangeRental;