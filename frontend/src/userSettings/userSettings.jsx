import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import './userSettings.css';

function GetUser(setUser)
{
  fetch('http://localhost:5165/api/Home/GetUserName', {
    method: 'GET',
    headers: {
        'Content-Type': 'application/json', 
    },
    credentials: 'include', // Cookies of authenticatie wordt meegegeven
    })
    .then(response => {
        console.log(response);
        if (!response.ok) {
            throw new Error('No Cookie');
        }
        return response.json();
    })
    .then(async data => {
      setUser(`${data.message}`);
    })
    .catch(error => {
        console.error('Error:', error);
    });
}
function UserSettings() {
  const [user, setUser] = useState(null);
  const [email, setEmail] = useState(null);
  const [adres, setAdres] = useState(null);
  const [phonenumber, setPhonenumber] = useState(null);
  const [dateOfBirth, setDateOfBirth] = useState(null);
  const [firstName, setFirstName] = useState(null);
  const [lastName, setLastName] = useState(null);
  const [password1, setPassword1] = useState(null);
  const [password2, setPassword2] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    GetUser(setUser);
  }, []);

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
}

  return (
    <>
      <header>
        <div id="left">
            <p id="user">{user}</p>
        </div>

        <div id="right">
        </div>
      </header>

      <body>
        <div>
            <h1>Gebruikers Instellingen: {user}</h1>
        </div>

        <div>
            <h2>Change</h2>
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
            <button id="button" type="button" onClick={onSubmit}>Opslaan</button>
        </div>
      </body>

      <footer></footer>
    </>
  );
}

export default UserSettings;
