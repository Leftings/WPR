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
                return response.json(); 
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
                return response.json(); 
            })
            .catch(() => {
                alert("Cookie was niet geldig"); 
                navigate('/'); // Navigeren naar de loginpagina
            });
    }, [navigate]);

    // Haalt de abonnementen op die beschikbaar zijn voor het bedrijf
    useEffect(() => {
        async function fetchSubscriptions() {
            try {
                const response = await fetch(`${BACKEND_URL}/api/Subscription/GetSubscriptions`);
                if (!response.ok) {
                    throw new Error('Failed to fetch subscriptions'); 
                }
                const responseData = await response.json();
                console.log(responseData);
                SetSubscriptions(responseData.data); // Zet de ontvangen abonnementen in de state
            } catch (error) {
                console.log(error);
            }
        }
        fetchSubscriptions(); 
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
        // Definieer een asynchrone functie voor het ophalen van bedrijfsinformatie
        const fetchBusinessData = async () => {
            try {
                console.log("Beginnend met het ophalen van voertuigmanager informatie...");

                const userId = await GetUserId(); // Verkrijg de gebruikers-ID
                console.log("Gebruikers-ID:", userId);

                if (!userId) {
                    throw new Error("Gebruikers-ID is niet gedefinieerd of niet gevonden!"); // Foutmelding als geen gebruikers-ID gevonden is
                }

                // Bouw de URL om de gegevens van de voertuigmanager op te halen
                const url = `${BACKEND_URL}/api/GetInfoVehicleManager/GetAllInfo?id=${userId}`;
                const response = await fetch(url); // Voer het fetch verzoek uit

                if (!response.ok) {
                    throw new Error(`API-verzoek mislukt met status: ${response.status}`); // Foutmelding bij mislukte API-respons
                }

                const data = await response.json(); // Parseer de API-respons naar JSON

                const { message, vehicleManagerInfo, customers } = data; // Haal belangrijke gegevens uit de API-respons

                if (!vehicleManagerInfo) {
                    console.error("vehicleManagerInfo ontbreekt of is niet gedefinieerd:", vehicleManagerInfo);
                    setError(["Fout: Vehicle manager info ontbreekt of responsbericht is niet 'Success'."]);
                    return;
                }

                const businessNumber = vehicleManagerInfo?.business; // Haal het bedrijfsnummer op uit vehicleManagerInfo
                console.log("Bedrijfsnummer uit vehicleManagerInfo:", businessNumber);

                if (!businessNumber) {
                    console.error("Bedrijfsnummer ontbreekt in vehicleManagerInfo.");
                    setError(["Bedrijfsnummer ontbreekt in vehicleManagerInfo."]);
                    return;
                }

                setBusinessInfo(vehicleManagerInfo); // Zet de opgehaalde bedrijfsinformatie in de state

                if (customers && customers.length > 0) {
                    setCustomers(customers); // Zet de klanten in de state als ze al aanwezig zijn
                    console.log("Gebruik bestaande klanten:", customers);
                } else {
                    console.log(`Opvragen klanten voor bedrijfsnummer: ${businessNumber}`);
                    const customerUrl = `${BACKEND_URL}/api/User/GetCustomersByBusinessNumber?businessNumber=${businessNumber}`;
                    const customerResponse = await fetch(customerUrl); // Haal klanten op via een ander API-verzoek

                    if (!customerResponse.ok) {
                        setCustomerError("Kon de klanten niet ophalen. Status: ${customerResponse.status}"); // Foutmelding als klanten niet opgehaald kunnen worden
                    }

                    const customerData = await customerResponse.json(); // Parseer de klantengegevens
                    console.log("Opgehaalde Klanten Gegevens:", customerData);

                    if (customerData?.data) {
                        setCustomers(customerData.data); // Zet de klanten in de state als ze gevonden zijn
                        console.log("Opgehaalde Klanten:", customerData.data);
                    } else {
                        setError(["Geen klanten gevonden voor het opgegeven bedrijfsnummer."]); // Foutmelding als er geen klanten gevonden zijn
                    }
                }
            } catch (error) {
                console.error("Fout tijdens fetchBusinessData:", error.message); // Log eventuele fouten
                setError([error.message || "Er is een onbekende fout opgetreden."]); // Zet de fout in de state
            }
        };

        fetchBusinessData(); // Voer de fetchBusinessData functie uit bij de component mount
    }, []);


    const onSubmit = async (event) => {
        event.preventDefault();

        // Controleer of de wachtwoorden overeenkomen
        if (password1 !== password2) {
            setError(["Wachtwoorden komen niet overeen"]);
            return;
        }

        try {
            // Haal de gebruikers-ID op
            const userId = await GetUserId();
            if (!userId) {
                throw new Error("Gebruikers-ID is niet gedefinieerd of niet gevonden!");
            }

            const url = `${BACKEND_URL}/api/GetInfoVehicleManager/GetAllInfo?id=${userId}`;
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(`API-aanroep mislukt met status: ${response.status}`);
            }

            const data = await response.json();
            const { vehicleManagerInfo } = data;

            if (!vehicleManagerInfo) {
                throw new Error("Informatie van de voertuigmanager ontbreekt of is niet gedefinieerd.");
            }

            // Update de gegevens die verzonden worden
            console.log("Geverifieerde vehicleManagerInfo:", vehicleManagerInfo);

            const updatedInfo = {
                VehicleManagerInfo: {
                    ID: vehicleManagerInfo?.id,
                    Password: password1 || vehicleManagerInfo?.password,
                    Email: contactEmail || vehicleManagerInfo?.email,
                },
                BusinessInfo: {
                    KvK: vehicleManagerInfo?.business || 'Standaardwaarde',
                    Abonnement: selectedSubscription || businessInfo?.Abonnement,
                    ContactEmail: contactEmail || businessInfo?.ContactEmail,
                    Adres: adres || businessInfo?.Adres,
                    BusinessName: businessName || businessInfo?.BusinessName,
                }
            };

            console.log("Te verzenden bijgewerkte gegevens:", updatedInfo);

            // Verzend de bijgewerkte gegevens naar de backend
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
                // Foutmelding als de update mislukt
                const errorData = await updateResponse.json();
                const errorMessage = 'Niet alles ingevuld of onjuiste gegeven ingevuld';
                setBusinessError(errorMessage);
            }
        } catch (error) {
            // Onverwachte fouten (zoals netwerkfouten) afhandelen
            setBusinessError(`Fout: ${error.message}`);
        }
    };

    const onSubmitVehicleManager = async () => {
        console.log("onSubmitVehicleManager wordt aangeroepen!");
        try {
            // Haal gebruikers-ID op en verwerk de informatie
            const userId = await GetUserId();
            if (!userId) {
                throw new Error("Gebruikers-ID is niet gedefinieerd of niet gevonden!");
            }

            const url = `${BACKEND_URL}/api/GetInfoVehicleManager/GetAllInfo?id=${userId}`;
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(`API-aanroep mislukt met status: ${response.status}`);
            }

            const data = await response.json();
            const { vehicleManagerInfo } = data;

            if (!vehicleManagerInfo) {
                throw new Error("Informatie van de voertuigmanager ontbreekt of is niet gedefinieerd.");
            }

            // Gegevens voor de voertuigmanager bijwerken
            const updatedVehicleManagerInfo = {
                ID: vehicleManagerInfo?.id,
                Password: password1 || vehicleManagerInfo?.password,
                Email: newEmail,
            };

            console.log("Bijgewerkte gegevens van voertuigmanager die naar de backend worden gestuurd:", updatedVehicleManagerInfo);

            const updateResponse = await fetch(`${BACKEND_URL}/api/ChangeBusinessSettings/ChangeVehicleManagerInfo`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(updatedVehicleManagerInfo),
            });

            const updateData = await updateResponse.json();

            // Log het volledige antwoord voor debugging
            console.log("Update-antwoord:", updateData);

            if (updateResponse.ok) {
                console.log("Voertuigmanager succesvol bijgewerkt.");
                navigate('/VehicleManager');
            } else {
                console.error("Fout van backend:", updateData.message || "Onbekende fout");
                setError([updateData.message || "Onbekende fout"]);
            }
        } catch (error) {
            console.error("Fout tijdens het indienen van het formulier:", error);
            setError([error.message || "Onbekende fout"]);
        }
    };

    const handleDelete = async (type) => {
        console.log("Proberen account te verwijderen met KvK:", vehicleManagerInfo?.business);

        // Controleren of KvK ontbreekt
        if (!vehicleManagerInfo?.business) {
            setError("KvK-nummer ontbreekt.");
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

        console.log('Te verzenden gegevens:', data);

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
        // Bijwerken van klantgegevens
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
                console.log("Klant succesvol bijgewerkt!");
            } else {
                const errorText = await response.text();
                console.error("Fout bij het bijwerken van de klant:", errorText);

                try {
                    const errorData = JSON.parse(errorText);
                    console.error("Geparseerde foutgegevens:", errorData);
                } catch (e) {
                    console.error("Respons is geen geldige JSON:", e);
                }
            }
        } catch (error) {
            console.error("Fout tijdens het bijwerken van de klant:", error.message);
        }
    };

    const checkNewEmail = async (email) => {
        console.log("E-mail controleren:", email);

        // Controleer of e-mail leeg is
        if (email === '') {
            setError("E-mail mag niet leeg zijn.");
            return false;
        }

        // Controleer of e-mail formaat geldig is
        const emailRegex = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;
        if (!emailRegex.test(email)) {
            setError("Ongeldig e-mailformaat.");
            return false;
        }

        const filledInDomain = email.split('@')[1];
        console.log("Uitgelezen domein:", filledInDomain);

        if (!filledInDomain) {
            setError("Ongeldig e-mailformaat, ontbrekend domein.");
            return false;
        }

        try {
            // Haal de gegevens van de voertuigmanager op
            const userId = await GetUserId();
            if (!userId) {
                throw new Error("Gebruikers-ID is niet gedefinieerd of niet gevonden!");
            }

            const url = `${BACKEND_URL}/api/GetInfoVehicleManager/GetAllInfo?id=${userId}`;
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(`API-aanroep mislukt met status: ${response.status}`);
            }

            const data = await response.json();
            const { vehicleManagerInfo } = data;

            if (!vehicleManagerInfo) {
                throw new Error("Informatie van de voertuigmanager ontbreekt of is niet gedefinieerd.");
            }

            const businessKvK = vehicleManagerInfo.business;
            if (!businessKvK) {
                throw new Error("KvK ontbreekt in de voertuigmanagerinformatie.");
            }

            const domainUrl = `${BACKEND_URL}/api/GetInfoVehicleManager/GetBusinessDomainByKvK?kvk=${businessKvK}`;
            const domainResponse = await fetch(domainUrl);
            if (!domainResponse.ok) {
                throw new Error(`API-aanroep voor bedrijfsdomein mislukt met status: ${domainResponse.status}`);
            }

            const domainData = await domainResponse.json();
            let businessDomain = domainData?.domain;

            if (!businessDomain) {
                throw new Error("Domein niet gevonden voor de opgegeven KvK.");
            }

            console.log("Geverifieerd bedrijfsdomein:", businessDomain);

            if (!businessDomain.startsWith('@')) {
                businessDomain = '@' + businessDomain;
            }

            // Vergelijk het domein van de e-mail met het bedrijfsdomein
            if (`@${filledInDomain}` === businessDomain) {
                if (email !== vehicleManagerInfo.email) {
                    const checkEmailUrl = `${BACKEND_URL}/api/ChangeBusinessSettings/CheckNewEmail?email=${email}`;
                    const checkEmailResponse = await fetch(checkEmailUrl);
                    if (!checkEmailResponse.ok) {
                        const responseData = await checkEmailResponse.json();
                        console.log("Foutantwoord van checkEmailUrl:", responseData);
                        setError(responseData.message || "Onbekende fout");
                        return false;
                    }

                    const checkEmailData = await checkEmailResponse.json();
                    console.log("checkEmailData:", checkEmailData);

                    if (checkEmailData.message && checkEmailData.message === 'Geldige email') {
                        console.log("E-mail is geldig.");
                        return true;
                    } else {
                        VehicleManagerError("Fout tijdens e-mailcontrole:", checkEmailData.message);
                        setError(checkEmailData.message || "Onbekende fout");
                        return false;
                    }
                }

                console.log("E-mail is hetzelfde als de huidige, geldig.");
                return true;
            } else {
                console.log("Domein komt niet overeen");
                setError(`Domein komt niet overeen met het opgegeven domein (${businessDomain})`);
                return false;
            }

        } catch (error) {
            console.error("Fout tijdens e-mailcontrole:", error);
            setError("Fout bij het controleren van de e-mail. Probeer het later opnieuw.");
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
