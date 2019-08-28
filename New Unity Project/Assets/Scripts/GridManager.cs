using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoav
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Singleton;

        public int Width;
        public int Height;
        public Tile TilePrefab;
        public Tile[,] Tiles;

        void Awake()
        {
            Tiles = new Tile[Width, Height];

            Singleton = this;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var tile = Instantiate(TilePrefab);
                    tile.transform.parent = transform;
                    tile.transform.position = new Vector3(x, 0, y);
                    tile.Visible(y < 8);
                    Tiles[x, y] = tile;
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }


    }

}