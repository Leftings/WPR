import React from 'react';
import { Link } from 'react-router-dom';
import TermsAndConditions from "../../GeneralSalePage/GeneralSalePage.jsx";
import './header.css'; 

function GeneralHeader() {
    return (
        <>
            <header className="header">
                <div id="left">
                    <h2 className="logo">CarAndAll</h2>
                </div>
                <nav id="right">
                    <ul className="nav-links">
                        <li><Link to="/#">Logout</Link></li>
                        <li><Link to="/GeneralSalePage">Zoek Auto's</Link></li>
                        <li><Link to="/about">Over ons</Link></li>
                        <li><Link to="/contact">Contact</Link></li>
                    </ul>
                </nav>
            </header>
        </>
    );
}

export default GeneralHeader;
