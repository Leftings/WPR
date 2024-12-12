import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './employeeFrontOffice.css';
import GeneralHeader from '../GeneralBlocks/header/header';
import GeneralFooter from '../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL_EMPLOYEE ?? 'http://localhost:5276';

function EmployeeFrontOffice() {
  return (
    <>
      <GeneralHeader>
      </GeneralHeader>

      <body>
        <h1>Front Office</h1>
        <Link to="./reviewHireRequest">Huur aanvragen beheren</Link>
      </body>

      <GeneralFooter>
      </GeneralFooter>
    </>
  );
}

export default EmployeeFrontOffice;
