using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoav
{
    public class Utils : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        public static float DistanceRangeNoY(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a.NoY(), b.NoY());
        }

        public static bool InRangeNoY(Vector3 a, Vector3 b, float range)
        {
            return DistanceRangeNoY(a, b) < range;
        }

    }

    public static class Vector3Extensions
    {
        public static Vector3
            OneValue(this Vector3 vector, float value) // changes the input vector to have a single value for all axis.
        {
            return new Vector3(value, value, value);
        }

        public static Vector3
            NoYChange(this Vector3 vector, float? x,
                float? z) //changes the vector only on the z, x axis, y remains the same.
        {
            return new Vector3(x ?? 0f, vector.y, z ?? 0f);
        }

        public static Vector3 NoY(this Vector3 vector) //changes the vector only on the z, x axis, y remains the same.
        {
            return new Vector3(vector.x, 0f, vector.z);
        }

        public static bool IsBehind(this Transform trans, Transform other, float angle)
        {
            return Vector3.Angle(trans.forward.NoY(), other.position.NoY() - trans.position.NoY()) > angle;
        }
    }
}