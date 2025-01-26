import React, { useState, useEffect } from 'react';
import { Form, Link, useNavigate } from 'react-router-dom';
import { NumberCheck } from '../../utils/numberFieldChecker.js';
import '../../index.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';
import { EmptyFieldChecker } from "../../utils/errorChecker.js";
import { toast } from "react-toastify";

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function IntakeForm() {
    // Initialiseren van de state variabelen
    const navigate = useNavigate();
    const [damagePresent, setDamagePresent] = useState(false);
    const [isValidContract, setIsValidContract] = useState(false);
    const [contractNumber, setContactNumber] = useState(null); 
    const [damageExplanation, setDamageExplanation] = useState('');
    const [endDate, setEndDate] = useState(null); 
    const [vehicleName, setVehicleName] = useState(null); 
    const [contract, setContract] = useState(null);
    const [orderId, setOrderId] = useState(null); 
    const [tooLate, setTooLate] = useState(false); 
    const [staffId, setStaffId] = useState(null); 
    const [error, setError] = useState([]); 
    const currDate = new Date(); 

    // Functie om schade te controleren (checkbox)
    const handleDamageCheck = (e) => {
        setDamagePresent(e.target.checked); // Zet damagePresent op true/false afhankelijk van de checkbox
        if (damagePresent) {
            setDamageExplanation(null); // Als schade niet aanwezig is, verwijder de toelichting
        }
    };

    // Functie om te controleren of het te laat is om het contract in te nemen
    const handleTooLate = (e) => {
        setTooLate(e.target.checked); // Zet de 'te laat' status
    };

    // Haalt contractgegevens op met behulp van het orderId
    const fetchContract = async (orderId) => {
        try {
            const response = await fetch(`${BACKEND_URL}/api/viewRentalData/GetFullReviewData?id=${orderId}`);

            if (!response.ok) {
                throw new Error("Contract vinden mislukt");
            }
            const data = await response.json();

            const contractData = data.message;

            if (!(Object.keys(contractData).length === 0)) {
                setContract(contractData); // Zet het contract als het bestaat
                setError([]); // Reset eventuele foutmeldingen
            } else {
                setContract(null); // Als het contract niet gevonden is, zet het op null
                setError(["Geen overeenkomend contract"]); // Voeg een foutmelding toe
            }
        } catch (err) {
            setError([err.message]); // Zet foutmelding in de state
            setContract(null); // Maak contract leeg bij fout
            setVehicleName(""); // Reset de voertuignaam
        }
    };

    // Functie die de zoekactie start wanneer de gebruiker op de zoekknop klikt
    const handleSearchClick = () => {
        if (orderId) {
            fetchContract(orderId); // Zoek het contract met het opgegeven orderId
        } else {
            setError(["Please enter a valid Order ID"]); // Foutmelding als geen orderId is ingevuld
        }
    };

    // Functie om het intakeformulier in te dienen
    const setIntake = () => {
        const formData = new FormData(); // Maak een nieuw FormData object aan

        // Als er geen schade is, voeg dan null toe voor de schade
        if (!damagePresent) {
            formData.append('Damage', null);
        } else {
            formData.append('Damage', damageExplanation); // Voeg de schade toelichting toe als er schade is
        }

        formData.append('FrameNrVehicle', contract.FrameNrVehicle); // Voeg voertuignummer toe
        formData.append('ReviewedBy', staffId); // Voeg de medewerker ID toe
        formData.append('Date', endDate); // Voeg de einddatum van het contract toe
        formData.append('Contract', contract.OrderId); // Voeg het contract ID toe
        formData.append('IsDamaged', damagePresent); // Voeg toe of er schade is

        // Verstuur de gegevens naar de backend om het intakeformulier in te dienen
        fetch(`${BACKEND_URL}/api/AddIntake/addIntake`, {
            method: 'POST',
            credentials: 'include',
            body: formData,
        })
            .then(response => {
                if (!response.ok) {
                    return response.json().then(err => {
                        throw new Error(err.message); // Foutmelding als het verzoek faalt
                    });
                }
                toast.success("Inname succesvol verwerkt!"); // Toon succesbericht
                navigate("/FrontOfficeEmployee"); // Navigeer naar een andere pagina
                return response.json();
            })
            .catch(error => {
                console.error("Error adding intake:", error.message); // Log de fout naar de console
                setError([error.message]); // Zet de foutmelding in de state
            });
    };

    // Functie om de gegevens van het formulier te valideren
    function Check() {
        let intakeData = {
            frameNrVehicle: contract.FrameNrVehicle,
            reviewedBy: staffId,
            Einddatum: endDate,
            contract
        };

        let errors = EmptyFieldChecker(intakeData); // Controleer of er lege velden zijn

        // Controleer of de schade toelichting is ingevuld als er schade is
        if (damagePresent && (damageExplanation === "" || damageExplanation === null)) {
            errors.push("Schadetoelichting niet ingevuld");
        }

        const currentDate = new Date();
        const parsedEndDate = new Date(endDate);

        // Controleer of het contract nog actief is (huidige datum moet na de einddatum zijn)
        if (currentDate < parsedEndDate) {
            errors.push("Huurcontract is nog van toepassing, kan inname niet verzenden.");
        }

        // Als er geen fouten zijn, verstuur dan het formulier
        if (errors.length === 0) {
            setIntake();
        }

        setError(errors); // Zet de verzamelde foutmeldingen
    }

    // Gebruik effect om de gebruiker te valideren via cookies bij het laden van de component
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
                setStaffId(id); // Zet de medewerker ID in de state
            })
            .catch(() => {
                alert("Cookie was niet geldig");
                navigate('/'); // Als er geen geldige cookie is, navigeer naar de login pagina
            });
    }, [navigate]);

    // Gebruik effect om de einddatum in te stellen op basis van het contract
    useEffect(() => {
        if (contract && !tooLate) {
            setEndDate(contract.EndDate); // Zet de einddatum als het contract nog actief is
        }
        if ((contract && tooLate)) {
            setEndDate(null); // Zet de einddatum naar null als het contract te laat is
        }
    }, [contract, tooLate]);

return (
        <>
            <GeneralHeader>
            </GeneralHeader>

            <main>
                <h1 className="title-text">Innameformulier</h1>

                <div className="intakeFormFormat">
                    <label htmlFor="contract">Contract</label>
                    <div className="contractFind">
                        <input
                            id="contract"
                            value={orderId}
                            onChange={(e) => setOrderId(NumberCheck(e.target.value))}
                            placeholder="Order ID van contract..."
                        />
                        <button
                            id="contract"
                            className="contractFindButton"
                            onClick={handleSearchClick}
                        >
                            <i className="fa-solid fa-magnifying-glass"></i>
                        </button>
                    </div>
                    {contract && (
                        <>
                            <label
                                htmlFor="vehicleName">{`${contract.Brand || "Ongeldig contract"} ${contract.Type || ""} (${contract.LicensePlate || "Probeer een ander ID"})`}</label>
                            <div className="checkbox-item">
                                <label htmlFor="damageCheck">Schade aanwezig:</label>
                                <input
                                    type="checkbox"
                                    id="damageCheck"
                                    checked={damagePresent}
                                    onChange={handleDamageCheck}
                                />
                            </div>

                            {damagePresent && (
                                <>
                                    <label htmlFor="damageExplanation">Toelichting:</label>
                                    <textarea
                                        className="resizable-textarea"
                                        id="damageExplanation"
                                        placeholder="Licht schade toe..."
                                        onChange={(e) => setDamageExplanation(e.target.value)}
                                    />
                                </>
                            )}

                            <label htmlFor="tooLateCheck">Oorspronkelijke einddatum: {new Date(contract.EndDate).toLocaleDateString()}</label>
                            <div className="checkbox-item">
                                <label htmlFor="tooLateCheck">Te laat:</label>
                                <input
                                    type="checkbox"
                                    id="checkbox-item"
                                    checked={tooLate}
                                    onChange={handleTooLate}
                                />
                            </div>

                            {tooLate && (
                                <>
                                    <label htmlFor="endDate">Einddatum</label>
                                    <input type="date" onChange={(e) => setEndDate(e.target.value)}/>
                                </>
                            )}

                            <p>{endDate}</p>
                            <button className="cta-button" onClick={Check}>Stuur</button>
                        </>
                    )}
                </div>

                <div className="intakeFormFormatFooter">
                    {/* Error display */}
                    {error.length > 0 && (
                        <div id="errors">
                            <ul>
                                {error.map((errorMessage, index) => (
                                    <li key={index}>{errorMessage}</li>
                                ))}
                            </ul>
                        </div>
                    )}
                </div>
            </main>

            <GeneralFooter>
            </GeneralFooter>
        </>
    );
}

export default IntakeForm;
