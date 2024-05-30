using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine.Caching
{
    public class DimensionCache
    {
        public ContainerCache[,] Cache;

        public bool IsEmpty(int x, int y) => Cache[x, y] == null;

        public void AddContainerCache(ContainerCache containerCache, int x, int y, int width, int height)
        {
            for (int xPos = x; xPos < x + width; ++xPos)
            {
                for (int yPos = y; yPos < y + height; ++yPos)
                {
                    Cache[xPos, yPos] = containerCache;
                }
            }
        }

        public ContainerCache CacheAt(Vector2Int position)
        {
            return Cache[position.x, position.y];
        }

        public ContainerCache CacheAt(int x, int y)
        {
            return Cache[x, y];
        }

        public DimensionCache(int width, int height) 
        {
            Cache = new ContainerCache[width, height];
        }

        public DimensionCache(IItemContainer[,] itemsMap)
        {
            Cache = new ContainerCache[itemsMap.GetLength(0), itemsMap.GetLength(1)];
            for(int x = 0;x < Cache.GetLength(0);++x)
            {
                for (int y = 0; y < Cache.GetLength(1); ++y)
                {
                    if (itemsMap[x, y] != null)
                        Cache[x, y] = new ContainerCache(itemsMap[x, y].Item.Thing, itemsMap[x, y].Amount);
                }
            }
        }

        public DimensionCache(IDimensionInventory dimensionInventory)
        {
            Cache = new ContainerCache[dimensionInventory.Width, dimensionInventory.Height];
            for (int x = 0; x < dimensionInventory.Width; ++x)
            {
                for (int y = 0; y < dimensionInventory.Height; ++y)
                {
                    if (!dimensionInventory.IsEmpty(x, y))
                    {
                        var itemAtPos = dimensionInventory.DimensionItemInfoAtPos(x, y);
                        Cache[x, y] = new ContainerCache(itemAtPos.Item.Thing, itemAtPos.Amount);
                    }
                }
            }
        }
    }
}