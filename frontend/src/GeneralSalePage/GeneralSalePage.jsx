import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import ShowImage from './ShowImage.jsx';

import './GeneralSalePage.css'

function WelcomeUser(setWelcome) {
    fetch('http://localhost:5165/api/Cookie/GetUserName', {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
        credentials: 'include',
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
    {
        id: 3,
        name: 'Ford Mustang',
        price: '$35,000',
        description: 'A powerful and iconic sports car with timeless style.',
        image: 'https://via.placeholder.com/150',
    },
    {
        id: 4,
        name: 'Ford Mustang',
        price: '$35,000',
        description: 'A powerful and iconic sports car with timeless style.',
        image: 'https://via.placeholder.com/150',
    },
    {
        id: 5,
        name: 'Ford Mustang',
        price: '$35,000',
        description: 'A powerful and iconic sports car with timeless style.',
        image: 'https://via.placeholder.com/150',
    },
    {
        id: 6,
        name: 'Ford Mustang',
        price: '$35,000',
        description: 'A powerful and iconic sports car with timeless style.',
        image: 'https://via.placeholder.com/150',
    },
    {
        id: 7,
        name: 'Ford Mustang',
        price: '$35,000',
        description: 'A powerful and iconic sports car with timeless style.',
        image: 'https://via.placeholder.com/150',
    },
    {
        id: 8,
        name: 'Ford Mustang',
        price: '$35,000',
        description: 'A powerful and iconic sports car with timeless style.',
        image: 'https://via.placeholder.com/150',
    },
    {
        id: 9,
        name: 'Ford Mustang',
        price: '$35,000',
        description: 'A powerful and iconic sports car with timeless style.',
        image: 'https://via.placeholder.com/150',
    },
    {
        id: 10,
        name: 'Ford Mustang',
        price: '$35,000',
        description: 'A powerful and iconic sports car with timeless style.',
        image: 'https://via.placeholder.com/150',
    },
];

function GeneralSalePage() {
    const [welcomeMessage, setWelcomeMessage] = useState('');
    const [vehicles, setVehicles] = useState([]);
    const [error, setError] = useState(null);

    const fetchVehicles= async () => {
        try {
            const response = await fetch(`http://localhost:5165/api/vehicle/GetAllVehicles`);

            if (!response.ok) {
                throw new Error(`Error fetching vehicles: ${response.statusText}`)
            }

            const data = await response.json();
            console.log(data);
            setVehicles(data);
            console.log(vehicles);
        } catch (e) {
            console.error(e);
            setError('Failed to load vehicles')
        }
    };

    useEffect(() => {
        fetchVehicles();
    }, []);

    useEffect(() => {
        console.log(vehicles);


    })

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
                        {vehicles.map((vehicle) => (
                            <div key={vehicle.FrameNr} className="car-card">
                                <div className="car-blob">
                                    {vehicle.Image ? (
                                        <img
                                            src={`data:image/jpeg;base64,${vehicle.Image}`}
                                            alt={`${vehicle.Brand || 'Unknown'} ${vehicle.Type || ''}`}
                                        />
                                    ) : (
                                        <p>Image not available</p>
                                    )}
                                </div>
                                <div className="car-info">
                                    <h2 className="car-name">{`${vehicle.Brand || 'Unknown'} ${vehicle.Type || ''}`}</h2>
                                    <p className="car-price">{`$${vehicle.Price}`}</p>
                                    <p className="car-description">Vroom Vroom</p>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>

            <GeneralFooter/>
        </>
    );
}

export default GeneralSalePage;
