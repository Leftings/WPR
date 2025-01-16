export const pushWithBody = async (url, formatBody) => {
    let errorData;
    console.log(formatBody);
    try
    {
        const response = await fetch (url, {
            method: 'POST',
            body: formatBody,
            credentials: 'include',
        });

        if (!response.ok)
        {
            errorData = await response.json();
            throw new Error(errorData.message);
        }
        else
        {
            console.log('succes');
            return { message: 'succes', errorDetected: false}
        }
    }
    catch (error)
    {
        console.error(error.message);
        return { errors: [error.message], errorDetected: true };
    }
}

export const pushWithoutBody = async (url) => {
    let errorData; 

    try
    {
        const response =  await fetch (url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
        });

        if (!response.ok)
        {
            errorData = await response.json();
            throw new Error(errorData);
        }
        else
        {
            return { message: 'succes', errorDetected: false}
        }
    }
    catch (error)
    {
        console.error(errorData);
        return { errors: [error.message], errorDetected: true };
    }
}