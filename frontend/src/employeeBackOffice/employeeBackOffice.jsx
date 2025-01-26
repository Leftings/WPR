import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import '../index.css';
import GeneralHeader from '../GeneralBlocks/header/header';
import GeneralFooter from '../GeneralBlocks/footer/footer';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

// Hoofd component voor EmployeeBackOffice
function EmployeeBackOffice() {
    const navigate = useNavigate(); // Hook voor programmatische navigatie

    // useEffect hook om de autorisatie te controleren bij het laden van de component
    useEffect(() => {
        // Fetch verzoek om de gebruikerssessie te valideren met cookies
        fetch(`${BACKEND_URL}/api/Cookie/GetUserId`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json', // Stel de content type in op JSON
            },
            credentials: 'include', // Verstuur cookies mee met het verzoek
        })
            .then(response => {
                if (!response.ok) { // Als de response niet ok is (statuscode niet 200-299)
                    throw new Error('No Cookie'); // Gooi een fout als de cookie validatie mislukt
                }
                return response.json(); // Parse de response en retourneer als JSON
            })
            .catch(() => {
                // Als de fetch mislukt (bijvoorbeeld geen geldige cookie of netwerkprobleem)
                alert("Cookie was niet geldig"); // Waarschuw de gebruiker dat de cookie ongeldig is
                navigate('/'); // Navigeer de gebruiker naar de startpagina (of inlogpagina)
            })
    }, [navigate]); // De dependency array zorgt ervoor dat de effect alleen bij de eerste render wordt uitgevoerd

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
