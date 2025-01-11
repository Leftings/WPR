export const sorter = (dataList, orderBy, orderHow) =>
{
    for (let data = 1; data < dataList.length; data++)
    {
        let key = dataList[data];
        let j = data - 1;

        while (j >= 0 && (
            orderHow === "Low"
            ? key[orderBy] > dataList[j][orderBy]
            : key[orderBy] <= dataList[j][orderBy])
        )
        {
            dataList[j + 1] = dataList[j];
            j = j - 1;
        }
        dataList[j + 1] = key;
    }

    return dataList;
}

export const specific = (dataList, orderBy, orderHow) =>
{
    console.log(typeof dataList);
    let length = dataList.length;
    for (let data = 0; data < length; data++)
    {
        console.log(dataList[data][orderBy]);
        if (dataList[data][orderBy] === 'X')
        {
           dataList.splice(data, 1);
           data -= 1;
           length -= 1;
        }
    }

    sorter(dataList, orderBy, orderHow);
    return dataList;
}