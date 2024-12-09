import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import './BuyPage.css';

function CustomSelect({ options, onChange }) {
    const [selected, setSelected] = useState(options[0]);
    const [isOpen, setIsOpen] = useState(false);

    const handleSelect = (option) => {
        setSelected(option);
        setIsOpen(false);
        onChange(option);
    };

    return (
        <div className="custom-select">
            <div
                className={`select-selected ${isOpen ? "select-arrow-active" : ""}`}
                onClick={() => setIsOpen((prev) => !prev)}
            >
                {selected}
            </div>
            {isOpen && (
                <div className="select-items">
                    {options.map((option, index) => (
                        <div
                            key={index}
                            className={`select-option ${option === selected ? "same-as-selected" : ""}`}
                            onClick={() => handleSelect(option)}
                        >
                            {option}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

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
    const [selectedPaymentMethod, setSelectedPaymentMethod] = useState("Select Payment Type");
    const [vehicleAvailable, setVehicleAvailable] = useState(true);
    const [fetchError, setFetchError] = useState(null);

    const checkVehicleAvailability = async () => {
        if (!vehicle || !vehicle.frameNr) {
            setFetchError("Invalid vehicle frame number.");
            return;
        }

        console.log("Vehicle FrameNr:", Vehicle.FrameNr);  // Log to confirm the vehicle frameNr

        try {
            const response = await fetch(`http://localhost:5165/api/vehicle/CheckIfVehicleExists/${Vehicle.FrameNr}`);
            console.log(response); 
            if (!response.ok) {
                throw new Error('Vehicle availability check failed');
            }

            const data = await response.json();
            console.log(data);  // Log the response data

            setVehicleAvailable(data.isAvailable);  // Ensure this is the expected response structure
        } catch (error) {
            setFetchError('There was an error checking vehicle availability. Please try again later.');
            console.error(error);
        }
    };

    useEffect(() => {
        if (vehicle) {
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
    };

    const handlePurchase = () => {
        if (!vehicleAvailable) {
            setErrorMessage("Deze auto is momenteel niet beschikbaar.");
            return;
        }

        const [startDate, endDate] = userDetails.rentalDates;
        if (!startDate || !endDate) {
            setErrorMessage("Selecteer zowel een start- als einddatum.");
            return;
        }

        const rentalDays = (endDate - startDate) / (1000 * 3600 * 24);

        if (rentalDays <= 0) {
            setErrorMessage("De einddatum moet na de startdatum liggen.");
            return;
        }

        alert(`Verhuur succesvol! Aantal dagen: ${rentalDays} dagen`);
        navigate("/");
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
                        <p className="car-price">{`Prijs: $${vehicle.price}`}</p>

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

                        <CustomSelect
                            options={["Select Payment Type", "Pre-paid", "Pay as you go"]}
                            onChange={(value) => setSelectedPaymentMethod(value)}
                        />
                        <button className="buy-button" onClick={handlePurchase}>
                            Bevestig Rental
                        </button>

                        {errorMessage && <p className="error-message">{errorMessage}</p>}
                        {fetchError && <p className="error-message">{fetchError}</p>}
                    </div>
                </div>
            </div>
            <GeneralFooter />
        </div>
    );
}

export default BuyPage;
