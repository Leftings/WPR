import React, { useState, useEffect } from 'react';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import '../index.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function WagenparkBeheerderOverzichtPage() {
    const [rentals, setRentals] = useState([]);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const response = await fetch(`${BACKEND_URL}/api/Rental/GetAllUserRentalsWithDetails`, {
                    credentials: 'include',
                });
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                const data = await response.json();
                console.log('Fetched rentals:', data); 
                setRentals(data);
            } catch (err) {
                console.error('Error fetching rentals:', err);
                setError('Failed to load rental details.');
            }
        };
        fetchData();
    }, []);

    return (
        <>
            <GeneralHeader />
            <main>
                <section className="hero">
                    <h1>Gehuurde auto's weergave</h1>
                </section>
                <div className="container">
                    {error && <div className="error-message">{error}</div>}
                    <div className="head">
                        <div>ID</div>
                        <div>Frame Number</div>
                        <div>Start Date</div>
                        <div>End Date</div>
                        <div>Price</div>
                        <div>Customer</div>
                        <div>Status</div>
                        <div>Reviewed By</div>
                        <div>VM Status</div>
                        <div>Kvk</div>
                    </div>
                    {rentals.length > 0 ? (
                        rentals.map((rental, index) => {
                            console.log('Rental:', rental);  
                            return (
                                <div className="row" key={index}>
                                    <div>{rental.id ?? 'N/A'}</div>
                                    <div>{rental.frameNrCar ?? 'N/A'}</div>
                                    <div>{rental.startDate ? new Date(rental.startDate).toLocaleDateString() : 'N/A'}</div>
                                    <div>{rental.endDate ? new Date(rental.endDate).toLocaleDateString() : 'N/A'}</div>
                                    <div>{rental.price ?? 'N/A'}</div>
                                    <div>{rental.customer ?? 'N/A'}</div>
                                    <div>{rental.status ?? 'N/A'}</div>
                                    <div>{rental.reviewedBy ?? 'N/A'}</div>
                                    <div>{rental.vmStatus ?? 'N/A'}</div>
                                    <div>{rental.kvk ?? 'N/A'}</div>
                                </div>
                            );
                        })
                    ) : (
                        <div>Geen Rentals gevonden.</div>
                    )}
                </div>
            </main>
            <GeneralFooter />
        </>
    );
}

export default WagenparkBeheerderOverzichtPage;
