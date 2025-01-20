import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import '../../index.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';


const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function IntakeForm() {
    const navigate = useNavigate();
    const [damagePresent, setDamagePresent] = useState(false);
    const [isValidContract, setIsValidContract] = useState(false);
    const [contractNumber, setContactNumber] = useState(null);
    const [damageExplanation, setDamageExplanation] = useState(null);
    const [endDate, setEndDate] = useState(null);

    const handleDamageCheck = (e) => {
        setDamagePresent(e.target.checked);
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
    return (
        <>
            <GeneralHeader>
            </GeneralHeader>

            <div className='body'>
                <h1 className='title-text'>Innameformulier</h1>

                <div className='intakeFormFormat'>
                    <label htmlFor='contract'>Contract</label>
                    <div className='contractFind'>
                        <input id='contract'/>
                        <button id='contract' className='contractFindButton'><i
                            className="fa-solid fa-magnifying-glass"></i></button>
                    </div>
                    <label htmlFor='vehicleName'>Voertuig: N/A</label>
                    <div className='checkbox-item'>
                        <label htmlFor='damageCheck'>Schade aanwezig:</label>
                        <input
                            type='checkbox'
                            id='damageCheck'
                            checked={damagePresent}
                            onChange={handleDamageCheck}
                        />
                    </div>

                    {damagePresent && (
                        <>
                            <label htmlFor="damageExplanation">Toelichting:</label>
                            <textarea className="resizable-textarea" id="damageExplanation" placeholder="Licht schade toe..."></textarea>
                        </>
                    )}

                    <label htmlFor="endDate">Einddatum</label>
                    <input type="date" />

                    <button className="cta-button">Stuur</button>
                </div>
            </div>

            <GeneralFooter>
            </GeneralFooter>
        </>
    );
}

export default IntakeForm;
