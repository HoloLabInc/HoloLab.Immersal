// Copyright (c) 2021 HoloLab Inc. All rights reserved.

using UnityEngine;

namespace HoloLab.Immersal
{
    public static class CoordinateUtility
    {
        public static Pose Inverse(this Pose pose)
        {
            var m = Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);
            var inverse = m.inverse;

            var rotation = inverse.ExtractRotation();
            var position = inverse.ExtractPosition();
            return new Pose(position, rotation);
        }

        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            return matrix.GetColumn(3);
        }

        public static Quaternion ExtractRotation(this Matrix4x4 matrix)
        {
            return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        }
    }
}