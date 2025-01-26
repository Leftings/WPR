export const getErrorMessage = (error, signUpType) => {
    const errorMessages = {
        'Invalid email format' : 'Verkeerd email format.',
        'Email already exists' : 'Deze email is al in gebruik.',
        'Invalid phone number' : 'Het telefoonnummer is niet correct, moet beginnen met: 06.',
        'Invalid birthday format' : 'Geboortedatum is incorrect',
        'Password must be at least 8 characters.' : 'Wachtwoord moet 8 karakter zijn',
        'Password must contain at least one number.' : 'Wachtwoord moet een nummer bevatten',
        'Password must contain at least one upper case letter.' : 'Wachtwoord moet een hoofdletter bevatten',
        'Password must contain at least one lower case letter.' : 'Wachtwoord moet een kleine leter bevatten',
        'Password must contain at least one symbol' : 'Wachtwoord moet een symbool bevatten'
        
    };
    return errorMessages[error.message] || `There was an error during making a ${signUpType} account: ${error.message}`;
};