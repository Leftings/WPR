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

function ChangeBusinessSettings() {
  const navigate = useNavigate();
  const [password1, setPassword1] = useState('');
  const [password2, setPassword2] = useState('');
  const [error, setError] = useState([]);
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
      fetch(`${BACKEND_URL}/api/Cookie/IsVehicleManager` , {
          method: 'GET',
          headers: {
              'Content-Type': 'application/json',
          },
          credentials: 'include',
      })
          .then(response => {
              if (!response.ok) {
                  return response.json().then(data => {
                    console.log(data);
                    throw new Error(data?.message || 'No Cookie'); 
                  });
                }
              return response.json();
          })
          .catch(() => {
              alert("Cookie was niet geldig");
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
                console.log("Fetching vehicle manager info...");

                const userId = await GetUserId(); // Retrieve user ID
                console.log("User ID:", userId);

                // Fetch vehicle manager info
                const vehicleManagerInfoResponse = await loadList(`${BACKEND_URL}/api/GetInfoVehicleManager/GetAllInfo?id=${userId}`);
                console.log("Vehicle Manager Info Response:", vehicleManagerInfoResponse);

                // Check if the response has the expected structure and contains vehicleManagerInfo
                if (vehicleManagerInfoResponse?.message === 'Success' && vehicleManagerInfoResponse?.vehicleManagerInfo) {
                    setVehicleManagerInfo(vehicleManagerInfoResponse.vehicleManagerInfo);

                    // Get the business number from the vehicle manager info
                    const businessNumber = vehicleManagerInfoResponse.vehicleManagerInfo?.Business;
                    console.log("Business Number:", businessNumber);

                    if (businessNumber) {
                        // Fetch customers associated with the business number
                        console.log(`Fetching customers for Business Number: ${businessNumber}`);
                        const customersResponse = await loadList(`${BACKEND_URL}/api/User/GetCustomersByBusinessNumber?businessNumber=${businessNumber}`);
                        console.log("Customers API Response:", customersResponse);

                        if (customersResponse?.data) {
                            console.log("Fetched Customers:", customersResponse.data);
                            setCustomers(customersResponse.data); // Assuming you have a setCustomers state
                        } else {
                            console.warn("No customers found for this business number.");
                            setError(["No customers found for this business number"]);
                        }
                    } else {
                        console.error("Business number is missing in vehicle manager info.");
                        setError(["Business number is missing"]);
                    }
                } else {
                    console.error("Error: Failed to fetch vehicle manager info.");
                    setError(["Error fetching vehicle manager info."]);
                }
            } catch (error) {
                console.error("Error fetching data:", error);
                setError(["Error fetching vehicle manager or customers data."]);
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
          console.log('abonnement set');
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
          setError([message.message]);
        }
      }
      else
      {
        setError(["Wachtwoorden komen niet overeen"]);
      }
    }
    catch (error)
    {
      console.log(error.message);
      setError(["Er is een fout opgetreden bij het wijzigen van de gegevens"]);
    }
  }

  const handleDelete = async (type) => {
      const confirmDelete = window.confirm(`Weet je zeker dat je het ${type}account wilt verwijderen?\nVerwijderde account kunnen niet meer terug gebracht worden.`);
      if (!confirmDelete) return;
      console.log('VM info: ', vehicleManagerInfo);

      let data;
      if (type === 'Business')
      {
        data = {
            KvK: businessInfo.KvK,
        };
      }
      else
      {
        data = {
            ID: vehicleManagerInfo.ID,
            KvK: vehicleManagerInfo.Business,
        }
      }

      console.log(data);
      try
      {
        const response = await pushWithBodyKind(`${BACKEND_URL}/api/ChangeBusinessSettings/Delete${type}`, data, 'DELETE');
        console.log(response);

        if (response.errorDetected)
        {
          const errorMessage = response.errors.join(', ');
          setError(`Account verwijderen is mislukt: \n${errorMessage}`);
        }
        else
        {
          navigate('/VehicleManager');
        }
      }
      catch (error) {
          const errorMessage = response.errors.join(', ');
          setError(`Account verwijderen is mislukt: \n${errorMessage}`);
      }
  };

  async function checkNewEmail(email)
  {
    if (email === '')
    {
      return true;
    }
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
        setError([response.message]);
        return false;
      }
    }
    else
    {
      setError([`Domein is niet hetzelfde als het opgegeven domain (${businessInfo.Domain})`]);
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
              <button id="buttonDelete" type="button" onClick={() => {handleDelete('Business');}}>Verwijderen Bedrijfs Account</button>
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
                <button id="buttonDelete" type="button" onClick={() => {handleDelete('VehicleManager');}}>Verwijderen Wagenparkbeheerder Account</button>
            </div>
        </div>
      </div>

        </main>
        <GeneralFooter />
    </>
  );
}

export default ChangeBusinessSettings;
