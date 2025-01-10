import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './addBusiness.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';
import { NumberCheck, KvKChecker } from '../../utils/numberFieldChecker.js';
import { NoSpecialCharacters } from '../../utils/stringFieldChecker.js';
import { EmptyFieldChecker } from '../../utils/errorChecker.js';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL_EMPLOYEE ?? 'http://localhost:5276';

function AddBusiness() {
    const [name, SetName] = useState('');
    const [kvk, SetKvk] = useState('');
    const [street, SetStreet] = useState('');
    const [number, SetNumber] = useState('');
    const [add, SetAdd] = useState('');
    const [error, SetErrors] = useState([]);
    

    function Push()
    {
        const validationErrors = EmptyFieldChecker({ name, kvk, street, number });

        if (kvk.length < 8)
        {
            validationErrors.push("Te kort KvK nummer");
        }
        SetErrors(validationErrors);

        console.log(validationErrors);

        if (validationErrors.length === 0)
        {
            const formData = new FormData();
            formData.append('KvK', kvk);
            formData.append('Name', name);
            formData.append('Adress', `${street} ${add}`);

            fetch(`${BACKEND_URL}/api/AddBusiness/addBusiness`, {
                method: 'POST',
                credentials: 'include',
                body: formData,
            })
            .then(response => {
                if (!response.ok)
                {
                    return response.json().then(err => {
                        throw new Error(err.message);
                    });
                }
                return response.json();
            })
            .then(reset => {
                SetName('');
                SetKvk('');
                SetStreet('');
                SetNumber('');
                SetAdd('');
            })
            .catch(error => {
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
