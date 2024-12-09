import React, {useState} from 'react';
import { Link } from 'react-router-dom';
import TermsAndConditions from "../../GeneralSalePage/GeneralSalePage.jsx";
import logo from '../../assets/logo.svg';
import logoHover from '../../assets/logo-green.svg';

import './header.css';


function GeneralHeader({ isLoggedIn, handleLogout}) {
    const [isMenuOpen, setIsMenuOpen] = useState(false);

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
                            <li>
                                <Link onClick={handleLogout} className="logout-button">Logout</Link>
                            </li>
                        ) : (
                            <li>
                                <Link to="/login">Login</Link>
                            </li>
                        )}
                        <li><Link to="/GeneralSalePage">Zoek Auto's</Link></li>
                        <li><Link to="/about">Over ons</Link></li>
                        <li><Link to="/contact">Contact</Link></li>
                        <li><Link to="/userSettings">Instellingen</Link></li>
                    </ul>
                </nav>
            ) : null}
        </>
    );
}

export default GeneralHeader;
