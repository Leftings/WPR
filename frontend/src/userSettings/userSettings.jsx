import React, { useState, useEffect } from 'react';
import {Await, Link, Navigate, useNavigate} from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";

import '../index.css';

/// <summary>
/// Haalt de gebruikersnaam van de ingelogde gebruiker op en zet deze in de opgegeven staat.
/// </summary>
/// <param name="setUser">Functie om de staat bij te werken met de opgehaalde gebruikersnaam.</param>
function GetUser(setUser) {
    fetch('http://localhost:5165/api/Cookie/GetUserName', {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
        credentials: 'include',  // Verstuurt cookies voor authenticatie
    })
        .then(response => {
            console.log(response);
            if (!response.ok) {
                throw new Error('No Cookie');  // Fout als er geen cookie is
            }
            return response.json();  // Zet de reactie om naar JSON
        })
        .then(data => {
            setUser(`${data.message}`);  // Zet de gebruikersnaam in de state
        })
        .catch(error => {
            console.error('Error:', error);  // Logt fouten naar de console
        });
}

/// <summary>
/// Haalt het gebruikers-ID op en retourneert het als een promise.
/// </summary>
/// <returns>Retourneert een promise met het gebruikers-ID of een foutmelding als de cookie niet gevonden wordt.</returns>
function GetUserId() {
    return new Promise((resolve, reject) => {
        fetch('http://localhost:5165/api/Cookie/GetUserId', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',  // Verstuurt cookies voor authenticatie
        })
            .then(response => {
                console.log(response);
                if (!response.ok) {
                    reject('No Cookie');  // Verwerpt de promise als er geen geldige cookie is
                }
                return response.json();  // Zet de reactie om naar JSON
            })
            .then(data => {
                resolve(data.message);  // Lost de promise op met het gebruikers-ID
            })
            .catch(error => {
                console.error('Error:', error);  // Logt fouten naar de console
                reject(error);  // Verwerpt de promise met de fout
            });
    });
}

/// <summary>
/// Verstuurt een PUT-verzoek om de gebruikersinformatie bij te werken met de opgegeven gegevens.
/// </summary>
/// <param name="userData">Object met de gebruikersinformatie die bijgewerkt moet worden.</param>
/// <returns>Retourneert een bericht dat aangeeft of de informatie succesvol is bijgewerkt of een foutmelding.</returns>
function ChangeUserInfo(userData) {
    return fetch('http://localhost:5165/api/ChangeUserSettings/ChangeUserInfo', {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(userData),  // Stuurt de gebruikersinformatie als JSON in de request body
        credentials: 'include',  // Verstuurt cookies voor authenticatie
    })
        .then(async (response) => {
            const data = await response.json();  // Wacht op de JSON-reactie
            if (!response.ok) {
                if (data.message !== 'Email detected') {
                    throw new Error("Onbekende fout");  // Behandelt foutmeldingen behalve 'Email detected'
                }
            }
            return data.message;  // Retourneert het bericht van de reactie
        })
        .catch((error) => {
            console.error(error);  // Logt fouten naar de console
            throw error;  // Gooit de fout opnieuw voor verdere verwerking
        });
}

/// <summary>
/// Verwijdert het gebruikersaccount met het opgegeven gebruikers-ID.
/// </summary>
/// <param name="userId">Het gebruikers-ID van de te verwijderen gebruiker.</param>
/// <returns>Retourneert een resultaat dat aangeeft of de verwijdering succesvol was of niet.</returns>
function DeleteUser(userId) {
    const encryptedUserId = encrypt(userId);  // Versleutelt het gebruikers-ID
    return fetch(`http://localhost:5165/api/ChangeUserSettings/DeleteUser/${encryptedUserId}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        },
        credentials: 'include',  // Verstuurt cookies voor authenticatie
    })
        .then(async (response) => {
            const data = await response.json();  // Wacht op de JSON-reactie
            if (!response.ok) {
                throw new Error(data.message || 'Fout');  // Gooi een foutmelding als de reactie niet OK is
            }
            return data;  // Retourneert de gegevens van de reactie
        })
        .catch((error) => {
            console.error('Fout bij het verwijderen van gebruiker:', error.message);  // Logt fouten naar de console
            throw error;  // Gooi de fout opnieuw
        });
}

/// <summary>
/// Behandelt de instellingen van de gebruiker, inclusief het updaten van gegevens en het verwijderen van het account.
/// </summary>
function UserSettings() {
    const navigate = useNavigate();
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
        fetch('http://localhost:5165/api/Cookie/GetUserId', {
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
            .then(() => {
                GetUser(setUser);
            })
            .catch(() => {
                navigate('/');
            })
    }, [navigate]);

    const onSubmit = async (event) => {
        event.preventDefault();
        try
        {
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

                const message = await ChangeUserInfo(userData);

                if (firstName !== '')
                {
                    GetUser(setUser);
                }

                if (message === 'Data Updated')
                {
                    navigate('/home');
                }
                else
                {
                    setError(message);
                }
            }
        }
        catch (error)
        {
            setError("Er zijn geen velden ingevoerd");
        }
    }

    useEffect(() => {
        GetUserId()
            .then((id) => {
                setUser(id);
                GetUser(setUser);
            })
            .catch(() => {
                navigate('/');
            });
    }, [navigate]);

    const handleDelete = async () => {
        const confirmDelete = window.confirm('Weet je zeker dat je je account wilt verwijderen?');
        if (!confirmDelete) return;

        try {
            const response = await fetch('http://localhost:5165/api/ChangeUserSettings/DeleteUser', {
                method: 'DELETE',
                credentials: 'include', // Verstuurt de cookie
            });

            const data = await response.json();
            if (response.ok) {
                alert('Je account is succesvol verwijderd');
                navigate('/'); // Stuur de gebruiker door na verwijdering
            } else {
                setError(data.message);
            }
        } catch (error) {
            setError('Verwijderen van account is mislukt');
        }
    };

  return (
    <>
    <GeneralHeader />
    <main>
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
            

            <button id="buttonDelete" type="button" onClick={handleDelete}>Delete account</button>

            {error && <p style={{color: 'red'}}>{error}</p>}
        </div>
      </body>

        </main>
        <GeneralFooter />
    </>
  );
}

export default UserSettings;
