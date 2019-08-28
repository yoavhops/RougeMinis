using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoav
{
    public class PlayerUnit : MonoBehaviour
    {
        public Tile MyTile;
        public int Sight;

        public void Init(Tile tile)
        {
            MyTile = tile;
            transform.position = tile.transform.position;
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