import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './addVehicle.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL_EMPLOYEE ?? 'http://localhost:5276';

function SyntaxLicensePlate(licensePlate)
{
    if (licensePlate.length % 3 === 0 && licensePlate.length !== 9)
    {
        licensePlate += '-';
    }
    return licensePlate;
}

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
            SetVehicleBlob('');
            SetPlace('');
            SetError([]);
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

        // Errors worden automatisch aangemaakt, doormiddel van de keys
        let errors = [];
        for (let key in vehicleData)
        {
            if (vehicleData[key] === '')
            {
                errors.push(`${key} is niet ingevuld\n`);
            }
            console.log(`${key}: ${vehicleData[key]}`);
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

        <div className="body">
            <h1>Toevoegen voertuig</h1>
            <div id="kind" value={kind} onChange={(e) => SetKind(e.target.value)}>
                <p>Soort voertuig</p>
                <select name="vehicle">
                    <option value="Car">Auto</option>
                    <option value="Camper">Camper</option>
                    <option value="Caravan">Caravan</option>
                </select>
                <br></br>
            </div>
            <div id="brand">
                <p>Merk voertuig</p>
                <input value={brand} onChange={(e) => SetBrand(e.target.value)}></input>
                <br></br>
            </div>
            <div id="type">
                <p>Type voertuig</p>
                <input value={type} onChange={(e) => SetType(e.target.value)}></input>
            </div>
            <div id="color">
                <p>Kleur voertuig</p>
                <input value={color} onChange={(e) => SetColor(e.target.value)}></input>
            </div>
            <div id="places">
                <p>Aantal zitplaatsen</p>
                <input type="number" value={places} onChange={(e) => SetPlaces(e.target.value)}></input>
            </div>
            <div id="licensePlate">
                <p>Nummerbord voertuig</p>
                <input value={licensePlate} onChange={(e) => SetLicensePlate(SyntaxLicensePlate(e.target.value))}></input>
                <br></br>
            </div>
            <div id="YoP">
                <p>Bouwjaar voertuig</p>
                <input type="number" value={YoP} onChange={(e) => SetYoP(e.target.value)}></input>
                <br></br>
            </div>
            <div id="price">
                <p>Prijs per dag</p>
                <input type="number" value={price} onChange={(e) => SetPrice(e.target.value)}></input>
                <br></br>
            </div>
            <div id="description">
                <p>Omschrijving voertuig</p>
                <input value={description} onChange={(e) => SetDescription(e.target.value)}></input>
                <br></br>
            </div>
            <div id="vehicleBlob">
                <p>Afbeelding voertuig (verplict)</p>
                <input type="file" onChange={(e) => SetVehicleBlob(e.target.files)}></input>
                <br></br>
            </div>
            <div id="confirm">
                <button onClick={Check}>Voertuig toevoegen</button>
                <br></br>
            </div>
            
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
        </div>

        <GeneralFooter>
        </GeneralFooter>
        </>
    );
}

export default AddVehicle;
