export const getErrorMessage = (error, signUpType) => {
    const errorMessages = {
        'Invalid email format' : 'The email format is invalid',
        'Email already exists' : 'The email is already in use. Please use a different email.',
        'Invalid phone number' : 'The phone number is invalid.',
        'Invalid birthday format' : 'The birthday is invalid.',
        'KVK number must be 8 digits' : 'KVK number must be 8 digits',
        'KVK number is not a valid KVK number' : 'KVK number is not a valid KVK number',
    };
    return errorMessages[error.message] || `There was an error during making a ${signUpType} account: ${error.message}`;
};