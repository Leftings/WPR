import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './StaffManagerTools.css';

function StaffTools() {
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const navigate = useNavigate();

    return (
        <>
            <GeneralHeader isLoggedIn={isLoggedIn} handleLogout={handleLogout} />
            <main>
                <section className="hero">
                    <h1>Staff Manager Tools</h1>
                    <Link to="/GeneralSalePage" className="cta-button">Voeg auto's toe aan systeem</Link>
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

                    <section className="abonnementen-info">
                        <h2>Bekijk onze Abonnementen</h2>
                        <p>We bieden verschillende abonnementsopties aan die passen bij jouw huurbehoeften. Bekijk ze en kies de beste optie voor jou!</p>
                        <Link to="/AbonementUitlegPage" className="cta-button">Ontdek Abonnementen</Link>
                    </section>
                </div>
            </main>
            <GeneralFooter />
        </>
    );
}

export default StaffTools;
