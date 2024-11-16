import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './login.css';

function Login() {
  // State for username, password, error message, and user type
  const [isEmployee, setIsEmployee] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  // Toggle between employee and customer views
  const toggleUserType = () => {
    setIsEmployee(!isEmployee);
  };

  // Handle login form submission
  const onSubmit = (event) => {
    event.preventDefault(); // Prevent default form submission

    if (!email || !password) {
        alert("Fill in email and password");
        return;
    }

    fetch('http://localhost:5165/api/Login/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json', // Set content type to JSON
        },
        body: JSON.stringify({ email, password, isEmployee }), // Send email and password as the request body
        credentials: 'include', // Include cookies or authentication credentials if needed
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Login failed');
        }
        return response.json();
    })
    .then(data => {
        console.log('Login successful', data);
        navigate('/home');
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

        <div id="right">
          <Link to="#" onClick={toggleUserType}>
            {isEmployee ? 'Klant' : 'Medewerker'}
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
          <label htmlFor="user">Gebruiker</label>
          <br></br>
          <input type="text" id="user" value={email} onChange={(e) => setEmail(e.target.value)}></input>
          <br></br>
          <label htmlFor="password">Wachtwoord</label>
          <br></br>
          <input type="password" id="password" value={password} onChange={(e) => setPassword(e.target.value)}></input>
          <br></br>
          <button id="button" type="button"onClick={onSubmit} >Login</button>
          {error && <p style={{ color: 'red' }}>{error}</p>}
        </div>
      </div>

      <footer></footer>
    </>
  );
}

export default Login;
