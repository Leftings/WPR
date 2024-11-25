import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";

import './GeneralSalePage.css'

function WelcomeUser(setWelcome) {
    fetch('http://localhost:5165/api/Cookie/GetUserName', {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
        credentials: 'include', // Cookies or authentication is included
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

const carsForSale = [
    {
        id: 1,
        name: 'Tesla Model 3',
        price: '$40,000',
        description: 'A sleek and efficient electric car with a long range and modern features.',
        image: 'https://via.placeholder.com/150', 
    },
    {
        id: 2,
        name: 'Ford Mustang',
        price: '$35,000',
        description: 'A powerful and iconic sports car with timeless style.',
        image: 'https://via.placeholder.com/150', 
    },
];

function GeneralSalePage() {
    const [welcomeMessage, setWelcomeMessage] = useState('');

    useEffect(() => {
        WelcomeUser(setWelcomeMessage);
    }, []);

    return (
        <>
            <GeneralHeader />

            <div className="general-sale-page">
                <h1 className="welcome-message">{welcomeMessage}</h1>

                <div className="car-sale-section">
                    <h1 className="title">Cars for Sale</h1>
                    <div className="car-grid">
                        {carsForSale.map((car) => (
                            <div className="car-card" key={car.id}>
                                <div className="car-blob">
                                    <img src={car.image} alt={car.name} className="car-image" />
                                </div>
                                <div className="car-info">
                                    <h2 className="car-name">{car.name}</h2>
                                    <p className="car-price">{car.price}</p>
                                    <p className="car-description">{car.description}</p>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>

            <GeneralFooter />
        </>
    );
}

export default GeneralSalePage;
