import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import '../index.css';
import GeneralHeader from '../GeneralBlocks/header/header';
import GeneralFooter from '../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function VehicleManager() {
  const navigate = useNavigate();

  useEffect(() => {
    fetch(`${BACKEND_URL}/api/Cookie/GetUserId` , {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
        credentials: 'include',
    })
        .then(response => {
            if (!response.ok) {
                return response.json().then(data => {
                  console.log(data);
                  throw new Error(data?.message || 'No Cookie'); 
                });
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
        <h1>Wagenpark Beheerder</h1>
        
        <div className='officeLinks'>
          <Link to="./reviewHireRequest">Huur aanvragen beheren</Link>
          <Link to="./changeBusinessSettings">Aanpassen Bedrijfsaccounts</Link>
        </div>
      </div>

      <GeneralFooter>
      </GeneralFooter>
    </>
  );
}

export default VehicleManager;
