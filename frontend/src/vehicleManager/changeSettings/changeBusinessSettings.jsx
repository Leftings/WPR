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
            credentials: 'include',  // Ensures cookies are sent with the request
        })
            .then(response => {
                console.log(response);

                // Check if response is not OK and provide more details
                if (!response.ok) {
                    if (response.status === 400) {
                        reject('Session cookie missing or invalid.');
                    } else {
                        reject(`Failed to fetch user ID, status: ${response.status}`);
                    }
                }
                return response.json(); // Parse the JSON body if response is OK
            })
            .then(data => {
                if (data.message) {
                    resolve(data.message);  // If data has a message, resolve with it
                } else {
                    reject('No user ID found in the response.');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                reject(`Error fetching user ID: ${error}`);
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
  const [updatedCustomers, setUpdatedCustomers] = useState([]); 
  const [customers, setCustomers] = useState([]); 




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



    const handleEmailChange = (index, newEmail) => {
        const updatedList = [...updatedCustomers];

        if (!updatedList[index]) {
            updatedList[index] = {}; 
        }

        updatedList[index].email = newEmail;
        setUpdatedCustomers(updatedList);
    };


    const handlePasswordChange = (index, newPassword) => {
        const updatedList = [...updatedCustomers];

        if (!updatedList[index]) {
            updatedList[index] = {};  
        }

        updatedList[index].password = newPassword;

        setUpdatedCustomers(updatedList);
    };


    const handleUpdate = () => {
        // Maak een array van de gewijzigde klanten om naar de backend te sturen
        const updatedData = customers.map((customer, index) => ({
            id: customer.id,
            email: updatedCustomers[index]?.email || customer.email,
            password: updatedCustomers[index]?.password || "",
        }));

        // Stuur de gegevens naar de backend voor update
        updateCustomerData(updatedData);
    };

    const updateCustomerData = async (customerId, updates) => {
        try {
            const response = await fetch(`${BACKEND_URL}/api/UpdateCustomer?id=${customerId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(updates),
            });

            if (response.ok) {
                console.log("Customer updated successfully!");
            } else {
                console.error("Failed to update customer:", await response.json());
            }
        } catch (error) {
            console.error("Error while updating customer:", error);
        }
    };

    useEffect(() => {
        const fetchBusinessData = async () => {
            try {
                console.log("Starting fetch for vehicle manager info...");

                const userId = await GetUserId();
                console.log("User ID:", userId);

                if (!userId) {
                    throw new Error("User ID is undefined or not found!");
                }

                const url = `${BACKEND_URL}/api/GetInfoVehicleManager/GetAllInfo?id=${userId}`;
                const response = await fetch(url);
                console.log("Raw API Response from fetch:", response);

                if (!response.ok) {
                    throw new Error(`API request failed with status: ${response.status}`);
                }

                const data = await response.json();
                console.log("Parsed API Response:", data);

                const { message, vehicleManagerInfo, customers } = data;
                console.log("Parsed vehicleManagerInfo:", vehicleManagerInfo);
                console.log("Parsed Customers:", customers);

                if (!vehicleManagerInfo) {
                    console.error("vehicleManagerInfo is missing or undefined:", vehicleManagerInfo);
                    setError(["Error: Vehicle manager info is missing or response message is not 'Success'."]);
                    return;
                }

                const businessNumber = vehicleManagerInfo?.business;
                console.log("Business Number from vehicleManagerInfo:", businessNumber);

                if (!businessNumber) {
                    console.error("Business number is missing in vehicle manager info.");
                    setError(["Business number is missing in vehicle manager info."]);
                    return;
                }

                setBusinessInfo(vehicleManagerInfo);

                if (customers && customers.length > 0) {
                    setCustomers(customers);
                    console.log("Using existing customers:", customers);
                } else {
                    console.log(`Fetching customers for business number: ${businessNumber}`);
                    const customerUrl = `${BACKEND_URL}/api/User/GetCustomersByBusinessNumber?businessNumber=${businessNumber}`;
                    const customerResponse = await fetch(customerUrl);

                    if (!customerResponse.ok) {
                        throw new Error(`Failed to fetch customers. Status: ${customerResponse.status}`);
                    }

                    const customerData = await customerResponse.json();
                    console.log("Fetched Customers Data:", customerData);

                    if (customerData?.data) {
                        setCustomers(customerData.data);
                        console.log("Fetched Customers:", customerData.data);
                    } else {
                        setError(["No customers found for the provided business number."]);
                    }
                }
            } catch (error) {
                console.error("Error during fetchBusinessData:", error.message);
                setError([error.message || "An unknown error occurred."]);
            }
        };

        fetchBusinessData();
    }, []);
    

    const onSubmit = async (event) => {
        event.preventDefault();

        if (password1 !== password2) {
            setError(["Passwords do not match"]);
            return;
        }

        try {
            // Step 1: Fetch the vehicle manager info again
            const userId = await GetUserId();
            if (!userId) {
                throw new Error("User ID is undefined or not found!");
            }

            const url = `${BACKEND_URL}/api/GetInfoVehicleManager/GetAllInfo?id=${userId}`;
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(`API request failed with status: ${response.status}`);
            }

            const data = await response.json();
            const { vehicleManagerInfo } = data;

            if (!vehicleManagerInfo) {
                throw new Error("Vehicle Manager Info is missing or undefined.");
            }

            console.log("Fetched vehicleManagerInfo:", vehicleManagerInfo);

            // Step 2: Construct updated info in the required format
            const updatedInfo = {
                VehicleManagerInfo: {
                    ID: vehicleManagerInfo?.id, // Correct field name: ID
                    Password: password1 || vehicleManagerInfo?.password, // New password or existing one
                    Email: contactEmail || vehicleManagerInfo?.email, // Updated email or existing one
                },
                BusinessInfo: {
                    KvK: vehicleManagerInfo?.business || 'Default Value', // Default if missing
                    Abonnement: selectedSubscription || businessInfo?.Abonnement, // Selected or existing subscription
                    ContactEmail: contactEmail || businessInfo?.ContactEmail, // Updated contact email or existing one
                    Adres: adres || businessInfo?.Adres, // Updated address or existing one
                    BusinessName: businessName || businessInfo?.BusinessName, // New or existing business name
                }
            };

            // Log the updated info before sending
            console.log("Updated Info JSON to be sent:", updatedInfo);

            // Step 3: Send the data to the correct API route (ChangeBusinessInfo)
            const updateResponse = await fetch(`${BACKEND_URL}/api/ChangeBusinessSettings/ChangeBusinessInfo
`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(updatedInfo),
            });

            if (updateResponse.ok) {
                navigate('/VehicleManager');
            } else {
                const errorData = await updateResponse.json();
                setError([errorData.message]);
            }

        } catch (error) {
            setError([error.message]);
        }
    };

    const onSubmitVehicleManager = async (event) => {
        event.preventDefault();

        try {
            // Log the values before sending to ensure the checked contactEmail and password1 are correct
            console.log("Checking email:", contactEmail); // Ensure this is the new, validated email
            console.log("New Password: ", password1);

            // Step 1: Fetch the vehicle manager info again to ensure we have the latest data
            const userId = await GetUserId();
            if (!userId) {
                throw new Error("User ID is undefined or not found!");
            }

            const url = `${BACKEND_URL}/api/GetInfoVehicleManager/GetAllInfo?id=${userId}`;
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(`API request failed with status: ${response.status}`);
            }

            const data = await response.json();
            const { vehicleManagerInfo } = data;

            if (!vehicleManagerInfo) {
                throw new Error("Vehicle Manager Info is missing or undefined.");
            }

            // Step 2: Construct the updated vehicle manager info object
            const updatedVehicleManagerInfo = {
                ID: vehicleManagerInfo?.id,
                Password: password1 || vehicleManagerInfo?.password,
                Email: newEmail, 
            };
            
            // Log the updated vehicle manager info to verify correctness
            console.log("Updated vehicle manager info being sent to backend:", updatedVehicleManagerInfo);

            // Step 3: Send the update request to the backend API
            const updateResponse = await fetch(`${BACKEND_URL}/api/ChangeBusinessSettings/ChangeVehicleManagerInfo`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(updatedVehicleManagerInfo),
            });

            const updateData = await updateResponse.json();

            // Handle the backend response
            if (updateResponse.ok) {
                // Successfully updated the vehicle manager info
                console.log("Vehicle manager updated successfully.");
                navigate('/VehicleManager');
            } else {
                // Log and set the error message if there was an issue with the backend response
                console.error("Error from backend:", updateData.message || "Unknown error");
                setError([updateData.message || "Unknown error"]);
            }
        } catch (error) {
            // Catch any errors that occurred during the process
            console.error("Error during form submission:", error);
            setError([error.message || "Unknown error"]);
        }
    };


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
    const handleCustomerUpdate = async (customerId, index) => {
        const customerData = {
            Email: updatedCustomers[index]?.email || customers[index].email,
            Password: updatedCustomers[index]?.password || "",
        };

        try {
            const response = await fetch(`${BACKEND_URL}/api/GetInfoVehicleManager/UpdateCustomer?id=${encodeURIComponent(customerId)}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(customerData),
            });

            if (response.ok) {
                console.log("Customer updated successfully!");
            } else {
                const errorText = await response.text();
                console.error("Failed to update customer:", errorText);

                try {
                    const errorData = JSON.parse(errorText);
                    console.error("Parsed error data:", errorData);
                } catch (e) {
                    console.error("Response is not valid JSON:", e);
                }
            }
        } catch (error) {
            console.error("Error while updating customer:", error.message);
        }
    };

    async function checkNewEmail(email) {
        console.log("Checking email:", email);

        if (email === '') {
            console.log("Email is empty, returning true");
            return true;
        }

        // Extract domain from the email
        const filledInDomain = email.split('@').pop();
        console.log("Extracted domain from email:", filledInDomain);

        const businessDomain = businessInfo?.Domain || 'wagenparkbeheerder.nl';
        console.log("Business domain:", businessDomain);

        if (filledInDomain === businessDomain) {
            try {
                const response = await loadSingle(`${BACKEND_URL}/api/ChangeBusinessSettings/CheckNewEmail?email=${email}`);
                const responseData = await response.json();
                console.log("API Response:", responseData);

                if (response.ok) {
                    console.log("Email is valid and response is OK");
                    return true;
                } else {
                    console.log("Error from API:", responseData.message || "Unknown error");
                    setError([responseData.message || "Unknown error"]);
                    return false;
                }
            } catch (error) {
                console.error("Error during email check:", error);
                setError([error.message || "Unknown error"]);
                return false;
            }
        } else {
            console.log(`Error: Domain does not match the business domain (${businessDomain})`);
            setError([`Domein is niet hetzelfde als het opgegeven domain (${businessDomain})`]);
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
          <div className='customer-update-container'>
              {error && <div className="error">{error}</div>}

              <h1>Update Customer gegevens</h1>
              {customers && customers.length > 0 ? (
                  <div className="customer-list">
                      {customers.map((customer, index) => (
                          <div key={customer.id} className="registrateFormat">
                              <label htmlFor={`inputCustomerEmail${index}`}>Customer Email</label>
                              <input
                                  type="email"
                                  id={`inputCustomerEmail${index}`}
                                  value={updatedCustomers[index]?.email || customer.email}
                                  onChange={(e) => handleEmailChange(index, e.target.value)}
                                  placeholder="Enter new email"
                              />

                              <label htmlFor={`inputCustomerPassword${index}`}>Customer Password</label>
                              <input
                                  type="password"
                                  id={`inputCustomerPassword${index}`}
                                  value={updatedCustomers[index]?.password || ""}
                                  onChange={(e) => handlePasswordChange(index, e.target.value)}
                                  placeholder="Enter new password"
                              />

                              <div className="update-button-container">
                                  <button
                                      onClick={() => handleCustomerUpdate(customer.id, index)}
                                      className="cta-button">
                                      Update Customer
                                  </button>
                              </div>
                          </div>
                      ))}
                  </div>
              ) : (
                  <div>No customers to update</div>
              )}
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
                <button
                    className='cta-button'
                    type="button"
                    onClick={async () => {
                        if (await checkNewEmail(newEmail)) {
                            await onSubmitVehicleManager(new Event('submit'));
                        }
                    }}
                >
                    Opslaan wijzigingen
                </button>

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
