import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import './PayPage.css';

function PayPage() {
    const location = useLocation();
    const navigate = useNavigate();
    const vehicle = location.state?.vehicle;

    const [userDetails, setUserDetails] = useState({
        name: "",
        email: "",
        phone: "",
        billingAddress: "",
        streetAddress: "",
        city: "",
        postalCode: "",
        rentalDates: [null, null],
    });

    const [errorMessage, setErrorMessage] = useState("");
    const [vehicleAvailable, setVehicleAvailable] = useState(null);
    const [fetchError, setFetchError] = useState(null);
    const [totalCost, setTotalCost] = useState(0);
    const [rentalDays, setRentalDays] = useState(0);

    const checkVehicleAvailability = async () => {
        if (!Vehicle || !Vehicle.FrameNr) {
            setFetchError("Invalid vehicle frame number.");
            setVehicleAvailable(false);
            return;
        }

        try {
            const response = await fetch(`http://localhost:5165/api/vehicle/CheckIfVehicleExists/${Vehicle.FrameNr}`);

            if (!response.ok) {
                throw new Error('Vehicle availability check failed');
            }

            const data = await response.json();
            setVehicleAvailable(data.isAvailable);
        } catch (error) {
            setFetchError('There was an error checking vehicle availability. Please try again later.');
            setVehicleAvailable(false);
            console.error(error);
        }
    };

    useEffect(() => {
        if (vehicle) {
            console.log("Vehicle found:", vehicle);
            checkVehicleAvailability();
        }
    }, [vehicle]);

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setUserDetails((prevDetails) => ({ ...prevDetails, [name]: value }));
    };

    const handleDateChange = (dates) => {
        const [start, end] = dates;
        setUserDetails((prevDetails) => ({
            ...prevDetails,
            rentalDates: [start, end],
        }));

        if (start && end) {
            const startDate = new Date(start);
            const endDate = new Date(end);

            const calculatedRentalDays = Math.ceil((endDate - startDate) / (1000 * 60 * 60 * 24));

            const rawPrice = vehicle?.price || "0";
            const pricePerDay = parseFloat(rawPrice.replace(',', '.'));

            if (isNaN(pricePerDay)) {
                console.error("Invalid vehicle price:", vehicle?.price);
                setTotalCost(0);
                setRentalDays(0);
                return;
            }

            if (calculatedRentalDays > 0) {
                setRentalDays(calculatedRentalDays);
                setTotalCost(calculatedRentalDays * pricePerDay);
            } else {
                setRentalDays(0);
                setTotalCost(0);
            }
        } else {
            setRentalDays(0);
            setTotalCost(0);
        }
    };

    const handlePurchase = async () => {
        if (rentalDays > 0 && totalCost > 0) {
            const rentalData = {
                CustomerName: userDetails.name,
                Email: userDetails.email,
                Phone: userDetails.phone,
                BillingAddress: userDetails.billingAddress,
                StreetAddress: userDetails.streetAddress,
                City: userDetails.city,
                PostalCode: userDetails.postalCode,
                FrameNrCar: vehicle.FrameNr,
                StartDate: userDetails.rentalDates[0],
                EndDate: userDetails.rentalDates[1],
                Price: totalCost,
            };

            try {
                const response = await fetch('http://localhost:5165/api/Rental/CreateRental', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(rentalData),
                });

                if (!response.ok) {
                    throw new Error('Rental creation failed');
                }

                const data = await response.json();
                alert(`Rental confirmed! Total cost: €${totalCost.toFixed(2)}`);
                navigate('/');
            } catch (error) {
                setErrorMessage('Error creating rental. Please try again later.');
                console.error(error);
            }
        } else {
            setErrorMessage("Please enter a valid rental period.");
        }
    };

    if (!vehicle) {
        return (
            <div className="buy-page">
                <GeneralHeader />
                <div className="error-message">
                    <h2>Car not found!</h2>
                    <p>Please go back and select a vehicle.</p>
                </div>
                <GeneralFooter />
            </div>
        );
    }

    return (
        <div className="buy-page">
            <GeneralHeader />
            <div className="content">
                <h1 className="title">Confirm Rental</h1>
                <div className="buy-details">
                    <div className="car-info">
                        <h2 className="car-title">{`${vehicle.brand || "Unknown"} ${vehicle.type || "Model"}`}</h2>
                        <p className="car-price">{`Price: €${vehicle.price} per day`}</p>

                        <div className="car-image-container">
                            {vehicle.image ? (
                                <img
                                    src={`data:image/jpeg;base64,${vehicle.image}`}
                                    alt={`${vehicle.brand || "Unknown"} ${vehicle.type || ""}`}
                                    className="car-image"
                                />
                            ) : (
                                <p>Image not available</p>
                            )}
                        </div>
                    </div>
                    <div className="user-info">
                        <h3 className="user-info-title">Billing Address and Rental Period</h3>

                        <input
                            type="text"
                            name="name"
                            placeholder="Enter your full name"
                            value={userDetails.name}
                            onChange={handleInputChange}
                            className="input-field"
                        />

                        <input
                            type="text"
                            name="streetAddress"
                            placeholder="Street and House Number"
                            value={userDetails.streetAddress}
                            onChange={handleInputChange}
                            className="input-field"
                        />

                        <input
                            type="text"
                            name="city"
                            placeholder="City"
                            value={userDetails.city}
                            onChange={handleInputChange}
                            className="input-field"
                        />

                        <input
                            type="text"
                            name="postalCode"
                            placeholder="Postal Code"
                            value={userDetails.postalCode}
                            onChange={handleInputChange}
                            className="input-field"
                        />

                        <h3>Rental Period</h3>
                        <DatePicker
                            selected={userDetails.rentalDates[0]}
                            onChange={handleDateChange}
                            startDate={userDetails.rentalDates[0]}
                            endDate={userDetails.rentalDates[1]}
                            selectsRange
                            inline
                            dateFormat="yyyy/MM/dd"
                            placeholderText="Select start and end date"
                        />

                        <button
                            className="buy-button"
                            onClick={handlePurchase}
                        >
                            Confirm Rental
                        </button>

                        {errorMessage && <p className="error-message">{errorMessage}</p>}
                        {fetchError && <p className="error-message">{fetchError}</p>}
                    </div>
                </div>

                {totalCost > 0 && (
                    <div className="total-cost">
                        <h3>Total Cost:</h3>
                        <p>€{totalCost.toFixed(2)}</p>
                    </div>
                )}
            </div>
            <GeneralFooter />
        </div>
    );
}

export default PayPage;
