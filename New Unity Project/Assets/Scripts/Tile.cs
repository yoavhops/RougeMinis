using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoav
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private TileVisiblePart _visibleIndicator;


        // Update is called once per frame
        void Update()
        {

        }

        public void Visible(bool see)
        {
            _visibleIndicator.gameObject.SetActive(see);
        }

    }
}