import React, { useState } from 'react';
import { isRouteErrorResponse, Link, useNavigate } from 'react-router-dom';
//import './login.css';
import '../index.css';
import logo from '../assets/logo.svg';
import logoHover from '../assets/logo-green.svg';
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

// Functie om te controleren of de gebruiker geblokkeerd is
function isUserBlocked() {
  const lastFailedAttempt = localStorage.getItem('lastFailedAttempt');
  const failedAttempts = parseInt(localStorage.getItem('failedAttempts') || '0', 10);
  const blockTime = .005 * 60 * 1000; // 5 minuten in milliseconden

  if (failedAttempts >= 10 && lastFailedAttempt) {
    const timeSinceLastAttempt = Date.now() - parseInt(lastFailedAttempt, 10);
    if (timeSinceLastAttempt < blockTime) {
      return true; 
    }
  }
  return false; 
}

// Functie om de mislukte poging bij te werken
function updateFailedLoginAttempt() {
  const failedAttempts = parseInt(localStorage.getItem('failedAttempts') || '0', 10) + 1;
  localStorage.setItem('failedAttempts', failedAttempts);
  localStorage.setItem('lastFailedAttempt', Date.now().toString());
}

// Functie om het aantal mislukte pogingen te resetten
function resetFailedLoginAttempts() {
  localStorage.removeItem('failedAttempts');
  localStorage.removeItem('lastFailedAttempt');
}

function Login() {
  const [userType, SetUserType] = useState('Customer');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const navigate = useNavigate();

  const toggleUserType = (event) => {
    SetUserType(event.target.value);
  };

  const onSubmit = async (event) => {
    event.preventDefault();

    if (isUserBlocked()) {
      setError('Teveel mislukte inlogpogingen. Probeer het over 5 minuten opnieuw.');
      return;
    }

    if (!email || !password) {
      if (!email && !password) {
        setError('Vul een emailadres en wachtwoord in');
      } else if (!email) {
        setError('Vul een emailadres in');
      } else {
        setError('Vul een wachtwoord in');
      }
      return;
    }

    try {
      const response = await fetch(`${BACKEND_URL}/api/Login/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password, userType }),
        credentials: 'include',
      });

      if (!response.ok) {
        // Verhoog de mislukte poging en update de tijd
        updateFailedLoginAttempt();
        setError(`${email} is geen geldig account of het wachtwoord klopt niet`);
        throw new Error('Login failed');
      } else {
        // Reset de mislukte inlogpogingen bij een succesvolle login
        resetFailedLoginAttempts();

        if (userType === 'Customer') {
          await fetch(`${BACKEND_URL}/api/Login/CheckSession`, { credentials: 'include' });
          navigate('/');
        } else if (userType === 'Employee') {
          const officeResponse = await fetch(`${BACKEND_URL}/api/Cookie/GetKindEmployee`, { credentials: 'include' });
          if (!officeResponse.ok) {
            throw new Error('Failed to fetch the kind of office');
          }
          const office = await officeResponse.json();
          console.log(office);

          if (office?.officeType === 'Front') {
            await fetch(`${BACKEND_URL}/api/Login/CheckSessionStaff`, { credentials: 'include' });
            navigate('/FrontOfficeEmployee');
          } else {
            await fetch(`${BACKEND_URL}/api/Login/CheckSessionStaff`, { credentials: 'include' });
            navigate('/BackOfficeEmployee');
          }
        } else {
          await fetch(`${BACKEND_URL}/api/Login/CheckSessionVehicleManager`, { credentials: 'include' });
          navigate('/VehicleManager');
        }
      }
    } catch (error) {
      console.error('Error: ', error);
    }
  };

  const toggleMenu = () => {
    setIsMenuOpen(!isMenuOpen);
  };

  return (
      <>
        <link rel="preload" as="image" href={logoHover} />
        <header className="header">
          <Link to="/">
            <div id="left" className="logo-container">
              <img src={logo} alt="Car And All Logo" className="logo-image" />
              <h1 className="logo">Car And All</h1>
            </div>
          </Link>
          <button id="right" className="hamburger-menu" onClick={toggleMenu}>
            &#9776; {/* Unicode for hamburger icon */}
          </button>
        </header>
        {isMenuOpen ? (
            <nav>
              <ul className="nav-links">
                <li><Link to="/vehicles">Zoek Auto's</Link></li>
                <li><Link to="/about">Over ons</Link></li>
                <li><Link to="/contact">Contact</Link></li>
              </ul>
            </nav>
        ) : null}

        <main>
          <div id="inlog">
            <h1>Login</h1>
            <div id="input">
              <label htmlFor="userType">Ik ben een:</label>
              <br></br>
              <select id="userType" value={userType} onChange={toggleUserType}>
                <option value="Customer">Klant</option>
                <option value="Employee">Medewerker van Car And All</option>
                <option value="VehicleManager">Wagenpark Beheerder</option>
              </select>
            </div>
          </div>

          <div id="input">
            <label htmlFor="user">Email</label>
            <br />
            <input type="text" id="user" value={email} onChange={(e) => setEmail(e.target.value)} />
            <br />
            <label htmlFor="password">Wachtwoord</label>
            <br />
            <input type="password" id="password" value={password} onChange={(e) => setPassword(e.target.value)} />
            <br />
            <button className="cta-button" type="button" onClick={onSubmit}>Login</button>
            {userType === "Customer" && (
                <>
                  <br />
                  <label htmlFor="noAccount">
                    Nog geen account bij ons? <Link to="/signUp">Meld nu aan!</Link>
                  </label>
                </>
            )}
            {error && <p style={{ color: 'red' }}>{error}</p>}
          </div>
        </main>

        <GeneralFooter />
      </>
  );
}

export default Login;
