import React from 'react';
import { Link } from 'react-router-dom';
import TermsAndConditions from "../../TermsAndConditions/TermsAndConditions.jsx";
import '../../index.css';

function GeneralFooter() {
    return (
        <>
            <footer className="footer">
                <p>&copy; 2024 CarAndAll. Alle rechten voorbehouden.</p>
                <div className="footer-links">
                    <Link to="/TermsAndConditions">Algemene Voorwaarde</Link>
                    <Link to="/Policy">Privacyverklaring</Link>
                </div>
            </footer>
        </>
    );
}

export default GeneralFooter;
