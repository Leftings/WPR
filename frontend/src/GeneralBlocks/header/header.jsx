import React from 'react';
import { Link } from 'react-router-dom';
import './header.css'; 

function GeneralHeader({ isLoggedIn, handleLogout}) {
    return (
        <>
            <header className="header">
                <div id="left">
                    <h2 className="logo">CarAndAll</h2>
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
                    </ul>
                </nav>
            </header>
        </>
    );
}

export default GeneralHeader;
