using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoav
{
    public class CameraScript : MonoBehaviour
    {
        public static CameraScript Singleton;

        [SerializeField] private float _speed = 4f;
        [SerializeField] private float _smoothSpeed = 0.1f;
        private Vector3 _target;

        void Awake()
        {
            Singleton = this;
        }

        void Start()
        {
            _target = transform.position;
        }

        public void Focus(Vector3 pos)
        {
            _target = new Vector3(pos.x, 0, pos.z);
            ;
        }

        void Update()
        {

            if (Input.GetKey(KeyCode.W))
            {
                _target += Time.deltaTime * Vector3.forward * _speed;
            }

            if (Input.GetKey(KeyCode.S))
            {
                _target += Time.deltaTime * Vector3.back * _speed;
            }

            if (Input.GetKey(KeyCode.D))
            {
                _target += Time.deltaTime * Vector3.right * _speed;
            }

            if (Input.GetKey(KeyCode.A))
            {
                _target += Time.deltaTime * Vector3.left * _speed;
            }

            transform.position = Vector3.Lerp(transform.position, _target, _smoothSpeed);
        }
    }

}