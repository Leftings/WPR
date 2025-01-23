import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ToastContainer, toast } from 'react-toastify';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import '../index.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

/**
 * CarRentalOverview is een component die alle gehuurde voertuigen toont voor de gebruiker.
 * Het ondersteunt het annuleren van een huur en het wijzigen van huurgegevens.
 * 
 * @component
 * @example
 * <CarRentalOverview />
 */
function CarRentalOverview() {
    const [rentals, setRentals] = useState([]);
    const [error, setError] = useState(null);
    const [chosenRental, setChosenRental] = useState(null);
    const currDate = new Date();
    const navigate = useNavigate();
    var modal = document.getElementById("myModal");

    /**
     * @brief Cancelen van huur
     * Functie voor het openen van de annuleermodus voor een geselecteerd voertuig.
     * 
     * @param {number} index - De index van het geselecteerde voertuig in de lijst.
     */
    const cancellation = (index) => {
        modal.style.display = "block";
        let rental = rentals[index];

        setChosenRental(rental);
    };
    
    const settingsError = (type) => {
        if (type === "cancel") {
            toast.error("Huurcontract is al van toepassing, annuleren is niet beschikbaar.");
        }
        if (type === "change") {
            toast.error("Huurcontract is al van toepassing, wijzigingen zijn niet beschikbaar.");
        }
    }
    
    /**
     * Functie voor het sluiten van de annuleermodus.
     */
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
            const response = await fetch(`${BACKEND_URL}/api/Rental/CancelRental?rentalId=${chosenRental.id}&frameNr=${chosenRental.frameNrCar}`, {
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

    /**
     * Functie voor het navigeren naar de pagina voor het wijzigen van de huurgegevens.
     * 
     * @param {Object} rental - Het huurvoertuig waarvan de gegevens gewijzigd moeten worden.
     */
    const handleWijziging = (rental) => {
        console.log('Navigating with rental:', rental);  // Add this log to check rental data
        navigate("/changeRental", { state: { rental } });
    }

    useEffect(() => {
        /**
         * Functie voor het ophalen van huurgegevens van de server.
         * De gegevens worden getoond in de lijst zodra ze zijn opgehaald.
         */
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

    useEffect(() => {
        fetch(`${BACKEND_URL}/api/Cookie/GetUserId`, {
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
            .then(data => {
                const id = data?.message;
            })
            .catch(() => {
                alert("Cookie was niet geldig");
                navigate('/');
            });
    }, [navigate]);

    /**
     * Functie voor het annuleren van de huur van een voertuig.
     * 
     * Stuur een verzoek naar de server om de huur te annuleren en vernieuw de pagina bij succes.
     */
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
                                <p>{`Naam - ${chosenRental.carName}`}</p>
                                <p>{`Kenteken - ${chosenRental.licensePlate}`}</p>
                            </>
                        ) : (
                            <p>Geen keuze gemaakt</p>
                        )}
                        <button className="cta-button" onClick={handleCancellation}>Annuleer</button>
                    </div>
                </div>

                <div className="container">
                    {error && <div className="error-message">{error}</div>} {/* Display error message */}

                    <div className="heads">
                        <div>Voertuig</div>
                        <div>Kenteken</div>
                        <div>Startdatum</div>
                        <div>Eindatum</div>
                        <div>Prijs</div>
                        <div>Status</div>
                        <div>Instellingen</div>
                    </div>

                    {rentals.length > 0 ? (
                        rentals.map((rental, index) => (
                            <div className="rows" key={index}>
                                <div>{rental.carName}</div>
                                <div>{rental.licensePlate}</div>
                                <div>{rental.startDate}</div>
                                <div>{rental.endDate}</div>
                                <div>{`€${rental.price}`}</div>
                                <div>{rental.status}</div>
                                {currDate < new Date(rental.startDate) ? (
                                    <div className="rental-config">
                                        <button className="cta-button" onClick={() => cancellation(index)}>Annuleer</button>
                                        <button className="cta-button" onClick={() => handleWijziging(rental)}>Wijzig</button>
                                    </div>
                                ) : (
                                    <div className="rental-config">
                                        <button className="cta-button-unavailable" onClick={() => settingsError("cancel")}>Annuleer</button>
                                        <button className="cta-button-unavailable" onClick={() => settingsError("change")}>Wijzig</button>
                                    </div>
                                )}
                            </div>
                        ))
                    ) : (
                        <div>Geen huur informatie gevonden</div>
                    )}
                </div>
            </main>
            <GeneralFooter/>
        </>
    );
}

export default CarRentalOverview;
