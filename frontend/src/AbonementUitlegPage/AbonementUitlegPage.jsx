import React, { useState, useEffect } from 'react';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
//import './AbonementUitlegPage.css';
import '../index.css';

function AbonementUitlegPage() { // Corrected component name
    const [isLoggedIn, setIsLoggedIn] = useState(false);

    useEffect(() => {
        fetch('http://localhost:5165/api/Login/CheckSession', { credentials: 'include' })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Not logged in');
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
                            <p>
                                Het Pay as You Go-abonnement is ideaal voor wie flexibel wil blijven.
                                Voor een vast maandelijks bedrag profiteer je van een aantrekkelijke procentuele korting op alle huurbedragen.
                                Of je nu af en toe een auto nodig hebt of regelmatig huurt, deze optie biedt je de vrijheid om te kiezen.
                            </p>
                        </div>
                        <div className="abonnementen-card">
                            <h3>Abonnement met Huurdagen</h3>
                            <p>
                                Het abonnement met huurdagen is ontworpen voor frequente huurders. Je betaalt vooraf een vast bedrag voor een specifiek aantal huurdagen,
                                wat je rust en kostenbesparingen oplevert. Ideaal voor wie vaak op pad is en een auto nodig heeft tegen een voordelige prijs.
                            </p>
                        </div>
                    </section>
                </div>
            </main>
            <GeneralFooter />
        </>
    );
}

export default AbonementUitlegPage;
