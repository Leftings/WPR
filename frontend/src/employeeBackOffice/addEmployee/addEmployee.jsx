import React, { useState, useEffect } from 'react';
import './addEmployee.css';
import GeneralHeader from '../../GeneralBlocks/header/header';
import GeneralFooter from '../../GeneralBlocks/footer/footer';
import { useNavigate } from 'react-router-dom';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL_EMPLOYEE ?? 'http://localhost:5276';

function AddEmployee() {
    const navigate = useNavigate();
    const [KindEmployee, SetKind] = useState('Front');
    const [FirstName, SetFirstName] = useState('');
    const [LastName, SetLastName] = useState('');
    const [Password, SetPassword] = useState('');
    const [Email, SetEmail] = useState('');
    const [ErrorMessage, SetError] = useState([]);

    // Function to handle sign-up
    function SignUp() {
        let data = {
            Office: KindEmployee,
            FirstName: FirstName,
            LastName: LastName,
            Password: Password,
            Email: Email
        };

        fetch(`${BACKEND_URL}/api/SignUpStaff/signUpStaff`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        })
        .then(response => {
            if (!response.ok) {
                return response.json().then(err => {
                    throw new Error(err.message);
                });
            }
            return response.json();
        })
        .then(data => {
            // Clear the form fields on success
            SetFirstName('');
            SetLastName('');
            SetEmail('');
            SetPassword('');
            SetKind('Front'); // reset "KindEmployee" to the default if needed
            SetError([]); // Clear errors
        })
        .catch(error => {
            console.error("Error adding employee: ", error.message);
            SetError([error.message]); // Set the error message
        });
    }

    // Function to check if all fields are filled in
    function Check() {
        let data = {
            FirstName,
            LastName,
            Password,
            Email
        };

        let errors = [];
        for (let key in data) {
            if (data[key] === '') {
                errors.push(`${key} is niet ingevuld\n`);
            }
        }

        if (errors.length === 0) {
            SignUp();  // Proceed with the sign-up if no errors
        } else {
            SetError(errors); // Show errors if any fields are empty
        }
    }

    useEffect(() => {
        fetch(`${BACKEND_URL}/api/Cookie/GetUserId` , {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('No Cookie');
                }
                return response.json();
            })
            .catch(() => {
                alert("Cookie was niet geldig");
                navigate('/');
            })
    }, [navigate]);

    return (
        <>
        <GeneralHeader />
        <div className='body'>
            <h1>Registreren Werknemer</h1>
            <br></br>
            <div id='registrate'>
                <label htmlFor='office'>Kantoor</label>
                <br></br>
                <select id='office'name='office' onChange={(e) => SetKind(e.target.value)} value={KindEmployee}>
                    <option value='Front'>Front Office</option>
                    <option value='Back'>Back Office</option>
                </select>
                <br></br>
                <label htmlFor='firstName'>Voornaam</label>
                <br></br>
                <input id='firstName' onChange={(e) => SetFirstName(e.target.value)}value={FirstName}></input>
                <br></br>
                <label htmlFor='lastName'>Achternaam</label>
                <br></br>
                <input id='lastName' onChange={(e) => SetLastName(e.target.value)} value={LastName}></input>
                <br></br>
                <label htmlFor='Password'>Wachtwoord</label>
                <br></br>
                <input id='Password' type='password' onChange={(e) => SetPassword(e.target.value)} value={Password}></input>
                <br></br>
                <label htmlFor='Email'>Email address</label>
                <br></br>
                <input id='Email' type='email' onChange={(e) => SetEmail(e.target.value)} value={Email}></input>
            </div>

            <button onClick={Check}>Registreren</button>

            {ErrorMessage.length > 0 && (
                <div id="errors">
                    <ul>
                        {ErrorMessage.map((errorMessage, index) => (
                            <li key={index}>{errorMessage}</li>
                        ))}
                    </ul>
                </div>
            )}
        </div>

        <GeneralFooter />
        </>
    );
}

export default AddEmployee;
