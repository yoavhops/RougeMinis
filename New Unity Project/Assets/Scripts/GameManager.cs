using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoav
{
    public class GameManager : MonoBehaviour
    {
        public PlayerUnit PlayerUnitPrefab;

        public List<PlayerUnit> PlayerUnits;

        private int _turn = 0;

        // Start is called before the first frame update
        void Start()
        {
            var playerUnit = Instantiate(PlayerUnitPrefab);
            playerUnit.Init(GridManager.Singleton.Tiles[5, 5]);
            PlayerUnits.Add(playerUnit);

            playerUnit = Instantiate(PlayerUnitPrefab);
            playerUnit.Init(GridManager.Singleton.Tiles[15, 5]);
            PlayerUnits.Add(playerUnit);

            ShowSight();

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NextTurn();
            }
        }

        void NextTurn()
        {
            var player = PlayerUnits[_turn];
            CameraScript.Singleton.Focus(player.transform.position);
            _turn++;
            _turn = _turn % PlayerUnits.Count;
        }

        void ShowSight()
        {
            var width = GridManager.Singleton.Width;
            var height = GridManager.Singleton.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    GridManager.Singleton.Tiles[x, y].Visible(false);
                }
            }

            foreach (var playerUnit in PlayerUnits)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var visible = Utils.InRangeNoY(
                            playerUnit.transform.position,
                            GridManager.Singleton.Tiles[x, y].transform.position,
                            playerUnit.Sight);

                        if (visible)
                        {
                            GridManager.Singleton.Tiles[x, y].Visible(true);
                        }
                    }
                }
            }

        }
    }
}