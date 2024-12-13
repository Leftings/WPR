import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './CarRentalOverview.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function CarRentalOverview() {
    const [rentals, setRentals] = useState([]);
    const [error, setError] = useState(null); // Added error state
    const navigate = useNavigate();

    useEffect(() => {
        const fetchData = async () => {

            try {
                let url = `${BACKEND_URL}/api/Rental/GetAllUserRentals`;

                const response = await fetch(url, {
                    credentials: 'include',
                });
                if (!response.ok) {
                    throw new Error(`Error fetching rentals: ${response.statusText}`);
                }

                const data = await response.json();
                setRentals(data); // Set rentals here
                setError(null); // Reset error on success
                console.log(data);
            } catch (error) {
                console.error('Failed to fetch rentals:', error);
                setError('Failed to load rentals'); // Show error message on failure
            }
        };

        fetchData();
    }, []);

    return (
        <>
            <GeneralHeader />
            <main>
                <section className="hero">
                    <h1>Mijn auto's</h1>
                </section>

                <div className="container">
                    {error && <div className="error-message">{error}</div>} {/* Display error message */}

                    <div className="head">
                        <div>Framenummer</div>
                        <div>Startdatum</div>
                        <div>Eindatum</div>
                        <div>Prijs</div>
                        <div>Status</div>
                    </div>

                    {rentals.length > 0 ? (
                        rentals.map((rental, index) => (
                            <div className="row" key={index}>
                                <div>{rental.frameNrCar}</div>
                                <div>{rental.startDate}</div>
                                <div>{rental.endDate}</div>
                                <div>{rental.price}</div>
                                <div>{rental.status}</div>
                            </div>
                        ))
                    ) : (
                        <div>No rentals found.</div>
                    )}
                </div>
            </main>
            <GeneralFooter />
        </>
    );
}

export default CarRentalOverview;
