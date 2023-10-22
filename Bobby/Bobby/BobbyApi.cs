using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Bobby
{
    public class BobbyApi
    {

        public static void Move(Vector3 p_Position)
        {
            Core.Bobby.Instance.Move(p_Position);
        }

        public static void Steal(GameObject p_Object)
        {
            Core.Bobby.Instance.StealObject(p_Object);
        }

        public static void Turn(Quaternion p_Rot)
        {
            Core.Bobby.Instance.Turn(p_Rot);
        }

        public static void LookAtLocation(Vector3 p_Position)
        {
            Core.Bobby.Instance.LookAtPosition(p_Position);
        }

        public static void ReleaseStolenObjects()
        {
            Core.Bobby.Instance.ReleaseStolenObjects();
        }

        public static void StopBobbyMovements()
        {
            Core.Bobby.Instance.StopMovements();
        }

        public static void SetColor(Color p_Color)
        {
            Core.Bobby.Instance.SetColor(p_Color);
        }

        public static void ResetPosition()
        {
            Core.Bobby.Instance.Move(Core.Bobby.Instance.GetBasePosition())
                .SetFinishedCallback(() =>
                {
                    Core.Bobby.Instance.Turn(Quaternion.Euler(Core.Bobby.Instance.GetBaseRotation()));
                });
        }

    }
}
