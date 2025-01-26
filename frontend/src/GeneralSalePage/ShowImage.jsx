import React, { useState, useEffect } from "react";

//const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';
const BACKEND_URL = '95.99.30.110:5000';
function ShowImage({ frameNr }) {
    const [imageSrc, setImageSrc] = useState('');
    const [error, setError] = useState(null);

    // Functie om de afbeelding op te halen van de backend
    const fetchVehicleImage = async () => {
        try {
            // Haal de afbeelding op via een GET-request
            const response = await fetch(`${BACKEND_URL}/api/vehicle/GetVehicleImageAsync?frameNr=${frameNr}`);

            // Controleer of het verzoek succesvol was
            if (!response.ok) {
                throw new Error(`Error fetching image: ${response.statusText}`);
            }

            // Converteer de respons naar een Base64-string
            const base64Image = await response.text();
            setImageSrc(base64Image);
        } catch (e) {
            // Log de fout in de console en sla een foutmelding op in de state
            console.error(e);
            setError('Failed to load image');
        }
    };

    // Gebruik een effect om de afbeelding op te halen wanneer `frameNr` verandert
    useEffect(() => {
        fetchVehicleImage();
    }, [frameNr]); 

    return (
        <div>
            {error && <p>{error}</p>}

            {imageSrc && !error && (
                <img src={`data:image/jpeg;base64,${imageSrc}`} alt="vehicles"/>
            )}
        </div>
    )
}

export default ShowImage