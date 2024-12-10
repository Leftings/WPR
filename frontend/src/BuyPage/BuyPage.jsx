import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import './BuyPage.css';

function BuyPage() {
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
            console.log("Vehicle object:", vehicle);
            checkVehicleAvailability();
        } else {
            console.error("Vehicle is undefined or missing required properties.");
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
        if (rentalDays > 0) {
            await sendConfirmationEmail();
            alert(`Verhuur succesvol! Aantal dagen: ${rentalDays} dagen, Totale kosten: €${totalCost.toFixed(2)}`);
            navigate("/");
        } else {
            setErrorMessage("Voer een geldige huurperiode in.");
        }
    };


    if (!vehicle) {
        return (
            <div className="buy-page">
                <GeneralHeader />
                <div className="error-message">
                    <h2>Auto niet gevonden!</h2>
                    <p>Ga terug en selecteer een voertuig.</p>
                </div>
                <GeneralFooter />
            </div>
        );
    }

    const sendConfirmationEmail = async () => {
        const rentalDatesString = userDetails.rentalDates
            .map((date) => date?.toISOString().split("T")[0]) // Format as 'YYYY-MM-DD'
            .join(" to ");

        const emailData = {
            email: userDetails.email,
            name: userDetails.name,
            rentalDates: rentalDatesString,
            totalCost: totalCost,
        };

        try {
            const response = await fetch("http://localhost:5165/api/Vehicle/SendConfirmationEmail", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(emailData),
            });

            if (!response.ok) {
                throw new Error("Failed to send confirmation email.");
            }

            console.log("Confirmation email sent successfully.");
        } catch (error) {
            console.error("Error sending confirmation email:", error);
            setErrorMessage("Could not send confirmation email. Please try again.");
        }
    };

    return (
        <div className="buy-page">
            <GeneralHeader />
            <div className="content">
                <h1 className="title">Bevestig Rental</h1>
                <div className="buy-details">
                    <div className="car-info">
                        <h2 className="car-title">{`${vehicle.brand || "Onbekend"} ${vehicle.type || "Model"}`}</h2>
                        <p className="car-price">{`Prijs: €${vehicle.price} per dag`}</p>

                        <div className="car-image-container">
                            {vehicle.image ? (
                                <img
                                    src={`data:image/jpeg;base64,${vehicle.image}`}
                                    alt={`${vehicle.brand || "Onbekend"} ${vehicle.type || ""}`}
                                    className="car-image"
                                />
                            ) : (
                                <p>Afbeelding niet beschikbaar</p>
                            )}
                        </div>
                    </div>
                    <div className="user-info">
                        <h3 className="user-info-title">Factuuradres en Huurperiode</h3>

                        <input
                            type="email"
                            name="email"
                            placeholder="Voer uw e-mailadres in"
                            value={userDetails.email}
                            onChange={handleInputChange}
                            className="input-field"
                        />

                        <input
                            type="text"
                            name="streetAddress"
                            placeholder="Straat en Huisnummer"
                            value={userDetails.streetAddress}
                            onChange={handleInputChange}
                            className="input-field"
                        />

                        <input
                            type="text"
                            name="city"
                            placeholder="Stad"
                            value={userDetails.city}
                            onChange={handleInputChange}
                            className="input-field"
                        />

                        <input
                            type="text"
                            name="postalCode"
                            placeholder="Postcode"
                            value={userDetails.postalCode}
                            onChange={handleInputChange}
                            className="input-field"
                        />

                        <h3>Huurperiode</h3>
                        <DatePicker
                            selected={userDetails.rentalDates[0]}
                            onChange={handleDateChange}
                            startDate={userDetails.rentalDates[0]}
                            endDate={userDetails.rentalDates[1]}
                            selectsRange
                            inline
                            dateFormat="yyyy/MM/dd"
                            placeholderText="Selecteer start- en einddatum"
                        />

                        <button
                            className="buy-button"
                            onClick={handlePurchase}
                        >
                            Bevestig Rental
                        </button>

                        {errorMessage && <p className="error-message">{errorMessage}</p>}
                        {fetchError && <p className="error-message">{fetchError}</p>}
                    </div>
                </div>

                {totalCost > 0 && (
                    <div className="total-cost">
                        <h3>Totaal Kosten:</h3>
                        <p>€{totalCost.toFixed(2)}</p>
                    </div>
                )}
            </div>
            <GeneralFooter />
        </div>
    );
}

export default BuyPage;
