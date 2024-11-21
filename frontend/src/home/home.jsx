import React from 'react';
import { Link } from 'react-router-dom';
import './home.css';

function Home() {
  return (
    <>
      <header className="header">
        <div id="left">
          <h2 className="logo">CarAndAll</h2>
        </div>
        <nav id="right">
          <ul className="nav-links">
            <li><Link to="/cars">Zoek Auto's</Link></li>
            <li><Link to="/about">Over ons</Link></li>
            <li><Link to="/contact">Contact</Link></li>
          </ul>
        </nav>
      </header>

      <main>
        <section className="hero">
          <h1>Vindt de perfecte auto voor jouw avontuur</h1>
          <p>Betaalbare prijzen, flexibele verhuur en een breed aanbod aan voertuigen om uit te kiezen.</p>
          <Link to="/cars" className="cta-button">Verken onze Auto's</Link>
        </section>

        <div class="container">
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


      <footer className="footer">
        <p>&copy; 2024 CarAndAll. All rights reserved.</p>
        <div className="footer-links">
          <Link to="/terms">Terms & Conditions</Link>
          <Link to="/privacy">Privacy Policy</Link>
        </div>
      </footer>
    </>
  );
}

export default Home;
