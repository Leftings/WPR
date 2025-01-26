import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import '../index.css';
import GeneralHeader from '../GeneralBlocks/header/header';
import GeneralFooter from '../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function AddVehicleManager() {
  const navigate = useNavigate();
  const [FirstName, SetFirstName] = useState('');
  const [LastName, SetLastName] = useState('');
  const [Password, SetPassword] = useState('');
  const [Email, SetEmail] = useState('');
  const [KvK, SetKvK] = useState('');
  const [ErrorMessage, SetError] = useState([]);

  function SignUp() {
    // Gegevens die verzonden worden naar de backend
    const data = {
      Job: 'Wagen',
      FirstName: FirstName, 
      LastName: LastName, 
      Password: Password, 
      Email: Email,
      KvK: KvK, 
    };

    // Verzenden van de POST-aanroep naar de backend om de gebruiker toe te voegen
    fetch(`${BACKEND_URL}/api/SignUpStaff/signUpStaff`, {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data), 
    })
        .then((response) => {
          // Controleer of de response goed is
          if (!response.ok) {
            return response.json().then((err) => {
              throw new Error(err.message || 'Fout opgetreden');
            });
          }
          return response.json(); 
        })
        .then(() => {
          // Reset de velden en toon een succesmelding
          SetFirstName(''); // Reset voornaam
          SetLastName(''); // Reset achternaam
          SetEmail(''); // Reset e-mail
          SetPassword(''); // Reset wachtwoord
          SetKvK(''); // Reset KvK
          SetError([]); 
          alert('Wagenpark Beheerder toegevoegd!'); 
          navigate('/vehicle-manager'); 
        })
        .catch((error) => {
          // Foutafhandelingslogica voor het toevoegen van de voertuigbeheerder
          console.error('Fout bij het toevoegen van de Voertuigmanager: ', error.message);
          SetError([error.message]); 
        });
  }

  function Check() {
    // Gegevens om te controleren
    let data = {
      FirstName,
      LastName,
      Password,
      Email,
      KvK,
    };

    // Array voor foutmeldingen
    let errors = [];
    for (let key in data) {
      if (data[key] === '') {
        // Voeg een foutmelding toe voor het ontbrekende veld
        errors.push(`${key} is niet ingevuld`);
      }
    }

    // Als er geen fouten zijn, voer dan de SignUp functie uit
    if (errors.length === 0) {
      SignUp();
    } else {
      SetError(errors);
    }
  }

  return (
    <>
      <GeneralHeader />
      <main>
        <h1>Voeg een Wagenpark Beheerder toe</h1>

        <div className='registrateFormat'>
          <label htmlFor='firstName'>Voornaam</label>
          <input
            id='firstName'
            onChange={(e) => SetFirstName(e.target.value)}
            value={FirstName}
          />

          <label htmlFor='lastName'>Achternaam</label>
          <input
            id='lastName'
            onChange={(e) => SetLastName(e.target.value)}
            value={LastName}
          />

          <label htmlFor='Password'>Wachtwoord</label>
          <input
            id='Password'
            type='password'
            onChange={(e) => SetPassword(e.target.value)}
            value={Password}
          />

          <label htmlFor='Email'>Email address</label>
          <input
            id='Email'
            type='email'
            onChange={(e) => SetEmail(e.target.value)}
            value={Email}
          />

          <label htmlFor='KvK'>KvK nummer</label>
          <input
            id='KvK'
            type='number'
            onChange={(e) => SetKvK(e.target.value)}
            value={KvK}
          />

          <div className='registrateFormatFooter'>
            <button className='cta-button' onClick={Check}>
              Registreren
            </button>
          </div>

          <div className='registrateFromatErrors'>
            {ErrorMessage.length > 0 && (
              <div id='errors'>
                <ul>
                  {ErrorMessage.map((errorMessage, index) => (
                    <li key={index}>{errorMessage}</li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        </div>
      </main>
      <GeneralFooter />
    </>
  );
}

export default AddVehicleManager;
