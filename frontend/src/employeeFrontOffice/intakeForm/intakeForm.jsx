import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { NumberCheck } from '../../utils/numberFieldChecker.js'
import '../../index.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';


const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function IntakeForm() {
    const navigate = useNavigate();
    const [damagePresent, setDamagePresent] = useState(false);
    const [isValidContract, setIsValidContract] = useState(false);
    const [contractNumber, setContactNumber] = useState(null);
    const [damageExplanation, setDamageExplanation] = useState("No damage present.");
    const [endDate, setEndDate] = useState(null);
    const [vehicleName, setVehicleName] = useState(null);
    const [contract, setContract] = useState(null);
    const [orderId, setOrderId] = useState(null);
    const [tooLate, setTooLate] = useState(false);
    const currDate = new Date();

    const handleDamageCheck = (e) => {
        setDamagePresent(e.target.checked);
    };

    const handleTooLate = (e) => {
        setTooLate(e.target.checked);
    };
    
    const fetchContract = async (orderId) => {
        try {
            const response = await fetch(`${BACKEND_URL}/api/viewRentalData/GetFullReviewData?id=${orderId}`);
            
            if (!response.ok) {
                throw new Error ("Contract vinden mislukt");
            }
            const data = await response.json();
            
            const contractData = data.message;
            
            setContract(contractData);
            
            if (contractData.Vehicle) {
                setVehicleName(contractData.Vehicle);
            } else {
                setVehicleName("Voertuig naam kan niet worden gevonden...")
            }
            
            setError("");
        } catch (err) {
            setError(err.message);
            setContract(null);
            setVehicleName("");
        }
    }
    
    const handleSearchClick = () => {
        
        if (orderId) {
            fetchContract(orderId);
        } else {
            setError("Please enter a valid Order ID");
        }
    };

    useEffect(() => {
        // Authorisatie check
        fetch(`${BACKEND_URL}/api/Cookie/GetUserId` , {
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
            .catch(() => {
                alert("Cookie was niet geldig");
                navigate('/');
            })
    }, [navigate]);

    useEffect(() => {
        if (contract && (currDate <= new Date(contract.EndDate) && contract.AccountType === 'Private')) {
            setEndDate(contract.EndDate);
        }
        if (contract && (currDate > new Date(contract.EndDate) || contract.AccountType === 'Business')) {
            setEndDate(null);
        }
    }, [contract, currDate]);
    
    return (
        <>
            <GeneralHeader>
            </GeneralHeader>

            <div className="body">
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

                            {contract.AccountType === 'Private' && (
                                <>
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
                                </>
                            )}

                            {(tooLate || contract.AccountType === 'Business') && (
                                <>
                                    <label htmlFor="endDate">Einddatum</label>
                                    <input type="date" onChange={(e) => setEndDate(e.target.value)}/>
                                </>
                            )}

                            <button className="cta-button">Stuur</button>
                        </>
                    )}
                </div>
            </div>

            <GeneralFooter>
            </GeneralFooter>
        </>
    );
}

export default IntakeForm;
