import React, { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import '../../index.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';
import { NumberCheck } from '../../utils/numberFieldChecker.js';
import { EmptyFieldChecker } from '../../utils/errorChecker.js';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function AddSubscription() {
    const navigate = useNavigate();
    const [type, SetType] = useState('');
    const [description, SetDescription] = useState('');
    const [discount, SetDiscount] = useState('');
    const [price, SetPrice] = useState('');
    const [error, SetError] = useState([]);

    const SetSubscription = () => {
        // Er wordt een format data aangemaakt
        const subscriptionData = {
            type,
            description,
            discount,
            price,
        };
        
        fetch(`${BACKEND_URL}/api/Subscription/AddSubscription`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(subscriptionData),
        })
            .then(response => {
                if (!response.ok) {
                    return response.json().then(err => {
                        throw new Error(err.message);
                    });
                }
                return response.json();
            })
            .then(() => {
                // Alle velden worden geleegt
                SetType('');
                SetDescription('');
                SetDiscount('');
                SetPrice('');
                SetError([]);
            })
            .catch(error => {
                console.error("Error adding Subscription:", error.message);
                SetError([error.message]);
            });
    };
    
    function Check()
    {
        let subscriptionData = {
            type,
            description,
            discount,
            price,
        };

        let errors = EmptyFieldChecker(subscriptionData);

        if (!NumberCheck(discount)) {
            errors.push('Discount must be a valid number.');
        }

        if (errors.length === 0)
        {
            SetSubscription();
        }
        SetError(errors);
    }

    useEffect(() => {
        // Authoristatie check
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
            <GeneralHeader />
            <main>
                <h1>Toevoegen Abonnement</h1>

                <div className='registrateFormat'>

                    <label htmlFor='Type'>Naam abonnement</label>
                    <input id = 'type' value={type} onChange={(e) => SetType(e.target.value)}></input>

                    <label htmlFor='description'>Beschrijving abonneemnt</label>
                    <input id='description' value={description} onChange={(e) => SetDescription(e.target.value)}></input>

                    <label htmlFor='discount'>Korting van abonnement</label>
                    <input id='discount' type="number" step="0.01" value={discount} onChange={(e) => SetDiscount(e.target.value)}></input>
                    
                    <label htmlFor='price'>Prijs abonnement</label>
                    <input id='price' type="number" step="1.00" value={price} onChange={(e) => SetPrice(e.target.value)}></input>
                    
                    <div className='registrateFormatFooter'>
                        {/*Errors worden netjes onder elkaar uitgelijnt*/}
                        {error.length > 0 && (
                            <div id="errors">
                                <ul>
                                    {error.map((errorMessage, index) => (
                                        <li key={index}>{errorMessage}</li>
                                    ))}
                                </ul>
                            </div>
                        )}

                        <button className='cta-button' onClick={Check}>Abonnement toevoegen</button>
                    </div>
                </div>
            </main>
            <GeneralFooter />
        </>
    );
}

export default AddSubscription;
