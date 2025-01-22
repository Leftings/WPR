import React, { useState, useEffect } from 'react';
import { Link, redirectDocument, useNavigate } from 'react-router-dom';
import { getErrorMessage } from '../utils/errorHandler.jsx'
//import "./signUp.css"
import '../index.css';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import {KvKChecker, NumberCheck} from "../utils/numberFieldChecker.js";
import { EmptyFieldChecker } from '../utils/errorChecker.js';
import { pushWithBody } from '../utils/backendPusher.js';
import { NoSpecialCharacters } from '../utils/stringFieldChecker.js';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function SignUp() {
    const [chosenType, setChosenType] = useState('Private');
    const [email, setEmail] = useState('');
    const [phonenumber, setPhonenumber] = useState('');
    const [dateOfBirth, setDateOfBirth] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [password1, setPassword1] = useState('');
    const [password2, setPassword2] = useState('');
    const navigate = useNavigate();
    const [subscription, SetSubscription] = useState('');
    const [name, SetName] = useState('');
    const [kvk, SetKvk] = useState('');
    const [street, SetStreet] = useState('');
    const [number, SetNumber] = useState('');
    const [add, SetAdd] = useState('');
    const [error, SetErrors] = useState([]);
    const [domain, SetDomain] = useState('');
    const [contactEmail, SetContactEmail] = useState('');
    const [isBusinessAccount, setIsBusinessAccount] = useState('Employee');
        

    const choice = (buttonId) => {
        setChosenType(buttonId);
    };
    
    async function Push()
    {
        let validationErrors = [];
        console.log(chosenType);
        if (chosenType === 'Private')
        {
            validationErrors = EmptyFieldChecker({ firstName, lastName, email, password1, password2, street, number, phonenumber, dateOfBirth });
        }
        else
        {
            if (isBusinessAccount === 'Business')
            {
                validationErrors = EmptyFieldChecker({ subscription, name, kvk, street, number, domain, contactEmail });

                if (kvk.length < 8)
                {
                    validationErrors.push("Te kort KvK nummer");
                }
            }
            else
            {
                validationErrors = EmptyFieldChecker({ email, password1, password2 });
            }
        }

        SetErrors(validationErrors);

        console.log(validationErrors);

        if (validationErrors.length === 0)
        {
            try
            {
                const formData = new FormData();
                if (chosenType === 'Private')
                {
                    formData.append('SignUpRequestCustomer.Email', email);
                    formData.append('SignUpRequestCustomer.AccountType', chosenType);
                    formData.append('SignUpRequestCustomer.Password', password1);
                    formData.append('SignUpRequestCustomerPrivate.FirstName', firstName);
                    formData.append('SignUpRequestCustomerPrivate.LastName', lastName);
                    formData.append('SignUpRequestCustomerPrivate.TelNumber', phonenumber);
                    formData.append('SignUpRequestCustomerPrivate.Adres', `${street} ${number}${add}`);
                    formData.append('SignUpRequestCustomerPrivate.BirthDate', new Date(dateOfBirth).toISOString().split('T')[0]);

                    const response = await pushWithBody(`${BACKEND_URL}/api/SignUp/signUp`, formData);
                    redirect(response);
                }
                else
                {
                    if (isBusinessAccount === 'Business')
                    {
                        formData.append('Subscription', subscription)
                        formData.append('KvK', kvk);
                        formData.append('Name', name);
                        formData.append('Adress', `${street} ${number}${add}`);
                        formData.append('Domain', domain);
                        formData.append('ContactEmail', contactEmail);

                        const response = await pushWithBody(`${BACKEND_URL}/api/AddBusiness/addBusiness`, formData);
                        redirect(response);
                    }
                    else
                    {
                        formData.append('SignUpRequestCustomer.Email', email);
                        formData.append('SignUpRequestCustomer.Password', password1);
                        formData.append('SignUpRequestCustomer.AccountType', chosenType);

                        const response = await pushWithBody(`${BACKEND_URL}/api/SignUp/signUp`, formData);
                        redirect(response);
                    }
                }
            } catch (error)
            {
                console.log(error);
                SetErrors([error]);
            }
        }

        function redirect(errors)
        {
            console.log(errors);
            if (errors.errorDetected)
            {
                SetErrors(errors.errors);
            }
            else
            {
                if (chosenType === 'Private' || isBusinessAccount === 'Employee')
                {
                    navigate('/login');
                }
                else
                {
                    navigate('/');
                }
            }
        }
    }

    useEffect(() => {
        SetErrors([]);
    }, [chosenType])

    return (
        <>
            <GeneralHeader />
            <div className='registrateFormatHeader'>
                {chosenType === 'Private' ? (
                    <h1>Aanmelden Particulier</h1>
                ) : (<h1>Aanmelden Bedrijf</h1>
                )}
                <br></br>
                <div id='account'>
                    <label htmlFor='button'>Soort account:</label>
                    <br></br>
                    <button className='cta-button'onClick={() => choice('Private')} id={chosenType === 'Private' ? 'typeButton-active' : 'typeButton'} type='button'>Particulier</button>
                    <button className='cta-button'onClick={() => choice('Business')} id={chosenType === 'Business' ? 'typeButton-active' : 'typeButton'} type='button'>Zakelijk</button>
                </div>
            </div>
            <div className='registrateFormat'>
                {chosenType === 'Private' ? (
                    <>
                        <label htmlFor="firstName">Voornaam</label>
                        <input type="text" id="firstName" value={firstName}onChange={(e) => setFirstName(e.target.value)}></input>

                        <label htmlFor="lastName">Achternaam</label>
                        <input type="text" id="lastName" value={lastName}onChange={(e) => setLastName(e.target.value)}></input>

                        <label htmlFor="email">E-mail</label>
                        <input type="text" id="email" value={email}onChange={(e) => setEmail(e.target.value)}></input>

                        <label htmlFor="password">Wachtwoord</label>
                        <input type="password" id="password" value={password1}onChange={(e) => setPassword1(e.target.value)}></input>

                        <label htmlFor="passwordConfirm">Herhaal wachtwoord</label>
                        <input type="password" id="passwordConfirm" value={password2} onChange={(e) => setPassword2(e.target.value)}></input>

                        <label htmlFor='inputStreet'>Straatnaam</label>
                        <input id='inputStreet' value={street} onChange={(e) => SetStreet(e.target.value)}></input>

                        <label htmlFor='inputNumber'>Nummer</label>
                        <input id='inputNumber' value={number} onChange={(e) => SetNumber(NumberCheck(e.target.value))}></input>

                        <label htmlFor='inputExtra'>Toevoeging (niet verplicht)</label>
                        <input id='inputExtra' value={add} onChange={(e) => SetAdd(NoSpecialCharacters(e.target.value.toUpperCase()))}></input>

                        <label htmlFor="phonenumber">Telefoonnummer</label>
                        <input type="tel" id="phonenumber" value={phonenumber}onChange={(e) => setPhonenumber(e.target.value)}></input>

                        <label htmlFor="dateOfBirth">Geboortedatum</label>
                        <input type="date" id="dateOfBirth" value={dateOfBirth}onChange={(e) => setDateOfBirth(e.target.value)}></input>    
                    </>
                ) : (<>
                    {chosenType === 'Business' && isBusinessAccount === 'Employee' ? (
                        <>
                        {chosenType === 'Business' && (
                            <>
                                <button className='cta-button'onClick={() => setIsBusinessAccount('Employee')} id={isBusinessAccount === 'Employee' ? 'typeButton-active' : 'typeButton'} type='button'>Medewerker</button>
                                <button className='cta-button'onClick={() => setIsBusinessAccount('Business')} id={isBusinessAccount === 'Business' ? 'typeButton-active' : 'typeButton'} type='button'>Bedrijf</button>
                            </>
                        )}
                        <label htmlFor='inputEmailBusiness'>Zakelijk email adress</label>
                        <input id = 'inputEmailBusiness' value={email} onChange={(e) => setEmail(e.target.value.toLowerCase())}></input>
    
                        <label htmlFor='inputPasswordBusiness1'>Wachtwoord</label>
                        <input id = 'inputPasswordBusiness1' value={password1} onChange={(e) => setPassword1(e.target.value)}></input>
    
                        <label htmlFor='inputPasswordBusiness2'>Herhaal Wachtwoord</label>
                        <input id = 'inputPasswordBusiness2' value={password2} onChange={(e) => setPassword2(e.target.value)}></input>
                        </>
                    ) :
                    (
                        <>
                            {chosenType === 'Business' && (
                                <>
                                    <button className='cta-button'onClick={() => setIsBusinessAccount('Employee')} id={isBusinessAccount === 'Employee' ? 'typeButton-active' : 'typeButton'} type='button'>Medewerker</button>
                                    <button className='cta-button'onClick={() => setIsBusinessAccount('Business')} id={isBusinessAccount === 'Business' ? 'typeButton-active' : 'typeButton'} type='button'>Bedrijf</button>
                                </>
                            )}
                            <label htmlFor='inputSubscriptionType'>Abonnement</label>
                            <input id='inputSubscriptionType' value={subscription} onChange={(e) = > SetSubscription(e.traget.value)}></input>
                            
                            <label htmlFor='inputBusinessName'>Bedrijfsnaam</label>
                            <input id='inputBusinessName' value={name} onChange={(e) => SetName(e.target.value)}></input>
    
                            <label htmlFor='inputKvK'>KvK</label>
                            <input id='inputKvK'value={kvk} onChange={(e) => SetKvk(KvKChecker(NumberCheck(e.target.value)))}></input>
    
                            <label htmlFor='inputDomain'>Domein naam</label>
                            <input id='inputDomain' value={domain} onChange={(e) => SetDomain(e.target.value.toLowerCase())} placeholder='@example.nl'></input>
    
                            <label htmlFor='inputStreet'>Straatnaam</label>
                            <input id='inputStreet' value={street} onChange={(e) => SetStreet(e.target.value)}></input>
    
                            <label htmlFor='inputNumber'>Nummer</label>
                            <input id='inputNumber' value={number} onChange={(e) => SetNumber(NumberCheck(e.target.value))}></input>
    
                            <label htmlFor='inputExtra'>Toevoeging (niet verplicht)</label>
                            <input id='inputExtra' value={add} onChange={(e) => SetAdd(NoSpecialCharacters(e.target.value.toUpperCase()))}></input>
    
                            <label htmlFor='inputContactEmail'>Concact Email</label>
                            <input id='inputContactEmail' value={contactEmail} onChange={(e) => SetContactEmail(e.target.value.toLowerCase())}></input>
                        </>
                    )}
                </>)}


                <div className='registrateFormatFooter'>
                    {error.length > 0 && (
                        <div className="error-message">
                            <ul>
                                {error.map((errorMessage, index) => (
                                    <li key={index}>{errorMessage}</li>
                                ))}
                            </ul>
                        </div>
                    )}

                    <button className='cta-button' onClick={Push}>Bevestig</button>
                    <p></p>
                    <label htmlFor="heeftAccount">Heeft u al een account? <Link id="redirect" to="/login">Log in!</Link></label>
                </div>
            </div>
            
            <GeneralFooter></GeneralFooter>
        </>
    );
}

export default SignUp;
