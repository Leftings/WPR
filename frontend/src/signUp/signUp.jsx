import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import "./signUp.css"

function SignUp() {
    const [chosenType, setChosenType] = useState(null);
    const [email, setEmail] = useState('');
    const [adres, setAdres] = useState('');
    const [phonenumber, setPhonenumber] = useState(null);
    const [dateOfBirth, setDateOfBirth] = useState(null)
    const [password1, setPassword1] = useState('');
    const [password2, setPassword2] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const choice = (buttonId) => {
        setChosenType(buttonId);
    };

    const onSubmit = (event) => {
        event.preventDefault();

        if (!email || !password || !adres || !phonenumber || !dateOfBirth || !password1 || !password2) {
            setError('Bepaalde verplichte veld(en) zijn niet ingevuld.')
        } else if (password1 !== password2) {
            setError('Wachtwoorden komen niet overeen.')
        } else {
            setError(null)
        }

        /*Insert Aanmeldproces naar backend hier...*/
    }

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
