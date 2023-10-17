using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Bobby.Core
{
    public abstract class Pet : MonoBehaviour
    {
        public class StolenObjectTransform
        {
            public Transform OriginalParent;

            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 Scale;

            public StolenObjectTransform(Vector3 p_Position, Vector3 p_Rotation, Vector3 p_Scale, Transform p_OriginalParent)
            {
                Position = p_Position;
                Rotation = p_Rotation;
                Scale = p_Scale;
                OriginalParent = p_OriginalParent;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public static Pet CreatePet<t_PetType>() where t_PetType : Pet
        {
            return new GameObject(nameof(t_PetType)).AddComponent<t_PetType>();
        }

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public abstract string GetName();

        public abstract float GetSpeed();

        public abstract float GetStealSpeed();

        public abstract float GetTurnSpeed();

        public abstract int GetActionLoopDelay();

        public abstract float GetMaxRadius();

        public abstract bool GetCanLoop();

        public abstract Vector3 GetBasePosition();

        public abstract Vector3 GetBaseRotation();

        public abstract Vector3 GetPositionOffset();

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        protected Vector3Animation m_MoveAnimation;
        protected Vector3Animation m_TurnAnimation;

        protected bool m_IsMoving;
        protected bool m_CanActionLoop;

        protected Dictionary<GameObject, StolenObjectTransform> m_StolenObjects = new Dictionary<GameObject, StolenObjectTransform>();

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public bool IsMoving() => m_IsMoving;

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public void Awake()
        {
            m_MoveAnimation = gameObject.AddComponent<Vector3Animation>();
            m_TurnAnimation = gameObject.AddComponent<Vector3Animation>();

            var l_ModelGameObject = new GameObject("Model");
            l_ModelGameObject.transform.SetParent(transform);
            GetModel().Result.transform.SetParent(l_ModelGameObject.transform);

            transform.localPosition = GetBasePosition() + GetPositionOffset();
            transform.localRotation = Quaternion.Euler(GetBaseRotation());
            ActionLoop();
        }

        public abstract Task<GameObject> GetModel();

        public abstract List<Action<Vector3, bool>> GetActionLoopActions();

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        
        private async void ActionLoop()
        {
            await Task.Delay(GetActionLoopDelay());

            if (GetCanLoop() == false)
                goto NextLoop;

            Vector3 l_RandomPosition = new Vector3(UnityEngine.Random.Range(-1 * GetMaxRadius(), GetMaxRadius()), 
                UnityEngine.Random.Range(-1 * GetMaxRadius(), GetMaxRadius()), 
                transform.localPosition.z);

            var l_Actions = GetActionLoopActions();
            int l_ActionsLength = l_Actions.Count();
            int l_Value = UnityEngine.Random.Range(0, l_ActionsLength - 1);
            l_Actions[l_Value](l_RandomPosition, IsMoving());

            NextLoop:
            ActionLoop();
        }

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        
        public Vector3Animation Move(Vector3 p_Position, bool p_IsStealingObject = false)
        {
            LookAtPosition(p_Position).SetFinishedCallback(() =>
            {
                m_MoveAnimation.Init(transform.position, p_Position, 10 / (p_IsStealingObject ? GetStealSpeed() : GetSpeed()));
                m_MoveAnimation.Play();
            });

            return m_MoveAnimation;
        }

        public Vector3Animation Turn(Quaternion p_Value)
        {
            m_TurnAnimation.Init(transform.localRotation.eulerAngles, new Vector3(p_Value.x, p_Value.y, p_Value.z), 10 / GetTurnSpeed());
            m_TurnAnimation.Play();

            return m_TurnAnimation;
        }

        public Vector3Animation LookAtPosition(Vector3 p_Position)
        {
            var l_Quaternion = Quaternion.FromToRotation(Vector3.forward, transform.localPosition - p_Position);
            Turn(l_Quaternion);
            return m_TurnAnimation;
        }

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public Vector3Animation StealObject(GameObject p_Target) {
            Move(p_Target.transform.localPosition, true).SetFinishedCallback(() =>
            {
                m_StolenObjects.Add(p_Target, new StolenObjectTransform(p_Target.transform.position, p_Target.transform.rotation.eulerAngles, p_Target.transform.localScale, p_Target.transform.parent));
                p_Target.transform.SetParent(transform);
            });
            return m_MoveAnimation;
        }

        public void ReleaseStolenObjects()
        {
            foreach (var l_Item in m_StolenObjects)
            {
                l_Item.Key.transform.localPosition = l_Item.Value.Position;
                l_Item.Key.transform.localRotation = Quaternion.Euler(l_Item.Value.Rotation);
                l_Item.Key.transform.localScale = l_Item.Value.Scale;
                l_Item.Key.transform.SetParent(l_Item.Value.OriginalParent);
            }

            m_StolenObjects.Clear();
        }
    }

}
