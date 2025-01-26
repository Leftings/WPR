import React, { useState, useEffect } from 'react';
import {Await, Link, Navigate, useNavigate} from 'react-router-dom';
import GeneralHeader from "../../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../../GeneralBlocks/footer/footer.jsx";
import { pushWithBodyKind, pushWithoutBodyKind } from '../../utils/backendPusher.js';
import { loadList, loadSingle } from '../../utils/backendLoader.js';

import '../../index.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

// Functie om de gebruikers-ID op te halen via een GET-verzoek
function GetUserId() {
    return new Promise((resolve, reject) => {
        fetch(`${BACKEND_URL}/api/Cookie/GetUserId`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',  // Authenticatiecookie wordt meegestuurd
        })
            .then(response => {
                console.log(response);

                if (!response.ok) {
                    // Foutafhandeling op basis van de statuscode
                    if (response.status === 400) {
                        reject('Session cookie missing or invalid.');
                    } else {
                        reject(`Failed to fetch user ID, status: ${response.status}`);
                    }
                }
                return response.json(); // Parse de JSON-response
            })
            .then(data => {
                // Als de 'message' aanwezig is in de data, wordt het geretourneerd
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

// Custom error class voor specifieke typefouten in de klant-gerelateerde processen
class CustomerError extends Error {
    constructor(message) {
        super(message);
        this.name = 'CustomerError';
    }
}

// Custom error class voor specifieke typefouten in voertuigmanager-gerelateerde processen
class VehicleManagerError extends Error {
    constructor(message) {
        super(message);
        this.name = 'VehicleManagerError';
    }
}

// Custom error class voor zakelijke fouten
class BusinessError extends Error {
    constructor(message) {
        super(message);
        this.name = 'BusinessError';
    }
}

// Hoofdcomponent voor het beheren van bedrijfsinstellingen, inclusief abonnementen en klanteninformatie
function ChangeBusinessSettings() {
    const navigate = useNavigate(); // Navigatie naar verschillende pagina's in de app
    const [password1, setPassword1] = useState(''); // Wachtwoordveld 1
    const [password2, setPassword2] = useState(''); // Wachtwoordveld 2
    const [error, setError] = useState([]); // Lijst van fouten
    const [businessName, setBusinessName] = useState(''); // Bedrijfsnaam
    const [abonnement, setAbonnement] = useState(''); // Abonnementstype
    const [businessInfo, setBusinessInfo] = useState({}); // Bedrijfsinformatie
    const [domain, setDomain] = useState(''); // Domeinnaam van het bedrijf
    const [adres, setAdres] = useState(''); // Bedrijfsadres
    const [contactEmail, setContactEmail] = useState(''); // Bedrijfse-mail
    const [vehicleManagerInfo, setVehicleManagerInfo] = useState({}); // Informatie over de voertuigmanager
    const [newEmail, setNewEmail] = useState(''); // Nieuw e-mailadres
    const [subscriptions, SetSubscriptions] = useState([]); // Abonnementen van het bedrijf
    const [selectedSubscription, SetSelectedSubscription] = useState(''); // Geselecteerd abonnement
    const [updatedCustomers, setUpdatedCustomers] = useState([]); // Bijgewerkte klantinformatie
    const [customers, setCustomers] = useState([]); // Klantlijst
    const [businessError, setBusinessError] = useState(''); // Zakelijke foutmelding
    const [customerError, setCustomerError] = useState(''); // Klantfoutmelding
    const [vehicleManagerError, setVehicleManagerError] = useState(''); // Voertuigmanagerfoutmelding
    const [showCustomers, setShowCustomers] = useState(false); // Toon klanten toggle

    // Haalt op of de gebruiker een voertuigmanager is
    useEffect(() => {
        fetch(`${BACKEND_URL}/api/Cookie/IsVehicleManager`, {
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
                        throw new Error(data?.message || 'No Cookie'); // Gooit fout als geen geldige cookie is gevonden
                    });
                }
                return response.json(); // Geeft de JSON-response terug
            })
            .catch(() => {
                alert("Cookie was niet geldig"); // Waarschuwing als cookie niet geldig is
                navigate('/'); // Navigeren naar de loginpagina
            });
    }, [navigate]);

    // Haalt de abonnementen op die beschikbaar zijn voor het bedrijf
    useEffect(() => {
        async function fetchSubscriptions() {
            try {
                const response = await fetch(`${BACKEND_URL}/api/Subscription/GetSubscriptions`);
                if (!response.ok) {
                    throw new Error('Failed to fetch subscriptions'); // Foutmelding bij mislukte aanvraag
                }
                const responseData = await response.json();
                console.log(responseData);
                SetSubscriptions(responseData.data); // Zet de ontvangen abonnementen in de state
            } catch (error) {
                console.log(error); // Log fouten
            }
        }
        fetchSubscriptions(); // Roept de functie aan om abonnementen op te halen
    }, []); // Lege afhankelijkhedenlijst betekent dat dit alleen bij de eerste render wordt uitgevoerd

    // Handelt de wijziging van het e-mailadres van een klant in de lijst
    const handleEmailChange = (index, newEmail) => {
        const updatedList = [...updatedCustomers]; // Maak een kopie van de klantlijst

        if (!updatedList[index]) {
            updatedList[index] = {}; // Als de klant nog niet bestaat, voeg een nieuwe toe
        }

        updatedList[index].email = newEmail; // Werk het e-mailadres bij
        setUpdatedCustomers(updatedList); // Zet de bijgewerkte lijst in de state
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
                     setCustomerError("Failed to fetch customers. Status: ${customerResponse.status}");
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
                        setCustomerError(`Nog geen customers die bij dit bedrijf horen!`);
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

            const updateResponse = await fetch(`${BACKEND_URL}/api/ChangeBusinessSettings/ChangeBusinessInfo`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(updatedInfo),
            });

            if (updateResponse.ok) {
                navigate('/VehicleManager');
            } else {
                // Try to extract error message from the response
                const errorData = await updateResponse.json();
                const errorMessage = 'Niet alles ingevuld of onjuiste gegeven ingevuld';  
                setBusinessError(errorMessage);
            }
        } catch (error) {
            // Handle unexpected errors (e.g., network errors)
            setBusinessError(`Error: ${error.message}`);
        }
    };
    
    const onSubmitVehicleManager = async () => {
        console.log("onSubmitVehicleManager is being called!");
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

            // Log the full response for debugging
            console.log("Update response:", updateData);

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

    const checkNewEmail = async (email) => {
        console.log("Checking email:", email);

        if (email === '') {
            setError("Email cannot be empty.");
            return false;
        }

        const emailRegex = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;
        if (!emailRegex.test(email)) {
            setError("Invalid email format.");
            return false;
        }

        const filledInDomain = email.split('@')[1];
        console.log("Extracted domain:", filledInDomain);

        if (!filledInDomain) {
            setError("Invalid email format, missing domain.");
            return false;
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

            const businessKvK = vehicleManagerInfo.business;
            if (!businessKvK) {
                throw new Error("Business KvK is missing or undefined in vehicleManagerInfo.");
            }

            const domainUrl = `${BACKEND_URL}/api/GetInfoVehicleManager/GetBusinessDomainByKvK?kvk=${businessKvK}`;
            const domainResponse = await fetch(domainUrl);
            if (!domainResponse.ok) {
                throw new Error(`API request for business domain failed with status: ${domainResponse.status}`);
            }

            const domainData = await domainResponse.json();
            let businessDomain = domainData?.domain;

            if (!businessDomain) {
                throw new Error("Domain not found for the specified KvK.");
            }

            console.log("Fetched business domain:", businessDomain);

            if (!businessDomain.startsWith('@')) {
                businessDomain = '@' + businessDomain;
            }

            if (`@${filledInDomain}` === businessDomain) {
                if (email !== vehicleManagerInfo.email) {
                    const checkEmailUrl = `${BACKEND_URL}/api/ChangeBusinessSettings/CheckNewEmail?email=${email}`;
                    const checkEmailResponse = await fetch(checkEmailUrl);
                    if (!checkEmailResponse.ok) {
                        const responseData = await checkEmailResponse.json();
                        console.log("Error response from checkEmailUrl:", responseData);
                        setError(responseData.message || "Unknown error");
                        return false;
                    }

                    const checkEmailData = await checkEmailResponse.json();
                    console.log("checkEmailData:", checkEmailData);

                    if (checkEmailData.message && checkEmailData.message === 'Geldige email') {
                        console.log("Email is valid.");
                        return true;
                    } else {
                        VehicleManagerError("Error during email check:", checkEmailData.message);
                        setError(checkEmailData.message || "Unknown error");
                        return false;
                    }
                }

                console.log("Email is the same as the current one, valid.");
                return true;
            } else {
                console.log("Domain mismatch detected");
                setError(`Domain is not the same as the specified domain (${businessDomain})`);
                return false;
            }

        } catch (error) {
            console.error("Error during email check:", error);
            setError("Error checking email. Please try again later.");
            return false;
        }
    };

    return (
        <>
            <GeneralHeader />

            <main>
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
                        {businessError && <p style={{ color: 'red' }}>{businessError}</p>}
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
                        {vehicleManagerError && <p style={{ color: 'red' }}>{vehicleManagerError}</p>}
                        <button
                            className='cta-button'
                            type='button'
                            onClick={() => {
                                checkNewEmail(newEmail).then((isValidEmail) => {
                                    setVehicleManagerError(new VehicleManagerError('Ongeldig emailadres, domein moet hetzelfde zijn als oude emailadres').message);
                                    if (isValidEmail) {
                                        if (password1 !== password2) {
                                            setVehicleManagerError('Wachtwoorden komen niet overeen.');
                                        } else {
                                            onSubmitVehicleManager(); 
                                        }
                                    }
                                });
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
                    



                <div className='customer-update-container'>
                    <h1 className={`expandable-header ${showCustomers ? 'active' : ''}`}>Update Customer gegevens</h1>

                    {/* Centered Button for Show/Hide */}
                    {customerError && <p style={{ color: 'red' }}>{customerError}</p>}
                    <div className='button-center-container'>
                        <button
                            className='expand-button'
                            onClick={() => setShowCustomers(!showCustomers)}
                        >
                            {showCustomers ? 'Verberg Klanten' : 'Toon Klanten'}
                        </button>
                    </div>

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
