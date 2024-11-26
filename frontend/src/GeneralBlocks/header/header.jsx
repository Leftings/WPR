import React from 'react';
import { Link } from 'react-router-dom';
import logo from '../../assets/logo.svg';
import './header.css'; 

function GeneralHeader({ isLoggedIn, handleLogout}) {
    return (
        <>
            <header className="header">
                <div id="left" className="logo-container">
                    <img src={logo} alt="Car And All Logo" className="logo-image" />
                    <h1 className="logo">Car And All</h1>
                </div>
                <nav id="right">
                    <ul className="nav-links">
                        {isLoggedIn ? (
                            <li>
                            <button onClick={handleLogout} className="logout-button">Logout</button>
                            </li>
                        ) : (
                            <li>
                                <Link to="/login">Login</Link>
                            </li>
                        )}
                        <li><Link to="/cars">Zoek Auto's</Link></li>
                        <li><Link to="/about">Over ons</Link></li>
                        <li><Link to="/contact">Contact</Link></li>
                        <lis><Link to="/userSettings">Instellingen</Link></lis>
                    </ul>
                </nav>
            </header>
        </>
    );
}

export default GeneralHeader;
