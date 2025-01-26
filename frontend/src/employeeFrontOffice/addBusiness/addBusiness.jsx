import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './addBusiness.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';
import { NumberCheck, KvKChecker } from '../../utils/numberFieldChecker.js';
import { NoSpecialCharacters } from '../../utils/stringFieldChecker.js';
import { EmptyFieldChecker } from '../../utils/errorChecker.js';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function AddBusiness() {
    // Initialiseer de state voor elk formulierveld
    const [name, SetName] = useState('');  // Bedrijfsnaam
    const [kvk, SetKvk] = useState('');    // KvK nummer
    const [street, SetStreet] = useState('');  // Straatnaam
    const [number, SetNumber] = useState('');  // Huisnummer
    const [add, SetAdd] = useState('');    // Toevoeging (bijv. appartementnummer)
    const [error, SetErrors] = useState([]);  // Lijst van foutmeldingen

    // Functie om het formulier te versturen
    function Push() {
        // Valideer de ingevulde gegevens
        const validationErrors = EmptyFieldChecker({name, kvk, street, number});

        // Als het KvK nummer te kort is, voeg dan een foutmelding toe
        if (kvk.length < 8) {
            validationErrors.push("Te kort KvK nummer");
        }

        // Zet de verzamelde foutmeldingen in de state
        SetErrors(validationErrors);

        console.log(validationErrors); // Log de foutmeldingen naar de console voor debugging

        // Als er geen foutmeldingen zijn, verstuur de gegevens naar de backend
        if (validationErrors.length === 0) {
            const formData = new FormData();  // Maak een nieuw FormData object aan
            formData.append('KvK', kvk);      // Voeg KvK nummer toe aan het formulier
            formData.append('Name', name);    // Voeg de bedrijfsnaam toe
            formData.append('Adress', `${street} ${add}`);  // Voeg het adres toe, inclusief straat en toevoeging

            // Verstuur het formulier naar de backend
            fetch(`${BACKEND_URL}/api/AddBusiness/addBusiness`, {
                method: 'POST',
                credentials: 'include',  // Zorg ervoor dat cookies worden meegestuurd
                body: formData,  // Verstuur de gegevens als FormData
            })
                .then(response => {
                    // Als de response niet ok is, gooi dan een fout
                    if (!response.ok) {
                        return response.json().then(err => {
                            throw new Error(err.message);
                        });
                    }
                    return response.json();  // Als de response goed is, parse de JSON
                })
                .then(reset => {
                    // Reset de formuliervelden na een succesvolle aanvraag
                    SetName('');
                    SetKvk('');
                    SetStreet('');
                    SetNumber('');
                    SetAdd('');
                })
                .catch(error => {
                    // Als er een fout optreedt, zet de foutmelding in de state
                    SetErrors([error.message]);
                })
        }
    }
    

    return (
        <>
            <GeneralHeader></GeneralHeader>
            <h1>Bedrijf Toevoegen</h1>
            <div className="body">
                <p>Bedrijfsnaam</p>
                <input value={name} onChange={(e) => SetName(e.target.value)}></input>
                <p>KvK</p>
                <input value={kvk} onChange={(e) => SetKvk(KvKChecker(NumberCheck(e.target.value)))}></input>
                <p>Straatnaam</p>
                <input value={street} onChange={(e) => SetStreet(e.target.value)}></input>
                <p>Nummer</p>
                <input value={number} onChange={(e) => SetNumber(NumberCheck(e.target.value))}></input>
                <p>Toevoeging (niet verplicht)</p>
                <input value={add} onChange={(e) => SetAdd(NoSpecialCharacters(e.target.value.toUpperCase()))}></input>
                <button onClick={Push}>Bevestig</button>

                {error.length > 0 && (
                    <div id="errors">
                        <ul>
                            {error.map((errorMessage, index) => (
                                <li key={index}>{errorMessage}</li>
                            ))}
                        </ul>
                    </div>
                )}
            </div>
            <GeneralFooter></GeneralFooter>
        </>
    );
}

export default AddBusiness;
