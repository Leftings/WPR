export const EmptyFieldChecker = (inputFields) =>
{
    const errors = [];
    for (let key in inputFields)
    {
        if (inputFields[key] === '' || inputFields[key] === null)
        {
            errors.push(`${key} is niet ingevuld`)
        }
    }

    return errors;
}