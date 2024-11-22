import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import './home.css';

function WelcomeUser(setWelcome)
{
  fetch('http://localhost:5165/api/Home/GetUserName', {
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
  const [welcome, setWelcome] = useState(null);

  useEffect(() => {
    WelcomeUser(setWelcome);
  }, []);

  return (
    <>
      <header>
        <div id="left">
        </div>

        <div id="right">
        <Link to="/userSettings" id="user" >{welcome}</Link>
        </div>
      </header>

      <div>
        <h1>Home</h1>
      </div>

      <footer></footer>
    </>
  );
}

export default Home;
