import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
//import './employeeFrontOffice.css';
import '../index.css';
import GeneralHeader from '../GeneralBlocks/header/header';
import GeneralFooter from '../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

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
      
      <main>
        <h1>Front Office</h1>
        
        <div className = "officeLinks">
          <Link to="./reviewHireRequest">Huur aanvragen beheren</Link>
          <Link to="./reviewBusiness">Bedrijf aanvragen beheren</Link>
            <Link to="./intakeForm">Innameformulier invullen</Link>
        </div>
      </main>

      <GeneralFooter>
      </GeneralFooter>
    </>
  );
}

export default EmployeeFrontOffice;
