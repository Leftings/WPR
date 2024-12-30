import React, { useState } from 'react';
import { isRouteErrorResponse, Link, useNavigate } from 'react-router-dom';
import './login.css';
import logo from '../assets/logo.svg';
import logoHover from '../assets/logo-green.svg';
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function CheckCookie() {
  return fetch(`${BACKEND_URL}/api/Login/CheckSession`, { credentials: 'include' })
    .then(response => {
      if (!response.ok) {
        throw new Error('Session check failed');
      }
      return response.json();
    })
    .then(data => {
      console.log('Session Active: ', data);
      return data;
    })
    .catch(error => {
      console.error('Error: ', error.message);
      return null;
    });
}

function CheckCookieEmployee()
{
  return fetch(`${BACKEND_URL}/api/Login/CheckSessionStaff`, { credentials: 'include' })
    .then(response => {
      if (!response.ok) {
        throw new Error('Session check failed');
      }
      return response.json();
    })
    .then(data => {
      console.log('Session Active: ', data);
      return data;
    })
    .catch(error => {
      console.error('Error: ', error.message);
      return null;
    });
}

function GetOffice() {
  console.log('Backend URL:', BACKEND_URL);
  console.log('Sending request to GetKindEmployee');
  return fetch(`${BACKEND_URL}/api/Cookie/GetKindEmployee`, { credentials: 'include' })
    .then(response => {
      if (!response.ok) {
        throw new Error('Getting kind of office failed');
      }
      return response.json();
    })
    .then(data => {
      console.log('Kind office: ', data);
      return data; // The office type (e.g., "Front" or "Back")
    })
    .catch(error => {
      console.error('Error: ', error.message);
      return null;
    });
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

    try
    {
      const response = await fetch(`${BACKEND_URL}/api/Login/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({email, password, userType}),
        credentials: 'include',
      });

      if (!response.ok)
      {
        setError(`${email} is geen geldig account of het wachtwoord klopt niet`);
        throw new Error('Login failed');
      }
      else
      {
        if (userType === 'Customer')
        {
          // klant wordt ingelogd terug naar de homepage gestuurd
          await fetch(`${BACKEND_URL}/api/Login/CheckSession`, { credentials: 'include' });
          navigate('/');
        }
        else if (userType === 'Employee')
        {
          // Soort medewerker wordt vastgesteld
          const officeResponse = await fetch(`${BACKEND_URL}/api/Cookie/GetKindEmployee`, { credentials: 'include' });
          if (!officeResponse.ok) {
              throw new Error('Failed to fetch the kind of office');
          }
          const office = await officeResponse.json();
          
          if (office?.message === 'Front')
          {
            // Medewerker wordt naar de frontoffice gestuurd
            await fetch(`${BACKEND_URL}/api/Login/CheckSessionStaff`, { credentials: 'include' });
            navigate('/FrontOfficeEmployee');
          }
          else
          {
            // Medewerker wordt naar de backoffice gestuurd
            await fetch(`${BACKEND_URL}/api/Login/CheckSessionStaff`, { credentials: 'include' });
            navigate('/BackOfficeEmployee');
          }
        }
        else
        {
          // Vehicle Manager wordt naar Vehicle Manager gestuurd
          await fetch(`${BACKEND_URL}/api/Login/CheckSessionVehicleManager`, { credentials: 'include' });
          navigate('/VehicleManager');
        }
      }
    }
    catch (error)
    {
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
              <img src={logo} alt="Car And All Logo" className="logo-image"/>
              <h1 className="logo">Car And All</h1>
            </div>
          </Link>
          <button id = "right" className="hamburger-menu" onClick={toggleMenu}>
            &#9776; {/* Unicode for hamburger icon */}
          </button>
        </header>
        {isMenuOpen ? (
            <nav>
              <ul className="nav-links">
                <li><Link to="/GeneralSalePage">Zoek Auto's</Link></li>
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
            <br/>
            <input type="text" id="user" value={email} onChange={(e) => setEmail(e.target.value)}/>
            <br/>
            <label htmlFor="password">Wachtwoord</label>
            <br/>
            <input type="password" id="password" value={password} onChange={(e) => setPassword(e.target.value)}/>
            <br/>
            <button id="button" type="button" onClick={onSubmit}>Login</button>
            {userType === "Customer" && (
                <>
                  <br/>
                  <label htmlFor="noAccount">
                    Nog geen account bij ons? <Link to="/signUp">Meld nu aan!</Link>
                  </label>
                </>
            )}
            {error && <p style={{color: 'red'}}>{error}</p>}
          </div>
        </main>

        <GeneralFooter/>
      </>
  );
}

  export default Login;
