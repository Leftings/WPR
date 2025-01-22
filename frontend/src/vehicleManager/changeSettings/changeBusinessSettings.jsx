import React, { useState, useEffect } from 'react';
import {Await, Link, Navigate, useNavigate} from 'react-router-dom';
import GeneralHeader from "../../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../../GeneralBlocks/footer/footer.jsx";
import { pushWithBodyKind, pushWithoutBodyKind } from '../../utils/backendPusher.js';
import { loadList, loadSingle } from '../../utils/backendLoader.js';

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
  const [businessInfo, setBusinessInfo] = useState({});
  const [domain, setDomain] = useState('');
  const [adres, setAdres] = useState('');
  const [contactEmail, setContactEmail] = useState('');
  const [vehicleManagerInfo, setVehicleManagerInfo] = useState({});
  const [newEmail, setNewEmail] = useState('');
  const [subscriptions, SetSubscriptions] = useState([]);
  const [selectedSubscription, SetSelectedSubscription] = useState('');

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
    async function fetchSubscriptions() {
        try {
            const response = await  fetch(`${BACKEND_URL}/api/Subscription/GetSubscriptions`)
            if (!response.ok) {
                throw new Error('Failed to fetch subcriptions')
            }
            const responseData = await response.json();
            console.log(responseData)
            SetSubscriptions(responseData.data);
        } catch (error) {
            console.log(error);
        }
    }
    
    fetchSubscriptions();
  }, []);

  useEffect(() => {
    async function fetchBusinessData() {
      try {
          const userId = await GetUserId();
          const vehicleManagerInfoResponse = await loadList(`${BACKEND_URL}/api/GetInfoVehicleManager/GetAllInfo?id=${userId}`);
          const response = await loadList(`${BACKEND_URL}/api/ChangeBusinessSettings/GetBusinessInfo?id=${userId}`);

          setVehicleManagerInfo(vehicleManagerInfoResponse.data);
          setBusinessInfo(response.data);
          console.log(response.data);
      } catch (error) {
          console.error(error);
      }
    }

    fetchBusinessData();
  }, []);


  const onSubmit = async (event) => {
    event.preventDefault();
    try
    {
      if (password1 === password2)
      {
        const userId = await GetUserId();

        if (abonnement == 0)
        {
          setAbonnement(businessInfo.Abonnement);
        }

        const data = {
          VehicleManagerInfo: {
            ID: Number(userId),
            Password: password1,
            Email: newEmail,
          },
          BusinessInfo: {
            KvK: 0,
            Abonnement: Number(abonnement),
            Adres: adres,
            BusinessName: businessName,
            ContactEmail: contactEmail,
          },
        };

        console.log(data);

        const message = await pushWithBodyKind(`${BACKEND_URL}/api/ChangeBusinessSettings/ChangeBusinessInfo`, data, 'PUT');
        
        if (message.message === 'succes')
        {
          navigate('/VehicleManager');
        }
        else
        {
          setError(message.message);
        }
      }
      else
      {
        setError("Wachtwoorden komen niet overeen");
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

  async function checkNewEmail(email)
  {
    const filledInDomain = '@' + email.split("@").pop();

    if (filledInDomain === businessInfo.Domain)
    {
      const response = await loadSingle(`${BACKEND_URL}/api/ChangeBusinessSettings/CheckNewEmail?email=email`);
      console.log(response);

      if (response.ok)
      {
        return true;
      }
      else
      {
        setError(response.message);
        return false;
      }
    }
    else
    {
      setError(`Domein is niet hetzelfde als het opgegeven domain (${businessInfo.Domain})`);
      return false;
    }
  }

  return (
    <>
    <GeneralHeader />
    <main>
      <div className='Body'>
        <div className='registrateFormatHeader'>
            <h1>Wijzigen Bedrijfsgegevens</h1>
        </div>

        <div className='registrateFormat'>
            <label htmlFor='inputBusinessName'>Wijzigen Bedrijfsnaam</label>
            <input type='text' id='inputBusinessName' value={businessName} onChange={(e) => setBusinessName(e.target.value)} placeholder={businessInfo.BusinessName}></input>

            <label htmlFor='inputSubscriptionType'>Abonnement</label>
            <select id='inputSubscriptionType' value={selectedSubscription}
                    onChange={(e) => SetSelectedSubscription(e.target.value)}>
                <option value={businessInfo.Abonnement}>Huidig: {businessInfo.Type}</option>
                {subscriptions.map((sub, index) => (
                    <option key={index} value={index +1}>
                        {sub}
                    </option>
                ))}
            </select>

            <label htmlFor='inputAdres'>Adres Wijzigen</label>
            <input type='text' id='inputAdres' value={adres} onChange={(e) => setAdres(e.target.value)} placeholder={businessInfo.Adres}></input>

            <label htmlFor='inputContactEmail'>Wijzigen Contact Email</label>
            <input type='text' id='inputContactEmail' value={contactEmail} onChange={(e) => setContactEmail(e.target.value)} placeholder={businessInfo.ContactEmail}></input>

            {/*<label htmlFor='inputDomain'>Wijzigen Domein</label>
            <input type='text' id='inputDomain' value={domain} onChange={(e) => setDomain(e.target.value)} placeholder={`Huidiig: ${businessInfo.Domain}`}></input>*/}

            <div className='registrateFormatFooter'>
              {error && <p style={{color: 'red'}}>{error}</p>}
              <button className='cta-button' type="button" onClick={onSubmit}>Opslaan wijzigingen</button>
            </div>

          </div>

          <div className='registrateFormatHeader'>
            <h1>Wijzigen WagenparkBeheerder Gegevens</h1>
          </div>
          <div className='registrateFormat'>
            <label htmlFor='inputNewEmail'>Nieuw Email</label>
            <input type='text' id='inputNewEmail' value={newEmail} onChange={(e) => setNewEmail(e.target.value)} placeholder={vehicleManagerInfo.Email}></input>

            <label htmlFor='inputChangePassword'>Nieuw Wachtwoord</label>
            <input type='password' id='inputChangePassword' value={password1} onChange={(e) => setPassword1(e.target.value)}></input>

            <label htmlFor='inputPasswordConfirm'>Herhaal Wachtwoord</label>
            <input type='password' id='inputPasswordConfirm' value={password2} onChange={(e) => setPassword2(e.target.value)}></input>
          

            <div className='registrateFormatFooter'>
                {error && <p style={{color: 'red'}}>{error}</p>}
                <button className='cta-button' type="button" onClick={async () => {if (await checkNewEmail(newEmail)){await onSubmit(new Event('submit'));}}}>Opslaan wijzigingen</button>
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
