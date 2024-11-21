import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import "./signUp.css"

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
    const [KvK, setKvK] = useState(null)

    const choice = (buttonId) => {
        setChosenType(buttonId);
    };

    const onSubmit = (event) => {
        event.preventDefault();
    
        if (!email || !adres || !phonenumber || !dateOfBirth || !password1 || !password2) {
            setError('Bepaalde verplichte veld(en) zijn niet ingevuld.');
            return;
        }
    
        if (password1 !== password2) {
            setError('Wachtwoorden komen niet overeen.');
            return;
        }
    
        setError(null);
    
        const formattedDate = new Date(dateOfBirth).toISOString().split('T')[0];
        let signUpType = chosenType === 1 ? 'signUpPersonal' : 'signUpEmployee';
    
        const data = signUpType === 'signUpPersonal'
            ? {
                Email: email,
                Password: password1,
                FirstName: firstName,
                LastName: lastName,
                TelNumber: phonenumber,
                Adres: adres,
                BirthDate: formattedDate,
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
    
        fetch(`http://localhost:5165/api/SignUp/${signUpType}`, {
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
                navigate('/');
            })
            .catch(error => {
                console.error('Error:', error);
                
                if (error.message === 'Invalid email format') {
                    setError('The email format is invalid.');
                } else if (error.message === 'Email already existing') {
                    setError('The email is already in use. Please use a different email.');
                } else if (error.message === 'Invalid phone number format') {
                    setError('The phone number is invalid.')
                } else {
                    setError(`There was an error during making a ${signUpType} account: ${error.message}`);
                }
            });
    };    

    return (
        <>
            <header>
                <div id="left">
                </div>

            </header>

            <div>
                <div id="inlog">
                    <h1>Aanmelden</h1>
                </div>

                <div id="input">
                    <label htmlFor="heeftAccount">Heeft u al een account? <Link to="/">Log in!</Link></label>
                    <br></br>
                    <br></br>
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
                    {error && <p style={{color: 'red'}}>{error}</p>}
                </div>
            </div>

            <footer></footer>
        </>
    );
}
export default SignUp;
