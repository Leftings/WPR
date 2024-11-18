import axios from 'axios';

export const handleLogin = async (email, password, onLoginSuccess, onLoginFailure) => {
    const loginData = { email, password };

    try {
        const response = await axios.post('http://localhost:5165/api/Login/login', loginData, {
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (response.status === 200) {
            onLoginSuccess(response.data.message);
        } else {
            onLoginFailure(response.data.message);
        }
    } catch (error) {
        onLoginFailure('An error occurred. Please try again.');
    }
};
