export const placingItems = (grid, cardWidth, spaceBetween) => {
    if (grid === null) {
      return grid;
    }
    if(grid.current == null)
    {
      return grid;
    }

    const gridWith = grid.current.offsetWidth;
    const columns = Math.floor((gridWith - cardWidth) / cardWidth);
    grid.current.style.gridTemplateColumns = `repeat(${columns}, ${cardWidth}px)`;
    console.log(grid.current.style.gridTemplateColumns);
    return grid;
}
  