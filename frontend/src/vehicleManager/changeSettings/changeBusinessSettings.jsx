import React, { useState, useEffect } from 'react';
import {Await, Link, Navigate, useNavigate} from 'react-router-dom';
import GeneralHeader from "../../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../../GeneralBlocks/footer/footer.jsx";
import { pushWithBodyKind, pushWithoutBodyKind } from '../../utils/backendPusher.js';

//import './userSettings.css';
import '../../index.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function GetUserId() {
  return new Promise((resolve, reject) => {
    fetch(`${BACKEND_URL}/api/Cookie/GetUserId`, {
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

function DeleteUser(userId) {
    const encryptedUserId = encrypt(userId)
    return fetch(`${BACKEND_URL}/api/ChangeUserSettings/DeleteUser/${encryptedUserId}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'},
        credentials: 'include',
    })
        .then(async (response) => {
            const data = await response.json();
            if (!response.ok) {
                throw new Error(data.message || 'Error');
            }
            return data;
        })
        .catch((error) => {
            console.error('Error deleting user:', error.message);
            throw error;
        })
}

function ChangeBusinessSettings() {
  const navigate = useNavigate();
  const [password1, setPassword1] = useState('');
  const [password2, setPassword2] = useState('');
  const [error, setError] = useState(null);
  const [businessName, setBusinessName] = useState('');
  const [abonnement, setAbonnement] = useState('');
  const [businessInfo, setBusinessInfo] = useState([]);

  useEffect(() => {
      fetch(`${BACKEND_URL}/api/Cookie/GetUserId`, {
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
              navigate('/');
          })
  }, [navigate]);

  useEffect(() => {
    const userId = GetUserId();

    const data = {ID: Number(userId)};

    const message = pushWithoutBodyKind(`${BACKEND_URL}`)
  })

  const onSubmit = async (event) => {
    event.preventDefault();
    try
    {
      if (password1 === password2)
      {
        const userId = await GetUserId();

        const data = {
          VehicleManagerInfo: {
            ID: Number(userId),
            Password: password1,
          },
          BusinessInfo: {
            KvK: 0,
            Abonnement: Number(abonnement),
          },
        };

        const message = await pushWithBodyKind(`${BACKEND_URL}/api/ChangeBusinessSettings/ChangeBusinessInfo`, data, 'PUT').message;

        if (message === 'succes')
        {
          navigate('/VehicleManager');
        }
        else
        {
          setError(message);
        }
      }
    }
    catch (error)
    {
      console.log(error.message);
      setError("Er is een fout opgetreden bij het wijzigen van de gegevens");
    }
  }

  const handleDelete = async () => {
      const confirmDelete = window.confirm('Are you sure you want to delete your account?');
      if (!confirmDelete) return;

      try {
          const response = await fetch(`${BACKEND_URL}/api/ChangeUserSettings/DeleteUser`, {
              method: 'DELETE',
              credentials: 'include', // Include the cookie
          });
          
          const data = await response.json();
          if (response.ok) {
              alert('Your account has been deleted successfully');
              navigate('/'); // Redirect after deletion
          } else {
              setError(data.message);
          }
      } catch (error) {
          setError('Failed to delete account');
      }
  };

  /*useEffect(() => {
    try
    {
      const response = await (fetch)
    }
  }, [])*/

  return (
    <>
    <GeneralHeader />
    <main>
      <div className='Body'>
        <div className='registrateFormatHeader'>
            <h1>Wijzigen bedrijfsgegevens</h1>
        </div>

        <div className='registrateFormat'>
            <label htmlFor='inputBusinessName'>Wijzigen Bedrijfsnaam</label>
            <input type='text' id='inputBusinessName' value={businessName} onChange={(e) => setBusinessName(e.target.value)}></input>

            <label htmlFor='inputAbonnement'>Abonnement wijzigen</label>
            <select id='inputAbonnement' value={abonnement} onChange={(e) => setAbonnement(e.target.value)}>
              <option value=''>Kies een abonnement</option>
              <option value={1}>Pay As You Go</option>
              <option value={2}>Standaard Abonnement</option>
            </select>

            <label htmlFor='inputChangePassword'>Wachtwoord</label>
            <input type='password' id='inputChangePassword' value={password1} onChange={(e) => setPassword1(e.target.value)}></input>

            <label htmlFor='inputPasswordConfirm'>Herhaal wachtwoord</label>
            <input type='password' id='inputPasswordConfirm' value={password2} onChange={(e) => setPassword2(e.target.value)}></input>
          

            <div className='registrateFormatFooter'>
                {error && <p style={{color: 'red'}}>{error}</p>}
                <button className='cta-button' type="button" onClick={onSubmit}>Opslaan wijzigingen</button>
                <button id="buttonDelete" type="button" onClick={handleDelete}>Verwijderen Bedrijfs Account</button>
            </div>
        </div>
      </div>

        </main>
        <GeneralFooter />
    </>
  );
}

export default ChangeBusinessSettings;
