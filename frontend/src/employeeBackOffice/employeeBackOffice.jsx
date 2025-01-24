import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import '../index.css';
import GeneralHeader from '../GeneralBlocks/header/header';
import GeneralFooter from '../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function EmployeeBackOffice() {
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
        <h1>Back Office</h1>

        <div className='officeLinks'>
            <Link to="./viewRentalData">Verhuur inzicht</Link>
            <Link to="./addEmployee">Werknemer toevoegen</Link>
            <Link to="./addVehicle">Voertuig toevoegen</Link>
            <Link to="/vehicles">Voertuig verwijderen</Link>
            <Link to="./addSubscription">Abonnement toevoegen</Link>
            <Link to="/abonnement">Abonnement verwijderen</Link>
        </div>
      </main>

      <GeneralFooter>
      </GeneralFooter>
    </>
  );
}

export default EmployeeBackOffice;
