import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './employeeFrontOffice.css';
import GeneralHeader from '../GeneralBlocks/header/header';
import GeneralFooter from '../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL_EMPLOYEE ?? 'http://localhost:5276';

function EmployeeFrontOffice() {
  const navigate = useNavigate();

  useEffect(() => {
    // Authorisatie check
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

      <body>
        <h1>Front Office</h1>
        <Link to="./reviewHireRequest">Huur aanvragen beheren</Link>
        <Link to="./addBusiness">Bedrijf toevoegen</Link>
      </body>

      <GeneralFooter>
      </GeneralFooter>
    </>
  );
}

export default EmployeeFrontOffice;
