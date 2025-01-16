import React, { useState, useEffect } from 'react';
//import './addEmployee.css';'
import '../../index.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';
import { useNavigate } from 'react-router-dom';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function AddEmployee() {
    const navigate = useNavigate();
    const [KindEmployee, SetKind] = useState('Front');
    const [FirstName, SetFirstName] = useState('');
    const [LastName, SetLastName] = useState('');
    const [Password, SetPassword] = useState('');
    const [Email, SetEmail] = useState('');
    const [ErrorMessage, SetError] = useState([]);
    const [KvK, SetKvK] = useState('');

    function SignUp() {
        // Alle gegevens worden naar JSON omgezet
        const data = {
            Job: KindEmployee,
            FirstName: FirstName,
            LastName: LastName,
            Password: Password,
            Email: Email,
            KvK: KindEmployee === 'Wagen' ? KvK : null // Als KindEmployee niet Wagen is, wordt KvK null
        };
    
        fetch(`${BACKEND_URL}/api/SignUpStaff/signUpStaff`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
        })
        .then(response => {
            if (!response.ok) {
                return response.json().then(err => {
                    throw new Error(err.message || 'Error occurred');
                });
            }
            return response.json();
        })
        .then(data => {
            // Alle velden worden gereset
            SetFirstName('');
            SetLastName('');
            SetEmail('');
            SetPassword('');
            SetKind('Front');
            SetError([]);
            SetKvK('');
        })
        .catch(error => {
            console.error("Error adding employee: ", error.message);
            SetError([error.message]);
        });
    }
    

    function Check() {
        let data = {
            KindEmployee,
            KvK,
            FirstName,
            LastName,
            Password,
            Email,
        };
        
        // Errors voor het niet invullen van een veld worden automatisch aangemaakt, door middel van de keys
        let errors = [];
        for (let key in data) {
            if (data[key] === '')
            {
                if (KindEmployee === 'Wagen' && key === 'KvK')
                {
                    errors.push(`KvK nummer is niet ingevuld\n`);
                }
                else if (key == 'KvK')
                {
                    //pass
                }
                else
                {
                    errors.push(`${key} is niet ingevuld\n`);
                }
            }
        }

        if (errors.length === 0)
        {
            SignUp(); 
        }
        else
        {
            SetError(errors);
        }
    }

    useEffect(() => {
        // Authorisatie checker
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
            <p>Tekst</p>
        </GeneralHeader>
        <div className='body'>
            <h1>Registreren Werknemer</h1>
            <div className='registrateFormat'>
                <label htmlFor='employee'>Soort Medewerker</label>
                <select id='employee' name='office' onChange={(e) => SetKind(e.target.value)} value={KindEmployee}>
                    <option value='Front'>Front Office</option>
                    <option value='Back'>Back Office</option>
                    <option value='Wagen'>Wagenpark Beheerder</option>
                </select>
                {KindEmployee == 'Wagen' && (
                    <>
                        <label htmlFor='KvK'>KvK nummer</label>
                        <input id='KvK' type='number' onChange={(e) => SetKvK(e.target.value)} value={KvK}></input>
                    </>
                )}
                <label htmlFor='firstName'>Voornaam</label>
                <input id='firstName' onChange={(e) => SetFirstName(e.target.value)}value={FirstName}></input>

                <label htmlFor='lastName'>Achternaam</label>
                <input id='lastName' onChange={(e) => SetLastName(e.target.value)} value={LastName}></input>

                <label htmlFor='Password'>Wachtwoord</label>
                <input id='Password' type='password' onChange={(e) => SetPassword(e.target.value)} value={Password}></input>

                <label htmlFor='Email'>Email address</label>
                <input id='Email' type='email' onChange={(e) => SetEmail(e.target.value)} value={Email}></input>

                <div className='registrateFormatFooter'>
                    <button className='cta-button' onClick={Check}>Registreren</button>
                </div>

                <div className='registrateFromatErrors'>
                    {/*Errors worden netjes onder elkaar uitgezet*/}
                    {ErrorMessage.length > 0 && (
                        <div id="errors">
                            <ul>
                                {ErrorMessage.map((errorMessage, index) => (
                                    <li key={index}>{errorMessage}</li>
                                ))}
                            </ul>
                        </div>
                    )}
                </div>
            </div>
        </div>

        <GeneralFooter />
        </>
    );
}

export default AddEmployee;
