
import React from 'react';
import { Link } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";

import './home.css';

function WelcomeUser(setWelcome)
{
  fetch('http://localhost:5165/api/Cookie/GetUserName', {
    method: 'GET',
    headers: {
        'Content-Type': 'application/json', 
    },
    credentials: 'include', // Cookies of authenticatie wordt meegegeven
    })
    .then(response => {
        console.log(response);
        if (!response.ok) {
            throw new Error('No Cookie');
        }
        return response.json();
    })
    .then(async data => {
      setWelcome(`Welcome, ${data.message}`);
    })
    .catch(error => {
        console.error('Error:', error);
    });
}
function Home() {
    return (
        <>
            <GeneralHeader /> 


            <main>
                <section className="hero">
                    <h1>Vindt de perfecte auto voor jouw avontuur</h1>
                    <p>Betaalbare prijzen, flexibele verhuur en een breed aanbod aan voertuigen om uit te kiezen.</p>
                    <Link to="/cars" className="cta-button">Verken onze Auto's</Link>
                </section>

                <div className="container">
                    <section className="features">
                        <div className="feature-card">
                            <h3>Grootte selectie</h3>
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
