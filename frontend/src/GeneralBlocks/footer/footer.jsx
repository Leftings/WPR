import React from 'react';
import { Link } from 'react-router-dom';
import TermsAndConditions from "../../TermsAndConditions/TermsAndConditions.jsx";
import './footer.css'; 

function GeneralFooter() {
    return (
        <>
            <footer className="footer">
                <p>&copy; 2024 CarAndAll. All rights reserved.</p>
                <div className="footer-links">
                    <Link to="/terms">Terms & Conditions</Link>
                    <Link to="/Terms&conditions">Privacy Policy</Link>
                </div>
            </footer>
        </>
    );
}

export default GeneralFooter;