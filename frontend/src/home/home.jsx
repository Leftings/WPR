import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './home.css';

function Home() {
    const [isEmployee, setIsEmployee] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        fetch(`http://localhost:5165/api/Employee/IsUserEmployee`, { credentials: 'include' })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error validating user type');
                }
                return response.text();
            })
            .then(data => {
                const isUserEmployee = data === 'true';
                setIsEmployee(isUserEmployee);
            })
            .catch(error => {
                console.error(error.message);
                setIsEmployee(false);
            });
    }, []);

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


                    {isEmployee && (
                        <section className="abonnementen-info">
                            <h2>Bekijk onze Abonnementen</h2>
                            <p>We bieden verschillende abonnementsopties aan die passen bij jouw huurbehoeften. Bekijk ze en kies de beste optie voor jou!</p>
                            <Link to="/AbonementUitlegPage" className="cta-button">Ontdek Abonnementen</Link>
                        </section>
                    )}
                </div>
            </main>
            <GeneralFooter />
        </>
    );
}

export default Home;
