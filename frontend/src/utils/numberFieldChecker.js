export const NumberCheck = (input) =>
{
    if (/[0-9]/.test(input[input.length -1 ]))
    {
        return input;
    }
    return input.slice(0, -1);
}

export const KvKChecker = (input) =>
{
    if (input.length > 8)
    {
        return input.slice(0, -1);
    }
    return input;
}