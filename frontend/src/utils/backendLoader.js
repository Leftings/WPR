export const loadArray = async (backendUrl) =>
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
      return { message: combinedData, data: combinedData };
    } 
    if (Array.isArray(data?.data))
      {
        const combinedData = data.data.reduce((acc, item) => 
        {
          return { ...acc, ...item };
        }, {})
        return { message: combinedData, data: combinedData };
      } 
    else
    {
      console.error("Expected 'message' to be an Array but got:", data?.message);
      return { message: data?.message };
    }

  } catch (error) {
    console.error(error);
    return null;
  }
}

export const loadList = async (backendUrl) => 
{
  console.log(backendUrl);
  try
  {
    const response = await fetch(backendUrl, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    })

    console.log("Response Status:", response.status);
    console.log("Response Headers:", response.headers);

    if (!response.ok)
    {
      const errorData = await response.json();
      throw new Error(errorData);
    }

    const data = await response.json();
    console.log('Parsed Data:', data);
    console.log(typeof data);
    if (data?.data && typeof data.data === 'object')
    {
      return { message: data.message, data: data.data };
    }
    else if (data?.message && typeof data.message === 'object')
    {
      return { message: data.message, data: data.message };
    }
    else
    {
      console.error("Expected 'message' to be an List / Dictionairy but got:", data?.message);
      return { message: data?.message};
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
    console.error(err);
    return null;
  }
}