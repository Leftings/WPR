import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './addVehicle.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL_EMPLOYEE ?? 'http://localhost:5276';

function AddVehicle() {
  return (
    <>
      <GeneralHeader>
      </GeneralHeader>

      <body>
        <h1>Toevoegen voertuig</h1>
        <div id="kind">
            <p>Soort voertuig</p>
            <select name="vehicle">
                <option value="car">Auto</option>
                <option value="camper">Camper</option>
                <option value="caravan">Caravan</option>
            </select>
            <br></br>
        </div>
        <div id="brand">
            <p>Merk voertuig</p>
            <input></input>
            <br></br>
        </div>
        <div id="licensPlate">
            <p>Nummerbord voertuig</p>
            <input></input>
            <br></br>
        </div>
        <div id="YoP">
            <p>Bouwjaar voertuig</p>
            <input type="number"></input>
            <br></br>
        </div>
        <div id="price">
            <p>Prijs per dag</p>
            <input type="number"></input>
            <br></br>
        </div>
        <div id="description">
            <p>Omschrijving voertuig</p>
            <input></input>
            <br></br>
        </div>
        <div id="vehicleBlob">
            <p>Afbeelding voertuig (verplict)</p>
            <input type="file"></input>
            <br></br>
        </div>
        <div id="confirm">
            <button>Voertuig toevoegen</button>
            <br></br>
        </div>
      </body>

      <GeneralFooter>
      </GeneralFooter>
    </>
  );
}

export default AddVehicle;
