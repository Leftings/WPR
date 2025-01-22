import React, { useState, useEffect } from 'react';
import GeneralHeader from "../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../GeneralBlocks/footer/footer.jsx";
import '../index.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function AbonementUitlegPage() {
    const [subscriptions, setSubscriptions] = useState([]);
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);
    const [loadingRequests, setLoadingRequests] = useState({});

    // Fetch single subscription details
    async function GetSubscription(id) {
        try {
            const response = await fetch(`${BACKEND_URL}/api/Subscription/GetSubscriptionData?id=${id}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
            });

            if (!response.ok) {
                const data = await response.json();
                throw new Error(data?.message || 'Failed to fetch subscription data');
            }

            const data = await response.json();
            return data.message;
        } catch (error) {
            console.error(error);
            return null;
        }
    }

    useEffect(() => {
        const fetchSubscriptions = async () => {
            try {
                setLoading(true);
                setError(null);

                // Fetch subscription IDs
                const response = await fetch(`${BACKEND_URL}/api/Subscription/GetSubscriptionIds`, {
                    method: 'GET',
                    credentials: 'include',
                });

                if (!response.ok) {
                    throw new Error('Failed to fetch subscription IDs');
                }

                const data = await response.json();
                console.log(data);
                const subscriptionIds = data?.message || [];
                console.log(subscriptionIds);

                const subscriptionsData = await Promise.all(
                    subscriptionIds.map(async (id) => {
                        setLoadingRequests((prevState) => ({ ...prevState, [id]: true }));
                        const subscription = await GetSubscription(id);
                        setLoadingRequests((prevState) => ({ ...prevState, [id]: false }));
                        return subscription;
                    })
                );

                setSubscriptions(subscriptionsData.filter(Boolean));
            } catch (error) {
                setError(error.message);
            } finally {
                setLoading(false);
            }
        };

        fetchSubscriptions();
    }, []);

    useEffect(() => {
        console.log("Subscriptions:", subscriptions);
    }, [subscriptions]);

    return (
        <>
            <GeneralHeader isLoggedIn={isLoggedIn} />
            <main>
                {loading ? (
                    <div className="loading-spinner"></div>
                ) : error ? (
                    <div className="error-message">{error}</div>
                ) : (
                    <div className="registrateFormat">
                        {subscriptions.length > 0 ? (
                            subscriptions.map((subscription) => (
                                <div key={subscription.ID} className="subscription-card">
                                    <div className="subscription-info">
                                        <h2 className="subscription-type">{subscription.type || 'Unknown'}</h2>
                                        <p className="subscription-description">{subscription.description || 'Unknown'}</p>
                                    </div>
                                </div>
                            ))

                        ) : (
                            <p>No subscriptions available.</p>
                        )}
                    </div>
                )}
            </main>
            <GeneralFooter />
        </>
    );
}

export default AbonementUitlegPage;
