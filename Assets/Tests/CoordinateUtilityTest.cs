#if !WINDOWS_UWP
using System.Collections;
using System.Collections.Generic;
using HoloLab.Immersal;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class CoordinateUtilityTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void PoseInverseTest()
        {
            var origin = new GameObject("Origin").transform;

            var child = new GameObject("Child").transform;
            child.SetParent(origin);

            var originCopy = new GameObject("OriginCopy").transform;
            originCopy.SetParent(child);

            var testPoseList = new List<(Vector3 position, Quaternion rotation)>
            {
                (Vector3.zero, Quaternion.identity),
                (new Vector3(1, 2, 3), Quaternion.Euler(10, 20, 30)),
                (new Vector3(-3.1f, 0.3f, 30.2f), Quaternion.Euler(-2.2f, 140f, 0.2f))
            };

            foreach (var testPose in testPoseList)
            {
                origin.localPosition = Vector3.zero;
                origin.localRotation = Quaternion.identity;

                child.localPosition = testPose.position;
                child.localRotation = testPose.rotation;

                // OriginCopy の位置を Origin の位置に合わせると、
                // OriginCopy の localPosition/localRotation が Origin->Child の
                // 逆変換となっている
                originCopy.position = origin.position;
                originCopy.rotation = origin.rotation;

                var pose = new Pose(testPose.position, testPose.rotation);

                var inversePose = pose.Inverse();

                Assert.IsTrue(originCopy.localPosition == inversePose.position);
                Assert.IsTrue(Quaternion.Angle(originCopy.localRotation, inversePose.rotation) < 0.001f);
            }
        }

        [Test]
        public void PoseInverse_Inverseを二回実行する_元のPoseと等しくなる()
        {
            var testPoseList = new List<(Vector3 position, Quaternion rotation)>
            {
                (Vector3.zero, Quaternion.identity),
                (new Vector3(1, 2, 3), Quaternion.Euler(10, 20, 30)),
                (new Vector3(-3.1f, 0.3f, 30.2f), Quaternion.Euler(-2.2f, 140f, 0.2f))
            };

            foreach (var testPose in testPoseList)
            {
                var pose = new Pose(testPose.position, testPose.rotation);

                var inversePose = pose.Inverse();
                var inverseOfInversePose = inversePose.Inverse();

                Assert.IsTrue(inverseOfInversePose.position == testPose.position);
                Assert.IsTrue(inverseOfInversePose.rotation == testPose.rotation);
            }
        }

        [Test]
        public void Matrix4x4_ExtractPositionRotation()
        {
            var testPoseList = new List<(Vector3 position, Quaternion rotation)>
            {
                (Vector3.zero, Quaternion.identity),
                (new Vector3(1, 2, 3), Quaternion.Euler(10, 20, 30)),
                (new Vector3(-3.1f, 0.3f, 30.2f), Quaternion.Euler(-2.2f, 140f, 0.2f))
            };

            foreach (var testPose in testPoseList)
            {
                var matrix = Matrix4x4.TRS(testPose.position, testPose.rotation, Vector3.one);

                Assert.IsTrue(matrix.ExtractPosition() == testPose.position);
                Assert.IsTrue(matrix.ExtractRotation() == testPose.rotation);
            }
        }
    }
}
#endif