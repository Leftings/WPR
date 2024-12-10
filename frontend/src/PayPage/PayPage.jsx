import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import './PayPage.css';

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
            const calculatedRentalDays = (end - start) / (1000 * 3600 * 24);
            const calculatedCost = calculatedRentalDays * vehicle.price;

            setRentalDays(calculatedRentalDays);
            setTotalCost(calculatedCost);
        }
    };

    const handlePurchase = () => {
        if (rentalDays > 0 && totalCost > 0) {
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
                            type="text"
                            name="name"
                            placeholder="Vul uw volledige naam in"
                            value={userDetails.name}
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
