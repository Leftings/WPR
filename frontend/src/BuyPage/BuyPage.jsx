import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
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
    });

    const [errorMessage, setErrorMessage] = useState("");
    const [selectedCar, setSelectedCar] = useState("Select car");

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setUserDetails((prevDetails) => ({ ...prevDetails, [name]: value }));
    };

    const handlePurchase = async () => {
        try {
            const response = await fetch("http://localhost:5165/api/purchase", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    vehicleId: vehicle.frameNr,
                    ...userDetails,
                }),
            });

            if (!response.ok) {
                throw new Error("Verhuur mislukt. Probeer het opnieuw.");
            }

            alert("Verhuur succesvol!");
            navigate("/");
        } catch (error) {
            console.error(error);
            setErrorMessage("Huring kon niet worden voltooid. Probeer het opnieuw.");
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
                        <p className="car-price">{`Prijs: $${vehicle.price}`}</p>
                        <img
                            src={`data:image/jpeg;base64,${vehicle.image || ""}`}
                            alt={`${vehicle.brand || "Onbekend"} ${vehicle.type || ""}`}
                            className="car-image"
                        />
                    </div>
                    <div className="user-info">
                        <h3 className="user-info-title">Uw Gegevens</h3>
                        <input
                            type="text"
                            name="name"
                            placeholder="Vul uw naam in"
                            value={userDetails.name}
                            onChange={handleInputChange}
                            className="input-field"
                        />
                        <input
                            type="tel"
                            name="phone"
                            placeholder="Vul uw telefoonnummer in"
                            value={userDetails.phone}
                            onChange={handleInputChange}
                            className="input-field"
                        />
                        <CustomSelect
                            options={["Select Payment Type", "Pre-paid", "Pay as you go"]}
                            onChange={(value) => setSelectedPaymentMethod(value)}
                        />
                        <button className="buy-button" onClick={handlePurchase}>
                            Bevestig Rental
                        </button>
                        {errorMessage && <p className="error-message">{errorMessage}</p>}
                    </div>
                </div>
            </div>
            <GeneralFooter />
        </div>
    );
}

export default BuyPage;
