import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import GeneralHeader from "../../GeneralBlocks/header/header.jsx";
import GeneralFooter from "../../GeneralBlocks/footer/footer.jsx";
import { pushWithBodyKind, loadList } from '../../utils/backendPusher.js';
import '../../index.css';

const BACKEND_URL = import.meta.env.VITE_REACT_APP_BACKEND_URL ?? 'http://localhost:5165';

function ChangeBusinessSettings() {
    const navigate = useNavigate();
    const [customers, setCustomers] = useState([]); // List of customers
    const [selectedCustomer, setSelectedCustomer] = useState(null); // Customer to update
    const [newEmail, setNewEmail] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [error, setError] = useState('');

    useEffect(() => {
        async function fetchCustomers() {
            try {
                const response = await loadList(`${BACKEND_URL}/api/ChangeBusinessSettings/GetNonPrivateCustomers`);
                if (response.data && Array.isArray(response.data)) {
                    setCustomers(response.data);
                } else {
                    console.error('Unexpected data format:', response);
                    setError('Failed to load customers.');
                }
            } catch (err) {
                console.error('Error fetching customers:', err);
                setError('Failed to load customers.');
            }
        }

        fetchCustomers();
    }, []);

    const updateCredentials = async () => {
        if (!selectedCustomer || !newEmail || !newPassword) {
            setError('All fields are required.');
            return;
        }

        const requestData = {
            UserId: selectedCustomer.id,
            NewEmail: newEmail,
            NewPassword: newPassword,
            BusinessCode: selectedCustomer.businessCode,
        };

        try {
            const response = await pushWithBodyKind(
                `${BACKEND_URL}/api/ChangeUserEmailAndPassword/updateUserCredentials`,
                requestData,
                'POST'
            );

            if (response.success) {
                alert('Credentials updated successfully!');
                setNewEmail('');
                setNewPassword('');
                setSelectedCustomer(null);
            } else {
                setError(response.message || 'Failed to update credentials.');
            }
        } catch (err) {
            console.error('Error updating credentials:', err);
            setError('Failed to update credentials.');
        }
    };

    return (
        <>
            <GeneralHeader />
            <main>
                <div className="Body">
                    <div className="registrateFormatHeader">
                        <h1>Update Customer Credentials</h1>
                    </div>
                    <div className="registrateFormat">
                        <label htmlFor="customerSelect">Select Customer</label>
                        <select
                            id="customerSelect"
                            value={selectedCustomer ? selectedCustomer.id : ''}
                            onChange={(e) =>
                                setSelectedCustomer(customers.find((c) => c.id === Number(e.target.value)))
                            }
                        >
                            <option value="">--Select a Customer--</option>
                            {customers.map((customer) => (
                                <option key={customer.id} value={customer.id}>
                                    {customer.email} (ID: {customer.id})
                                </option>
                            ))}
                        </select>

                        {selectedCustomer && (
                            <>
                                <label htmlFor="inputNewEmail">New Email</label>
                                <input
                                    type="email"
                                    id="inputNewEmail"
                                    value={newEmail}
                                    onChange={(e) => setNewEmail(e.target.value)}
                                />

                                <label htmlFor="inputNewPassword">New Password</label>
                                <input
                                    type="password"
                                    id="inputNewPassword"
                                    value={newPassword}
                                    onChange={(e) => setNewPassword(e.target.value)}
                                />

                                <button
                                    className="cta-button"
                                    type="button"
                                    onClick={updateCredentials}
                                >
                                    Update Credentials
                                </button>
                            </>
                        )}

                        {error && <p style={{ color: 'red' }}>{error}</p>}
                    </div>
                </div>
            </main>
            <GeneralFooter />
        </>
    );
}

export default ChangeBusinessSettings;
