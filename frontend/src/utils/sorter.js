export const sorter = (dataList, orderBy, orderHow) =>
{
    for (let data = 1; data < dataList.length; data++)
    {
        console.log(dataList);
        console.log(dataList[data][orderBy]);
        let key = dataList[data];
        let j = data - 1;

        while (j >= 0 && (
            orderHow.toString().toLowerCase() === "low"
            ? key[orderBy] <= dataList[j][orderBy]
            : key[orderBy] > dataList[j][orderBy])
        )
        {
            dataList[j + 1] = dataList[j];
            j = j - 1;
        }
        dataList[j + 1] = key;
    }

    return dataList;
}

export function sorterArray(dataList, orderBy) {
    const orderMap = {
        Car: 1,
        Camper: 2,
        Caravan: 3,
    };

    return dataList.sort((a, b) => {
        const aOrder = orderMap[a[orderBy]] ?? Number.MAX_VALUE;
        const bOrder = orderMap[b[orderBy]] ?? Number.MAX_VALUE;

        return aOrder - bOrder;
    });
}
    

export const specific = (dataList, orderBy, orderHow, prefix) =>
{
    console.log(typeof dataList);
    let length = dataList.length;
    for (let data = 0; data < length; data++)
    {
        console.log(dataList[data][orderBy]);
        if (dataList[data][orderBy] === prefix)
        {
           dataList.splice(data, 1);
           data -= 1;
           length -= 1;
        }
    }

    sorter(dataList, orderBy, orderHow);
    return dataList;
}