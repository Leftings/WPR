import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import '../index.css';

function Home() {
    const [isEmployee, setIsEmployee] = useState(false);
    const [isVehicleManager, setIsVehicleManager] = useState(false);
    const [error, setError] = useState(null);
    const navigate = useNavigate();

    // Effect om te controleren of de gebruiker een voertuigbeheerder is
    useEffect(() => {
        const checkVehicleManager = async () => {
            try {
                // API-aanroep om te controleren of de gebruiker een voertuigbeheerder is
                const response = await fetch('http://localhost:5276/api/Cookie/IsVehicleManager', {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    credentials: 'include', // Zorgt ervoor dat cookies worden meegezonden
                });

                // Controleer of de respons succesvol was
                if (!response.ok) {
                    throw new Error('Error fetching Vehicle Manager info...');
                }

                // Respons omzetten naar JSON en controleren of gebruiker een voertuigbeheerder is
                const data = await response.json();
                setIsVehicleManager(data === true);
                console.log(isVehicleManager);
            } catch (error) {
                // Log de fout en zet `isVehicleManager` naar false
                console.error(error.message);
                setIsVehicleManager(false);
            }
        };

        // Roep de functie aan om de voertuigbeheerderstatus te controleren
        checkVehicleManager();
    }, []);

    // Effect om te controleren of de gebruiker een werknemer is
    useEffect(() => {
        fetch(`http://localhost:5165/api/Employee/IsUserEmployee`, { credentials: 'include' })
            .then(response => {
                // Controleer of de respons succesvol was
                if (!response.ok) {
                    throw new Error('Error validating user type');
                }
                return response.text(); // Converteer de respons naar tekst
            })
            .then(data => {
                // Controleer of de gebruiker een werknemer is (op basis van de backend-respons)
                const isUserEmployee = data === 'true';
                setIsEmployee(isUserEmployee);
            })
            .catch(error => {
                // Log de fout en zet `isEmployee` naar false
                console.error(error.message);
                setIsEmployee(false);
            });
    }, []); 

    return (
        <>
            <GeneralHeader />
            <main>
                <section className="hero">
                    <h1>Vind de perfecte auto voor jouw avontuur</h1>
                    <p>Betaalbare prijzen, flexibele verhuur en een breed aanbod aan voertuigen om uit te kiezen.</p>
                    <Link to="/vehicles" className="cta-button">Verken onze Auto's</Link>
                </section>

                <div className="container">
                    <section className="features">
                        <div className="feature-card">
                            <h3>Grote selectie</h3>
                            <p>Van sedans tot SUVs, we hebben een auto voor elke gelegenheid.</p>
                        </div>
                        <div className="feature-card">
                            <h3>De beste prijzen</h3>
                            <p>Concurrerende prijzen die binnen uw budget passen.</p>
                        </div>
                        <div className="feature-card">
                            <h3>Flexibele verhuuropties</h3>
                            <p>Kies een huurperiode die perfect bij uw situatie past.</p>
                        </div>
                    </section>


                    {isEmployee && (
                        <section className="abonnementen-info">
                            <h2>Bekijk onze Abonnementen</h2>
                            <p>We bieden verschillende abonnementsopties aan die passen bij jouw huurbehoeften. Bekijk
                                ze en kies de beste optie voor jou!</p>
                            <Link to="/AbonementUitlegPage" className="cta-button">Ontdek Abonnementen</Link>
                        </section>
                    )}

                    {isVehicleManager && (
                    <section className="abonnementen-info">
                        <h2>Bekijk het overzicht van alle Rentals</h2>
                        <p>Op de volgende pagina zult u een uitgebreide pagina vinden waarin alle verhuurde auto's te
                            vinden zijn</p>
                        <Link to="/wagenparkBeheerderOverzichtPage" className="cta-button">Bekijk Rentals</Link>
                    </section>
                    )}
                </div>
            </main>
            <GeneralFooter/>
        </>
    );
}

export default Home;
