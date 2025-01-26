import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import TermsAndConditions from "../../GeneralSalePage/GeneralSalePage.jsx";
import logo from '../../assets/logo.svg';
import logoHover from '../../assets/logo-green.svg';
import '@fortawesome/fontawesome-free/css/all.css';

import '../../index.css';

function GeneralHeader() {
    const [isLoggedIn, setIsLoggedIn] = useState(false); // State om bij te houden of de gebruiker is ingelogd
    const [isMenuOpen, setIsMenuOpen] = useState(false); // State om de status van het menu (open of gesloten) bij te houden
    const navigate = useNavigate(); // Hook van react-router-dom om navigatie te beheren

    // useEffect wordt uitgevoerd bij het laden van de component
    useEffect(() => {
        // Checkt of er een geldige sessie bestaat bij het laden van de pagina
        fetch('http://localhost:5165/api/Login/CheckSession', { credentials: 'include' })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Not logged in'); // Foutmelding als de gebruiker niet is ingelogd
                }
                return response.json();
            })
            .then(() => setIsLoggedIn(true)) // Zet de isLoggedIn state op true als de gebruiker is ingelogd
            .catch(() => setIsLoggedIn(false)); // Zet de isLoggedIn state op false als er een fout is
    }, []); // De useEffect wordt maar één keer uitgevoerd (bij het laden van de component)

    // Functie voor het uitloggen van de gebruiker
    const handleLogout = () => {
        // Verstuurt een POST-verzoek naar de backend om de sessie van de gebruiker te beëindigen
        fetch('http://localhost:5165/api/Cookie/Logout', { method: 'POST', credentials: 'include' })
            .then(() => {
                setIsLoggedIn(false); // Zet de isLoggedIn state op false
                navigate('/login'); // Navigeer de gebruiker naar de loginpagina
            })
            .catch(error => console.error('Logout error', error)); // Log eventuele fouten
    };

    // Functie om het menu te openen of te sluiten
    const toggleMenu = () => {
        setIsMenuOpen(!isMenuOpen); // Wijzig de waarde van isMenuOpen (true/false)
    };

    return (
        <>
            <link rel="preload" as="image" href={logoHover} />
            <div className="header-container">
                <header className="header">
                    <Link to="/">
                        <div id="left" className="logo-container">
                            <img src={logo} alt="Car And All Logo" className="logo-image"/>
                            <h1 className="logo">Car And All</h1>
                        </div>
                    </Link>
                    <button id = "right" className="hamburger-menu" onClick={toggleMenu}>
                        &#9776; {/* Unicode for hamburger icon */}
                    </button>
                </header>
                {isMenuOpen ? (
                    <nav>
                        <ul className="nav-links">
                            {isLoggedIn ? (
                                <>
                                <li><Link onClick={handleLogout} className="logout-button">Logout</Link></li>
                                <li><Link to="/overviewRental">Mijn auto's</Link></li>
                                <li><Link to="/userSettings">Instellingen</Link></li>
                                </>
                            ) : (
                                <li>
                                    <Link to="/login">Login</Link>
                                </li>
                            )}
                            <li><Link to="/abonnement">Abonnement</Link> </li>
                            <li><Link to="/vehicles">Zoek Auto's</Link></li>
                            <li><Link to="/about">Over ons</Link></li>
                            <li><Link to="/contact">Contact</Link></li>
                        </ul>
                    </nav>
                ) : null}
            </div>
        </>
    );
}

export default GeneralHeader;
