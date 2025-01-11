export const loadList = async (backendUrl) =>
{
  try
  {
    const response = await fetch(backendUrl, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })

    if (!response.ok)
    {
      const errorData = await response.json();
      throw new Error(errorData);
    }

    const data = await response.json()

    if (Array.isArray(data?.message))
    {
      const combinedData = data.message.reduce((acc, item) => 
      {
        return { ...acc, ...item };
      }, {})
      return { message: combinedData };
    }

    else if (data?.message && typeof data.message === 'object')
    {
      const combinedData = Object.assign({}, data.message);
      return { message: data.message };
    }
    
    else
    {
      console.error("Expected 'message' to be an object but got:", data?.message);
      return { message: data?.message };
    }

  } catch (error) {
    console.error(error);
    return null;
  }
}

export const loadSingle = async (backendUrl) => 
{
  try
  {
    const response = await fetch(backendUrl, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })
  
    if (!response.ok)
    {
      const errorData = await response.json();
      throw new Error(errorData?.message);
    }
  
    return response;
  }
  catch (err) {
    console.error(error);
    return null;
  }
}