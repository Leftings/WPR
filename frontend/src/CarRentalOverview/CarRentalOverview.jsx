import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import './CarRentalOverview.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function CarRentalOverview() {
    const [rentals, setRentals] = useState([]);
    const [error, setError] = useState(null);
    const [chosenRental, setChosenRental] = useState(null);
    const navigate = useNavigate();
    var modal = document.getElementById("myModal");

    const cancellation = (index) => {
        modal.style.display = "block";
        let rental = {
            Id: rentals[index].id,
            Name: rentals[index].carName,
            LicensePlate: rentals[index].licensePlate,
            FrameNr: rentals[index].frameNrCar
        };

        setChosenRental(rental);
    };
    
    const closeCancellation = () => {
        modal.style.display = "none";
    };

    window.onclick = function(event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    };

    const handleCancellation = async () => {

        try {
            const response = await fetch(`${BACKEND_URL}/api/Rental/CancelRental?rentalId=${chosenRental.Id}&frameNr=${chosenRental.FrameNr}`, {
                method: 'DELETE',
                credentials: 'include',
            });

            const data = await response.json();
            if (response.ok) {
                window.location.reload();
            } else {
                setError(data.message);
            }
        } catch (error) {
            setError('Rental cancellation failed');
        }
    };

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
                setRentals(data);
                setError(null);
                console.log(data);
            } catch (error) {
                console.error('Failed to fetch rentals:', error);
                setError('Failed to load rentals');
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

                <div id="myModal" className="modal">
                    <div className="modal-content">
                        <span className="close" onClick={closeCancellation}>&times;</span>
                        <p>Contract van volgende voertuig wordt geannuleerd:</p>
                        {chosenRental ? (
                            <>
                                <p>{`Naam - ${chosenRental.Name}`}</p>
                                <p>{`Kenteken - ${chosenRental.LicensePlate}`}</p>
                            </>
                        ) : (
                            <p>Geen keuze gemaakt</p>
                        )}
                        <button id="button" onClick={handleCancellation}>Annuleer</button>
                    </div>
                </div>

                <div className="container">
                    {error && <div className="error-message">{error}</div>} {/* Display error message */}

                    <div className="head">
                        <div>Voertuig</div>
                        <div>Kenteken</div>
                        <div>Startdatum</div>
                        <div>Eindatum</div>
                        <div>Prijs</div>
                        <div>Status</div>
                        <div>Settings</div>
                    </div>

                    {rentals.length > 0 ? (
                        rentals.map((rental, index) => (
                            <div className="row" key={index}>
                                <div>{rental.carName}</div>
                                <div>{rental.licensePlate}</div>
                                <div>{rental.startDate}</div>
                                <div>{rental.endDate}</div>
                                <div>{`€${rental.price}`}</div>
                                <div>{rental.status}</div>
                                <div className="rental-config">
                                    <button id="button" onClick={() => cancellation(index)}>Annuleer</button>
                                    <button id="button">Wijzig</button>
                                </div>
                            </div>
                        ))
                    ) : (
                        <div>No rentals found.</div>
                    )}
                </div>
            </main>
            <GeneralFooter/>
        </>
    );
}

export default CarRentalOverview;
