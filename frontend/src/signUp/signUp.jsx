import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getErrorMessage } from '../utils/errorHandler.jsx'
import "./signUp.css"
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function SignUp() {
    const [chosenType, setChosenType] = useState(null);
    const [email, setEmail] = useState('');
    const [adres, setAdres] = useState('');
    const [phonenumber, setPhonenumber] = useState('');
    const [dateOfBirth, setDateOfBirth] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [password1, setPassword1] = useState('');
    const [password2, setPassword2] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();
    const [KvK, setKvK] = useState('')

    const choice = (buttonId) => {
        setChosenType(buttonId);
    };

    const onSubmit = (event) => {
        event.preventDefault();
    
        if (!email || !adres || !phonenumber || !password1 || !password2) {
            setError('Bepaalde verplichte veld(en) zijn niet ingevuld.');
            return;
        }
    
        if (password1 !== password2) {
            setError('Wachtwoorden komen niet overeen.');
            return;
        }
        
        if (chosenType === 2) {
            if (!KvK || KvK.length !== 8) {
                setError('KVK number must be 8 digits.');
                return;
            }
        }
    
        setError(null);

        let signUpType = chosenType === 1 ? 'signUpPersonal' : 'signUpEmployee';

      
        const data = signUpType === 'signUpPersonal'
            ? {
                Email: email,
                Password: password1,
                FirstName: firstName,
                LastName: lastName,
                TelNumber: phonenumber,
                Adres: adres,
                BirthDate: new Date(dateOfBirth).toISOString().split('T')[0],
                KvK: null
            }
            : {
                Email: email,
                Password: password1,
                FirstName: firstName,
                LastName: lastName,
                TelNumber: phonenumber,
                Adres: adres,
                BirthDate: null,
                KvK: KvK
            };
    
        fetch(`${BACKEND_URL}/api/SignUp/${signUpType}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
            credentials: 'include',
        })
            .then(response => {
                if (!response.ok) {
                    return response.json().then(err => {
                        throw new Error(err.message || 'Sign up failed')
                    });
                }
                return response.json();
            })
            .then(data => {
                console.log('Sign up successful', data);
                navigate('/login');
            })
            .catch(error => {
                console.error('Error:', error);
                setError(getErrorMessage(error, signUpType));
            });
    };    

    return (
        <>
            <GeneralHeader />

            <div>
                <div id="aanmelden">
                    <h1>Aanmelden</h1>
                </div>

                <div id="input">
                    <label htmlFor="button">Soort Account:</label>
                    <br></br>
                    <button
                        onClick={() => choice(1)}
                        id={chosenType === 1 ? 'typeButton-active' : 'typeButton'}
                        type="button"
                    >
                        Particulier
                    </button>
                    <button
                        onClick={() => choice(2)}
                        id={chosenType === 2 ? 'typeButton-active' : 'typeButton'}
                        type="button"
                    >
                        Zakelijk
                    </button>

                    {chosenType === 1 && (
                        <>
                            <br></br>
                            <label htmlFor="firstName">Voornaam</label>
                            <br></br>
                            <input type="text" id="firstName" value={firstName}
                                   onChange={(e) => setFirstName(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="lastName">Achternaam</label>
                            <br></br>
                            <input type="text" id="lastName" value={lastName}
                                   onChange={(e) => setLastName(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="email">E-mail</label>
                            <br></br>
                            <input type="text" id="email" value={email}
                                   onChange={(e) => setEmail(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="adres">Adres</label>
                            <br></br>
                            <input type="text" id="adres" value={adres}
                                   onChange={(e) => setAdres(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="phonenumber">Telefoonnummer</label>
                            <br></br>
                            <input type="tel" id="phonenumber" value={phonenumber}
                                   onChange={(e) => setPhonenumber(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="dateOfBirth">Geboortedatum</label>
                            <br></br>
                            <input type="date" id="dateOfBirth" value={dateOfBirth}
                                   onChange={(e) => setDateOfBirth(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="password">Wachtwoord</label>
                            <br></br>
                            <input type="password" id="password" value={password1}
                                   onChange={(e) => setPassword1(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="passwordConfirm">Herhaal wachtwoord</label>
                            <br></br>
                            <input type="password" id="passwordConfirm" value={password2}
                                   onChange={(e) => setPassword2(e.target.value)}></input>
                            <br></br>
                            <button id="button" type="button" onClick={onSubmit}>Maak Account</button>
                        </>
                    )}

                    {chosenType === 2 && (
                        <>
                            <br></br>
                            <label htmlFor="firstName">Voornaam</label>
                            <br></br>
                            <input type="text" id="firstName" value={firstName}
                                   onChange={(e) => setFirstName(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="lastName">Achternaam</label>
                            <br></br>
                            <input type="text" id="lastName" value={lastName}
                                   onChange={(e) => setLastName(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="email">E-mail</label>
                            <br></br>
                            <input type="text" id="email" value={email}
                                   onChange={(e) => setEmail(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="adres">Adres</label>
                            <br></br>
                            <input type="text" id="adres" value={adres}
                                   onChange={(e) => setAdres(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="phonenumber">Telefoonnummer</label>
                            <br></br>
                            <input type="tel" id="phonenumber" value={phonenumber}
                                   onChange={(e) => setPhonenumber(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="kvk">KVK</label>
                            <br></br>
                            <input type="text" id="kvk" value={KvK}
                                   onChange={(e) => setKvK(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="password">Wachtwoord</label>
                            <br></br>
                            <input type="password" id="password" value={password1}
                                   onChange={(e) => setPassword1(e.target.value)}></input>
                            <br></br>
                            <label htmlFor="passwordConfirm">Herhaal wachtwoord</label>
                            <br></br>
                            <input type="password" id="passwordConfirm" value={password2}
                                   onChange={(e) => setPassword2(e.target.value)}></input>
                            <br></br>
                            <button id="button" type="button" onClick={onSubmit}>Maak Account</button>
                        </>
                    )}
                    <br></br>
                    <label htmlFor="heeftAccount">Heeft u al een account? <Link id="redirect" to="/login">Log
                        in!</Link></label>
                    {error && <p style={{color: 'red'}}>{error}</p>}
                </div>
            </div>
            
        </>
    );
}

export default SignUp;
