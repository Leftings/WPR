import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './login.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function CheckCookie()
{
  fetch(`${BACKEND_URL}/api/Login/CheckSession`, { credentials: 'include' })
    .then(response => {
      if (!response.ok)
      {
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
    })
}
function Login() {
  const [isEmployee, setIsEmployee] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const toggleUserType = () => {
    setIsEmployee(!isEmployee);
  };

  // Verbinden met API voor login
  const onSubmit = (event) => {
    event.preventDefault(); 

    if (!email || !password) {
        if (!email && !password)
        {
          setError('Vul een emailadres en wachtwoord in');
        }
        else if (!email)
        {
          setError('Vul een emailadres in');
        }
        else
        {
          setError('Vul een wachtwoord in');
        }
        return;
    }

    fetch(`${BACKEND_URL}/api/Login/login`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json', 
        },
        body: JSON.stringify({ email, password, isEmployee }),
        credentials: 'include', // Cookies of authenticatie wordt meegegeven
    })
    .then(response => {
        if (!response.ok) {
            setError(`${email} is geen geldig account of het wachtwoord klopt niet`);
            throw new Error('Login failed');
        }
        return response.json();
    })
    .then(async data => {
        if (await CheckCookie() !== null)
        {

          console.log('Login successful', data);
          navigate('/');
        }

        return 'Cookie error';

    })
    .catch(error => {
        console.error('Error:', error);
    });
};

  return (
    <>
      <header>
        <div id="left">
        </div>
        <div id="carLink">
            <Link to="/">CarAndAll</Link>
        </div>

        <div id="right">
          <Link to="#" onClick={toggleUserType}>
            {isEmployee ? 'Klant' : 'Medewerker'} {/* als isEmployee = true, Klant wordt getoond, als isEmployee = false, Medewerker wordt getoond*/}
          </Link>
        </div>
      </header>

      <div>
        {isEmployee ? (
          <div id="inlog">
            <h1>Medewerkers Login</h1>
          </div>
        ) : (
          <div id="inlog">
            <h1>Klanten Login</h1>
          </div>
        )}

        <div id="input">
          <label htmlFor="user">Email</label>
          <br></br>
          <input type="text" id="user" value={email} onChange={(e) => setEmail(e.target.value)}></input>
          <br></br>
          <label htmlFor="password">Wachtwoord</label>
          <br></br>
          <input type="password" id="password" value={password} onChange={(e) => setPassword(e.target.value)}></input>
          <br></br>
          <button id="button" type="button"onClick={onSubmit} >Login</button>
            {!isEmployee && (
            <>
            <br></br>
            <label htmlFor="noAccount">Nog geen account bij ons? <Link to="/signUp">Meld nu aan!</Link></label>
            </>
            )}
          {error && <p style={{ color: 'red' }}>{error}</p>}
        </div>
      </div>

      <footer></footer>
    </>
  );
}

export default Login;
