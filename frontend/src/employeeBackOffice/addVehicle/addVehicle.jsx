import React, { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
//import './addVehicle.css';
import '../../index.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';
import { SyntaxLicensePlate } from '../../utils/stringFieldChecker.js'
import { NumberCheck } from '../../utils/numberFieldChecker.js';
import { EmptyFieldChecker } from '../../utils/errorChecker.js';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function AddVehicle() {
    const navigate = useNavigate();
    const [kind, SetKind] = useState('Car');
    const [brand, SetBrand] = useState('');
    const [type, SetType] = useState('');
    const [color, SetColor] = useState('');
    const [licensePlate, SetLicensePlate] = useState('');
    const [YoP, SetYoP] = useState('');
    const [price, SetPrice] = useState('');
    const [description, SetDescription] = useState('');
    const [places, SetPlaces] = useState('');
    const [vehicleBlob, SetVehicleBlob] = useState(null);
    const [error, SetError] = useState([]);
    const reference = useRef(null);

    
    const SetVehicle = () => {
        // Er wordt een format data aangemaakt
        const formData = new FormData();
    
        formData.append('YoP', YoP);
        formData.append('Brand', brand);
        formData.append('Type', type);
        formData.append('LicensePlate', licensePlate);
        formData.append('Color', color);
        formData.append('Sort', kind);
        formData.append('Price', price);
        formData.append('Description', description);
        formData.append('Places', places);
    
        if (vehicleBlob && vehicleBlob[0]) {
            formData.append('vehicleBlob', vehicleBlob[0]); // vehicleBlob wordt omgezet naar binary
        }
    
        fetch(`${BACKEND_URL}/api/AddVehicle/addVehicle`, {
            method: 'POST',
            credentials: 'include',
            body: formData,
        })
        .then(response => {
            if (!response.ok) {
                return response.json().then(err => {
                    throw new Error(err.message);
                });
            }
            return response.json();
        })
        .then(vehicleData => {
            // Alle velden worden geleegt
            SetBrand('');
            SetType('');
            SetColor('');
            SetLicensePlate('');
            SetYoP('');
            SetPrice('');
            SetDescription('');
            SetVehicleBlob(null);
            SetPlaces('');
            SetError([]);

            reference.current.value = '';
        })
        .catch(error => {
            console.error("Error adding vehicle:", error.message);
            SetError([error.message]);
        });
    };
    


    function Check()
    {
        let vehicleData = {
            kind,
            brand,
            licensePlate,
            YoP,
            price,
            description,
            vehicleBlob,
            places
        };

        let errors = EmptyFieldChecker(vehicleData);

        if (licensePlate.length !== 9)
        {
            errors.push('licenseplate heeft geen geldige lengte');
        }

        if (errors.length === 0)
        {
            SetVehicle();
        }
        SetError(errors);
    }

    useEffect(() => {
        // Authoristatie check
        fetch(`${BACKEND_URL}/api/Cookie/GetUserId` , {
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
                alert("Cookie was niet geldig");
                navigate('/');
            })
    }, [navigate]);
  

  
    return (
        <>
        <GeneralHeader>
        </GeneralHeader>
        <div className='body'>
            <h1>Toevoegen voertuig</h1>

            <div className='registrateFormat'>
                <label htmlFor='selectVehicle'>Soort voertuig</label>
                <select id='selectVehicle' name="vehicle" value={kind} onChange={(e) => SetKind(e.target.value)}>
                    <option value="Car">Auto</option>
                    <option value="Camper">Camper</option>
                    <option value="Caravan">Caravan</option>
                </select>

                <label htmlFor='brand'>Merk voertuig</label>
                <input id = 'brand' value={brand} onChange={(e) => SetBrand(e.target.value)}></input>

                <label htmlFor='type'>Type voertuig</label>
                <input id='type' value={type} onChange={(e) => SetType(e.target.value)}></input>

                <label htmlFor='color'>Kleur voertuig</label>
                <input id='color' value={color} onChange={(e) => SetColor(e.target.value)}></input>

                <label htmlFor='places'>Aantal zitplaatsen</label>
                <input id='places' value={places} onChange={(e) => SetPlaces(NumberCheck(e.target.value))}></input>

                <label htmlFor='licensePlate'>Nummerbord voertuig</label>
                <input id='licensePlate' value={licensePlate} onChange={(e) => SetLicensePlate(SyntaxLicensePlate(e.target.value, licensePlate))}></input>

                <label htmlFor='YoP'>Bouwjaar voertuig</label>
                <input id='YoP' value={YoP} onChange={(e) => SetYoP(NumberCheck(e.target.value))}></input>

                <label htmlFor='price'>Prijs per dag</label>
                <input id='price' value={price} onChange={(e) => SetPrice(NumberCheck(e.target.value))}></input>

                <label htmlFor='description'>Omschrijving voertuig</label>
                <input id='description' value={description} onChange={(e) => SetDescription(e.target.value)}></input>
                
                <label htmlFor='vehicleBlob'>Afbeelding voertuig</label>
                <input id='vehicleBlob' type="file" ref={reference} onChange={(e) => SetVehicleBlob(e.target.files)}></input>

                <div className='registrateFormatFooter'>
                    {/*Errors worden netjes onder elkaar uitgelijnt*/}
                    {error.length > 0 && (
                        <div id="errors">
                            <ul>
                                {error.map((errorMessage, index) => (
                                    <li key={index}>{errorMessage}</li>
                                ))}
                            </ul>
                        </div>
                    )}

                    <button className='cta-button' onClick={Check}>Voertuig toevoegen</button>
                </div>
            </div>
        </div>

        <GeneralFooter>
        </GeneralFooter>
        </>
    );
}

export default AddVehicle;
