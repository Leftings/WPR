import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './GeneralSalePage.css';

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
        price: '$700.00/dag',
        description: 'A sleek and efficient electric car with a long range and modern features.',
        image: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTf_UHxDfJtjBv1c8MsINc_kFNg_suyA3YVAw&s',
    },
    {
        id: 2,
        name: 'Ford Mustang',
        price: '$450.00/dag',
        description: 'A powerful and iconic sports car with timeless style.',
        image: 'https://www.boredpanda.com/blog/wp-content/uploads/2022/09/messed-up-looking-cars-632ad6bc4cf8d__700.jpg',
    },
    {
        id: 3,
        name: 'Chevrolet Camaro',
        price: '$500.00/dag',
        description: 'A muscular and aggressive sports car with powerful performance.',
        image: 'https://i1.sndcdn.com/artworks-Tx4TtEalhzFE9Wna-HMhChw-t500x500.jpg',
    },
    {
        id: 4,
        name: 'BMW M3',
        price: '$650.00/dag',
        description: 'A high-performance sedan with sporty handling and a luxurious interior.',
        image: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSmdTUcNFPahcDyNT5h9H0al-u9caq-qY3rDQ&s',
    },
    {
        id: 5,
        name: 'Audi R8',
        price: '$1200.00/dag',
        description: 'A luxurious supercar with unmatched performance and striking design.',
        image: 'https://upload.wikimedia.org/wikipedia/commons/a/a7/2020_Audi_R8_V10.jpg',
    },
    {
        id: 6,
        name: 'Mercedes-Benz G-Class',
        price: '$750.00/dag',
        description: 'An iconic luxury SUV with off-road capabilities and premium features.',
        image: 'https://upload.wikimedia.org/wikipedia/commons/a/a4/Mercedes-Benz_G-Class_G_400_d_4MATIC.jpg',
    },
    {
        id: 7,
        name: 'Porsche 911',
        price: '$1000.00/dag',
        description: 'A legendary sports car with exceptional performance and timeless design.',
        image: 'https://upload.wikimedia.org/wikipedia/commons/7/73/2019_Porsche_911_Carrera_S.jpg',
    },
    {
        id: 8,
        name: 'Lamborghini Huracán',
        price: '$1500.00/dag',
        description: 'An Italian supercar known for its stunning looks and extreme performance.',
        image: 'https://upload.wikimedia.org/wikipedia/commons/9/92/2015_Lamborghini_Huracán_LP_610-4.jpg',
    },
    {
        id: 9,
        name: 'Jaguar F-Type',
        price: '$900.00/dag',
        description: 'A sleek British sports car with impressive speed and a refined interior.',
        image: 'https://upload.wikimedia.org/wikipedia/commons/5/58/Jaguar_F-Type_Coupe_3.0_Supercharged_V6.jpg',
    },
    {
        id: 10,
        name: 'Ferrari 488 GTB',
        price: '$2000.00/dag',
        description: 'An elite supercar with breathtaking performance and exquisite craftsmanship.',
        image: 'https://upload.wikimedia.org/wikipedia/commons/f/fb/2016_Ferrari_488_GTB_3.9_Front.jpg',
    },
    {
        id: 11,
        name: 'Toyota Supra',
        price: '$600.00/dag',
        description: 'A reborn classic sports car with a perfect blend of performance and reliability.',
        image: 'https://upload.wikimedia.org/wikipedia/commons/7/70/2020_Toyota_Supra_3.0.jpg',
    },
    {
        id: 12,
        name: 'Nissan GT-R',
        price: '$750.00/dag',
        description: 'A high-performance sports car known for its speed, agility, and engineering excellence.',
        image: 'https://upload.wikimedia.org/wikipedia/commons/0/07/2017_Nissan_GT-R_%28R35%29_3.8_Front.jpg',
    },
    {
        id: 13,
        name: 'Honda Civic Type R',
        price: '$350.00/dag',
        description: 'A compact performance car with sporty handling and aggressive design.',
        image: 'https://upload.wikimedia.org/wikipedia/commons/f/f3/2017_Honda_Civic_Type_R.jpg',
    },
    {
        id: 14,
        name: 'Subaru WRX STI',
        price: '$400.00/dag',
        description: 'A rally-inspired performance car with all-wheel drive and excellent handling.',
        image: 'https://i.pinimg.com/736x/f7/97/42/f797426f043d2cd954891f7d17686887.jpg',
    },
    {
        id: 15,
        name: 'Mazda MX-5 Miata',
        price: '$250.00/dag',
        description: 'A lightweight sports car with exceptional handling and fun-to-drive dynamics.',
        image: 'https://upload.wikimedia.org/wikipedia/commons/0/01/2016_Mazda_MX-5_Miata.jpg',
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
                    <h1 className="title">Auto's te huur</h1>
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
                                    <Link
                                        to={`/car/${car.id}`}
                                        state={{ car }}
                                        className="view-details-link"
                                    >
                                        View Details
                                    </Link>
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