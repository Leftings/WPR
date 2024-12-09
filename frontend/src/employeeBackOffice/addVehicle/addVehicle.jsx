import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './addVehicle.css';
import GeneralFooter from '../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL_EMPLOYEE ?? 'http://localhost:5276';

function AddVehicle() {
  return (
    <>
      <GeneralHeader>
      </GeneralHeader>

      <body>
        <h1>Toevoegen voertuig</h1>
      </body>

      <GeneralFooter>
      </GeneralFooter>
    </>
  );
}

export default AddVehicle;
