import React, { useState, useEffect } from 'react';
import {Link, useNavigate} from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import GeneralSalePage from "../GeneralSalePage/GeneralSalePage.jsx";

import './home.css';

function Home() {
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        fetch('http://localhost:5165/api/Login/CheckSession', { credentials: 'include' })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Not logged in')
                }
                return response.json();
            })
            .then(() => setIsLoggedIn(true))
            .catch(() =>setIsLoggedIn(false));
    }, []);

    const handleLogout = () => {
        fetch('http://localhost:5165/api/Cookie/Logout', { method: 'POST', credentials: 'include' })
            .then(() => {
                setIsLoggedIn(false);
                navigate('/login');
            })
            .catch(error => console.error('Logout error', error));
    };

    return (
        <>
            <GeneralHeader />
            <main>
                <section className="hero">
                    <h1>Vind de perfecte auto voor jouw avontuur</h1>
                    <p>Betaalbare prijzen, flexibele verhuur en een breed aanbod aan voertuigen om uit te kiezen.</p>
                    <Link to="/GeneralSalePage" className="cta-button">Verken onze Auto's</Link>
                </section>

                <div className="container">
                    <section className="features">
                        <div className="feature-card">
                            <h3>Grote selectie</h3>
                            <p>Van sedans tot SUVs, we hebben een auto voor elke gelegenheid.</p>
                        </div>
                        <div className="feature-card">
                            <h3>De beste prijzen</h3>
                            <p>Concurrerende prijzen die binnen uw budget passen.</p>
                        </div>
                        <div className="feature-card">
                            <h3>Flexibele verhuuropties</h3>
                            <p>Kies een huurperiode die perfect bij uw situatie past.</p>
                        </div>
                    </section>
                </div>
            </main>
            <GeneralFooter />
        </>
    );
}
export default Home;