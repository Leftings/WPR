import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './abonnementen.css'; 

function Abonnementen() {
    const [isLoggedIn, setIsLoggedIn] = useState(false);

    useEffect(() => {
        fetch('http://localhost:5165/api/Login/CheckSession', { credentials: 'include' })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Not logged in')
                }
                return response.json();
            })
            .then(() => setIsLoggedIn(true))
            .catch(() => setIsLoggedIn(false));
    }, []);

    return (
        <>
            <GeneralHeader isLoggedIn={isLoggedIn} />
            <main>
                <section className="hero">
                    <h1>Ontdek onze Abonnementen</h1>
                    <p>Keuze uit verschillende abonnementsopties die passen bij jouw behoeften.</p>
                </section>

                <div className="container">
                    <section className="abonnementen-features">
                        <div className="abonnementen-card">
                            <h3>Pay as You Go Abonnement</h3>
                            <p>Betaal maandelijks een vast bedrag en ontvang een procentuele korting op de huurbedragen. Perfect voor flexibele huurders.</p>
                            <Link to="/abonnementen/payg" className="cta-button">Meer informatie</Link>
                        </div>
                        <div className="abonnementen-card">
                            <h3>Abonnement met Huurdagen</h3>
                            <p>Betaling vooraf voor een vast aantal huurdagen. Ideaal voor degenen die regelmatig een auto nodig hebben tegen een voordelige prijs.</p>
                            <Link to="/abonnementen/huurdagen" className="cta-button">Meer informatie</Link>
                        </div>
                    </section>
                </div>
            </main>
            <GeneralFooter />
        </>
    );
}

export default Abonnementen;
