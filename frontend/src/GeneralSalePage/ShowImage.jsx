import React, { useState, useEffect } from "react";

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function ShowImage({ frameNr }) {
    const [imageSrc, setImageSrc] = useState('');
    const [error, setError] = useState(null);

    const fetchVehicleImage = async () => {
        try {
            const response = await fetch(`${BACKEND_URL}/api/vehicle/GetVehicleImageAsync?frameNr=${frameNr}`);

            if(!response.ok) {
                throw new Error(`Error fetching image: ${response.statusText}`);
            }

            const base64Image = await response.text();
            setImageSrc(base64Image);
        } catch (e) {
            console.error(e);
            setError('Failed to load image');
        }
    };

    useEffect(() => {
        fetchVehicleImage()
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