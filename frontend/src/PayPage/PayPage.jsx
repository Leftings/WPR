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

    console.log('Vehicle Data:', vehicle); // Add this line to debug vehicle data

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
    const [totalCost, setTotalCost] = useState(0);
    const [rentalDays, setRentalDays] = useState(0);

    // Fetch user details directly from the API
    const fetchUserDetails = async () => {
        try {
            const response = await fetch('http://localhost:5165/api/User/GetUserDetails', {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include', // Only include credentials if necessary for authentication
            });

            if (!response.ok) throw new Error('Failed to fetch user details');

            const userData = await response.json();
            setUserDetails((prevDetails) => ({
                ...prevDetails,
                name: userData.name,
                email: userData.email || "",
                phone: userData.phone,
                billingAddress: userData.billingAddress,
                streetAddress: userData.streetAddress,
                city: userData.city,
                postalCode: userData.postalCode,
            }));
        } catch (error) {
            console.error("Error fetching user details:", error);
            setErrorMessage("Error fetching user details.");
        }
    };

    useEffect(() => {
        fetchUserDetails(); // Fetch user details on component mount
    }, []);

    const handleDateChange = (dates) => {
        const [start, end] = dates;
        setUserDetails((prevDetails) => ({ ...prevDetails, rentalDates: [start, end] }));

        if (start && end) {
            const days = Math.ceil((end - start) / (1000 * 60 * 60 * 24));
            const pricePerDay = parseFloat(vehicle?.price.replace(',', '.') || "0");

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
    };

    const handlePurchase = async () => {
        if (!userDetails.email || !userDetails.rentalDates[0] || !userDetails.rentalDates[1]) {
            setErrorMessage("Please complete all required fields.");
            return;
        }

        const rentalData = {
            FrameNrCar: Vehicle?.FrameNr, // Corrected property name
            StartDate: userDetails.rentalDates[0].toISOString(),
            EndDate: userDetails.rentalDates[1].toISOString(),
            Price: totalCost,
            Email: userDetails.email,
        };

        console.log("Rental Data Sent:", rentalData);

        try {
            const response = await fetch("http://localhost:5165/api/Rental/CreateRental", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(rentalData),
            });

            if (!response.ok) {
                const data = await response.json();
                console.error("Error Response Data:", data);
                setErrorMessage(`Error: ${data.message}`);
                return;
            }

            const data = await response.json();
            console.log("Rental created:", data.message);
            navigate("/confirmation", { state: { rental: data } });
        } catch (error) {
            console.error("Error creating rental:", error);
            setErrorMessage("There was an error processing your rental. Please try again.");
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
                    <div className="user-info">
                        <h3>Billing Address and Rental Period</h3>
                        <input type="text" name="name" placeholder="Enter your full name" value={userDetails.name} onChange={(e) => setUserDetails({ ...userDetails, name: e.target.value })} className="input-field" />
                        <input type="email" name="email" placeholder="Enter your email" value={userDetails.email} onChange={(e) => setUserDetails({ ...userDetails, email: e.target.value })} className="input-field" />
                        <DatePicker selected={userDetails.rentalDates[0]} onChange={handleDateChange} startDate={userDetails.rentalDates[0]} endDate={userDetails.rentalDates[1]} selectsRange inline />
                        <div className="total"><p>Total: €{totalCost.toFixed(2)}</p></div>
                    </div>
                    <button onClick={handlePurchase} className="purchase-btn">Confirm Rental</button>
                </div>
                {errorMessage && <p className="error-message">{errorMessage}</p>}
            </div>
            <GeneralFooter />
        </div>
    );
}

export default PayPage;
