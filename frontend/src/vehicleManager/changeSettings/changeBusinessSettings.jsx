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
            credentials: 'include',  
        })
            .then(response => {
                console.log(response);

                if (!response.ok) {
                    if (response.status === 400) {
                        reject('Session cookie missing or invalid.');
                    } else {
                        reject(`Failed to fetch user ID, status: ${response.status}`);
                    }
                }
                return response.json(); 
            })
            .then(data => {
                if (data.message) {
                    resolve(data.message);  
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
  const [businessError, setBusinessError] = useState('');
  const [customerError, setCustomerError] = useState('');
  const [vehicleManagerError, setVehicleManagerError] = useState('');
  const [showCustomers, setShowCustomers] = useState(false);






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

            const updatedInfo = {
                VehicleManagerInfo: {
                    ID: vehicleManagerInfo?.id,
                    Password: password1 || vehicleManagerInfo?.password, 
                    Email: contactEmail || vehicleManagerInfo?.email,
                },
                BusinessInfo: {
                    KvK: vehicleManagerInfo?.business || 'Default Value', 
                    Abonnement: selectedSubscription || businessInfo?.Abonnement, 
                    ContactEmail: contactEmail || businessInfo?.ContactEmail, 
                    Adres: adres || businessInfo?.Adres,
                    BusinessName: businessName || businessInfo?.BusinessName, 
                }
            };

            console.log("Updated Info JSON to be sent:", updatedInfo);

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

            const updatedVehicleManagerInfo = {
                ID: vehicleManagerInfo?.id,
                Password: password1 || vehicleManagerInfo?.password,
                Email: newEmail, 
            };
            
            console.log("Updated vehicle manager info being sent to backend:", updatedVehicleManagerInfo);

            const updateResponse = await fetch(`${BACKEND_URL}/api/ChangeBusinessSettings/ChangeVehicleManagerInfo`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(updatedVehicleManagerInfo),
            });

            const updateData = await updateResponse.json();

            if (updateResponse.ok) {
                console.log("Vehicle manager updated successfully.");
                navigate('/VehicleManager');
            } else {
                console.error("Error from backend:", updateData.message || "Unknown error");
                setError([updateData.message || "Unknown error"]);
            }
        } catch (error) {
            console.error("Error during form submission:", error);
            setError([error.message || "Unknown error"]);
        }
    };


    const handleDelete = async (type) => {
        console.log("Attempting to delete account with KvK:", vehicleManagerInfo?.business);

        if (!vehicleManagerInfo?.business) {
            setError("Business number (KvK) is missing.");
            return;
        }

        const confirmDelete = window.confirm(
            `Weet je zeker dat je het ${type} account wilt verwijderen?\nVerwijderde account kunnen niet meer teruggebracht worden.`
        );
        if (!confirmDelete) return;

        let data = { KvK: vehicleManagerInfo?.business };

        if (type === 'VehicleManager') {
            data.ID = vehicleManagerInfo.ID;
        }

        console.log('Data to be sent:', data);

        try {
            const response = await pushWithBodyKind(
                `${BACKEND_URL}/api/ChangeBusinessSettings/Delete${type}`,
                data,
                'DELETE'
            );
            if (response.errorDetected) {
                const errorMessage = response.errors.join(', ');
                setError(`Account verwijderen is mislukt: \n${errorMessage}`);
            } else {
                navigate('/VehicleManager');
            }
        } catch (error) {
            console.error(error);
            setError('Account verwijderen is mislukt. Probeer het later opnieuw.');
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

    // Instead of just returning false, set the appropriate error state
    const checkNewEmail = async (email) => {
        console.log("Checking email:", email);

        if (email === '') {
            setError("Email cannot be empty.");
            return false;
        }

        const filledInDomain = email.split('@').pop();
        const businessDomain = businessInfo?.Domain || 'wagenparkbeheerder.nl';

        if (filledInDomain === businessDomain) {
            try {
                const response = await loadSingle(`${BACKEND_URL}/api/ChangeBusinessSettings/CheckNewEmail?email=${email}`);

                if (!response.ok) {
                    const responseData = await response.json();
                    setError(responseData.message || "Unknown error");
                    return false;
                }

                const responseData = await response.json();
                if (response.ok) {
                    return true; // Email is valid
                } else {
                    setError(responseData.message || "Unknown error");
                    return false;
                }
            } catch (error) {
                console.error("Error during email check:", error);
                setError("Email is already in use.");
                return false;
            }
        } else {
            setError(`Domain is not the same as the specified domain (${businessDomain})`);
            return false;
        }
    };



    return (
        <>
            <GeneralHeader />

            <main>
                {/* Company Details Section */}
                <div className='registrateFormatHeader'>
                    <h1>Wijzigen Bedrijfsgegevens</h1>
                </div>
                <div className='registrateFormat'>
                    <label htmlFor='inputBusinessName'>Wijzigen Bedrijfsnaam</label>
                    <input
                        type='text'
                        id='inputBusinessName'
                        value={businessName}
                        onChange={(e) => setBusinessName(e.target.value)}
                        placeholder={businessInfo.BusinessName}
                    />

                    <label htmlFor='inputSubscriptionType'>Abonnement</label>
                    <select
                        id='inputSubscriptionType'
                        value={selectedSubscription}
                        onChange={(e) => SetSelectedSubscription(e.target.value)}
                    >
                        <option value={businessInfo.Abonnement}>
                            Huidig: {businessInfo.Type}
                        </option>
                        {subscriptions.map((sub, index) => (
                            <option key={index} value={index + 1}>
                                {sub}
                            </option>
                        ))}
                    </select>

                    <label htmlFor='inputAdres'>Adres Wijzigen</label>
                    <input
                        type='text'
                        id='inputAdres'
                        value={adres}
                        onChange={(e) => setAdres(e.target.value)}
                        placeholder={businessInfo.Adres}
                    />

                    <label htmlFor='inputContactEmail'>Wijzigen Contact Email</label>
                    <input
                        type='text'
                        id='inputContactEmail'
                        value={contactEmail}
                        onChange={(e) => setContactEmail(e.target.value)}
                        placeholder={businessInfo.ContactEmail}
                    />

                    <div className='registrateFormatFooter'>
                        {vehicleManagerError && <p style={{ color: 'red' }}>{vehicleManagerError}</p>}
                        <button
                            className='cta-button'
                            type='button'
                            onClick={onSubmit}
                        >
                            Opslaan wijzigingen
                        </button>
                        <button
                            id='buttonDelete'
                            type='button'
                            onClick={() => {
                                handleDelete('Business');
                            }}
                        >
                            Verwijderen Bedrijfs Account
                        </button>
                    </div>
                </div>

                {/* Vehicle Manager Details Section */}
                <div className='registrateFormatHeader'>
                    <h1>Wijzigen WagenparkBeheerder Gegevens</h1>
                </div>
                <div className='registrateFormat'>
                    <label htmlFor='inputNewEmail'>Nieuw Email</label>
                    <input
                        type='text'
                        id='inputNewEmail'
                        value={newEmail}
                        onChange={(e) => setNewEmail(e.target.value)}
                        placeholder={vehicleManagerInfo.Email}
                    />

                    <label htmlFor='inputChangePassword'>Nieuw Wachtwoord</label>
                    <input
                        type='password'
                        id='inputChangePassword'
                        value={password1}
                        onChange={(e) => setPassword1(e.target.value)}
                    />

                    <label htmlFor='inputPasswordConfirm'>Herhaal Wachtwoord</label>
                    <input
                        type='password'
                        id='inputPasswordConfirm'
                        value={password2}
                        onChange={(e) => setPassword2(e.target.value)}
                    />

                    <div className='registrateFormatFooter'>
                        {customerError && <p style={{ color: 'red' }}>{customerError}</p>}
                        {error && <p style={{ color: 'red' }}>{error}</p>}
                        <button
                            className='cta-button'
                            type='button'
                            onClick={async () => {
                                if (await checkNewEmail(newEmail)) {
                                    if (password1 !== password2) {
                                        setCustomerError('Wachtwoorden komen niet overeen.');
                                    } else {
                                        await onSubmitVehicleManager(new Event('submit'));
                                    }
                                }
                            }}
                        >
                            Opslaan wijzigingen
                        </button>
                        <button
                            id='buttonDelete'
                            type='button'
                            onClick={() => {
                                handleDelete('VehicleManager');
                            }}
                        >
                            Verwijderen Wagenparkbeheerder Account
                        </button>
                    </div>
                </div>

                {/* Customer Update Section */}
                <div className='customer-update-container'>
                    <h1 className={`expandable-header ${showCustomers ? 'active' : ''}`}>Update Customer gegevens</h1>

                    {/* Centered Button for Show/Hide */}
                    <div className='button-center-container'>
                        <button
                            className='expand-button'
                            onClick={() => setShowCustomers(!showCustomers)}
                        >
                            {showCustomers ? 'Verberg Klanten' : 'Toon Klanten'}
                        </button>
                    </div>

                    {/* Only show customer list when showCustomers is true */}
                    {showCustomers && (
                        customers && customers.length > 0 ? (
                            <div className='customer-list'>
                                {customers.map((customer, index) => (
                                    <div key={customer.id} className='registrateFormat'>
                                        <label htmlFor={`inputCustomerEmail${index}`}>Customer Email</label>
                                        <input
                                            type='email'
                                            id={`inputCustomerEmail${index}`}
                                            value={updatedCustomers[index]?.email || customer.email}
                                            onChange={(e) => handleEmailChange(index, e.target.value)}
                                            placeholder='Enter new email'
                                        />

                                        <label htmlFor={`inputCustomerPassword${index}`}>Customer Password</label>
                                        <input
                                            type='password'
                                            id={`inputCustomerPassword${index}`}
                                            value={updatedCustomers[index]?.password || ''}
                                            onChange={(e) => handlePasswordChange(index, e.target.value)}
                                            placeholder='Enter new password'
                                        />

                                        <div className='update-button-container'>
                                            <button
                                                onClick={() => handleCustomerUpdate(customer.id, index)}
                                                className='cta-button'
                                            >
                                                Update Customer
                                            </button>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        ) : (
                            customers && customers.length === 0 && <div>No customers to update</div>
                        )
                    )}
                </div>
            </main>
            <GeneralFooter />
        </>
    );
}

export default ChangeBusinessSettings;
