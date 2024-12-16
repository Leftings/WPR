import React, {useEffect, useState} from 'react';
import { Link, useNavigate } from 'react-router-dom';
import TermsAndConditions from "../../GeneralSalePage/GeneralSalePage.jsx";
import logo from '../../assets/logo.svg';
import logoHover from '../../assets/logo-green.svg';

import './header.css';


function GeneralHeader() {
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const navigate = useNavigate();

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

    const handleLogout = () => {
        fetch('http://localhost:5165/api/Cookie/Logout', { method: 'POST', credentials: 'include' })
            .then(() => {
                setIsLoggedIn(false);
                navigate('/login');
            })
            .catch(error => console.error('Logout error', error));
    };

    const toggleMenu = () => {
        setIsMenuOpen(!isMenuOpen);
    };

    return (
        <>
            <link rel="preload" as="image" href={logoHover} />
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
                        <li><Link to="/GeneralSalePage">Zoek Auto's</Link></li>
                        <li><Link to="/about">Over ons</Link></li>
                        <li><Link to="/contact">Contact</Link></li>
                    </ul>
                </nav>
            ) : null}
        </>
    );
}

export default GeneralHeader;
