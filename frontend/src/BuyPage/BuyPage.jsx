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
        billingAddress: "",    
        rentalStartDate: "",
        rentalEndDate: "",
    });

    const [errorMessage, setErrorMessage] = useState("");
    const [selectedPaymentMethod, setSelectedPaymentMethod] = useState("Select Payment Type");
    const [vehicleAvailable, setVehicleAvailable] = useState(true); 

    const checkVehicleAvailability = async () => {
        try {
            const response = await fetch(`http://localhost:5165/api/vehicle/CheckIfVehicleExists/${vehicle.frameNr}`);
            if (!response.ok) {
                throw new Error("Fout bij het controleren van de beschikbaarheid van de auto.");
            }

            const availabilityData = await response.json();
            if (availabilityData.message === "Vehicle found.") {
                setVehicleAvailable(true); 
                setErrorMessage(""); 
            } else {
                setVehicleAvailable(false); 
                setErrorMessage("Deze auto is momenteel niet beschikbaar.");
            }
        } catch (error) {
            console.error(error);
            setVehicleAvailable(false); 
            setErrorMessage("Fout bij het controleren van de beschikbaarheid.");
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

    const handlePurchase = () => {
        if (!vehicleAvailable) {
            setErrorMessage("Deze auto is momenteel niet beschikbaar.");
            return; 
        }

        const startDate = new Date(userDetails.rentalStartDate);
        const endDate = new Date(userDetails.rentalEndDate);
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
                        <img
                            src={`data:image/jpeg;base64,${vehicle.image || ""}`}
                            alt={`${vehicle.brand || "Onbekend"} ${vehicle.type || ""}`}
                            className="car-image"
                        />
                    </div>
                    <div className="user-info">
                        <h3 className="user-info-title">Factuuradres en Huurperiode</h3>

                        {/* Factuuradres */}
                        <input
                            type="text"
                            name="billingAddress"
                            placeholder="Vul uw factuuradres in"
                            value={userDetails.billingAddress}
                            onChange={handleInputChange}
                            className="input-field"
                        />

                        {/* Startdatum huurperiode */}
                        <h3> Startdatum huurperiode </h3>
                        <input
                            type="date"
                            name="rentalStartDate"
                            value={userDetails.rentalStartDate}
                            onChange={handleInputChange}
                            className="input-field"
                        />

                        {/* Einddatum huurperiode */}
                        <h3> Einddatum huurperiode </h3>
                        <input
                            type="date"
                            name="rentalEndDate"
                            value={userDetails.rentalEndDate}
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
