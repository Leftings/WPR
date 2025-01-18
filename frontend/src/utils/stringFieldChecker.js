export const NoSpecialCharacters = (input) => {
    if (/[A-Za-z]/.test(input[input.length - 1])) {
        return input;
    }
    return input.slice(0, -1);
};

export const SyntaxLicensePlate = (licensePlate, oldPlate) => {
    if (licensePlate > oldPlate && /[A-Za-z0-9]/.test(licensePlate[licensePlate.length - 1])) {
        if (licensePlate.length !== 9) {
            if ((licensePlate.length === 2 || licensePlate.length === 6)) {
                licensePlate += '-';
            }
        }

        if (licensePlate.length > 9) {
            let temp = '';

            for (let i = 0; i < 9; i++) {
                temp += licensePlate[i];
            }

            return temp.toUpperCase();
        }
        return licensePlate.toUpperCase();
    }
    return licensePlate.slice(0, -1).toUpperCase();
};
