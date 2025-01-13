export const placingItems = (grid, cardWidth) =>
{
    if (!grid)
    {
        return;
    }
    
    const containerWidth = grid.offsetWidth;
    const columns = Math.floor(containerWidth / cardWidth) + 350;
    console.log(containerWidth, cardWidth, columns);
    grid.style.gridTemplateColumns = `repeat(${columns > 0 ? columns : 1}, 1fr)`;
    return grid;
}