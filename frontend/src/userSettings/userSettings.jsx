import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import './userSettings.css';

function GetUser(setUser)
{
  fetch('http://localhost:5165/api/Cookie/GetUserName', {
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
    .then(data => {
      setUser(`${data.message}`);
    })
    .catch(error => {
        console.error('Error:', error);
    });
}

function GetUserId() {
  return new Promise((resolve, reject) => {
    fetch('http://localhost:5165/api/Cookie/GetUserId', {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json', 
    },
    credentials: 'include',  // Cookies of authenticatie wordt meegegeven
    })
    .then(response => {
      console.log(response);
      if (!response.ok) {
        reject('No Cookie');
      }
      return response.json();
    })
    .then(data => {
      resolve(data.message);
    })
    .catch(error => {
      console.error('Error:', error);
      reject(error); 
    });
  });
}


function ChangeUserInfo(userData) {
  return fetch('http://localhost:5165/api/ChangeUserSettings/ChangeUserInfo', {
    method: 'PUT',
    headers: {
        'Content-Type': 'application/json', 
    },
    body: JSON.stringify(userData),
    credentials: 'include', // Cookies of authenticatie wordt meegegeven
  })
  .then(response => {
    console.log(response);
    if (!response.ok) {
        throw new Error('Failed to change user info');
    }
    return response.json(); 
  })
  .then(data => {
    return data.message;
  })
  .catch(error => {
    console.error('Error:', error);
    throw error;
  });
}


function UserSettings() {
  const [user, setUser] = useState('');
  const [email, setEmail] = useState('');
  const [adres, setAdres] = useState('');
  const [phonenumber, setPhonenumber] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [password1, setPassword1] = useState('');
  const [password2, setPassword2] = useState('');
  const [error, setError] = useState(null);

  useEffect(() => {
    GetUser(setUser);
  }, []);

  const onSubmit = async (event) => {
    event.preventDefault();
    if (password1 === password2)
    {
      const userId = await GetUserId();

      const userData = {
        ID: userId,
        Email: email,
        Password: password1,
        FirstName: firstName,
        LastName: lastName,
        TelNum: phonenumber,
        Adres: adres,
      };

      await ChangeUserInfo(userData);

      if (firstName !== '')
      {
        GetUser(setUser);
      }

      setError("Gegevens zijn bijgewerkt");
    }
    else
    {
      setError("De wachtwoorden komen niet overeen");
    }
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

            {error && <p style={{color: 'red'}}>{error}</p>}
        </div>
      </body>

      <footer></footer>
    </>
  );
}

export default UserSettings;