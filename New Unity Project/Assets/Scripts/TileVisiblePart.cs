using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoav
{
    public class TileVisiblePart : MonoBehaviour
    {
        [SerializeField] private Renderer rend;

        private Color _origColor;

        [SerializeField] private Color _highLightColor;


        void Start()
        {
            _origColor = rend.material.color;
        }

        void Update()
        {

        }

        void OnMouseEnter()
        {
            rend.material.color = _highLightColor;
        }

        void OnMouseExit()
        {
            rend.material.color = _origColor;
        }
    }
}