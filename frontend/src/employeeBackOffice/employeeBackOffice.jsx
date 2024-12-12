import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './employeeBackOffice.css';
import GeneralHeader from '../GeneralBlocks/header/header';
import GeneralFooter from '../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL_EMPLOYEE ?? 'http://localhost:5276';

function EmployeeBackOffice() {
  return (
    <>
      <GeneralHeader>
      </GeneralHeader>

      <div className='body'>
        <h1>Back Office</h1>

        <div id='backOfficeLinks'>
          <Link to="./addEmployee">Werknemer toevoegen</Link>
          <Link to="./addVehicle">Voertuig toevoegen</Link>
        </div>
      </div>

      <GeneralFooter>
      </GeneralFooter>
    </>
  );
}

export default EmployeeBackOffice;
