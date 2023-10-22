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

        public const float JUMP_HEIGHT = 1;

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

        public abstract Vector3 GetRotationOffset();

        protected virtual void Init()
        {

        }

        protected virtual void OnSteal(GameObject p_StolenObject)
        {

        }

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
            m_MoveAnimation.OnVectorChange += OnMove;
            m_MoveAnimation.OnFinished += (p_Pos) =>
            {
                m_IsMoving = false;
            };
            m_TurnAnimation = gameObject.AddComponent<Vector3Animation>();
            m_TurnAnimation.OnVectorChange += OnTurn;

            var l_ModelGameObject = new GameObject("Model");
            l_ModelGameObject.transform.SetParent(transform);
            if (m_ModelObject == null)
                m_ModelObject = GetModel();

            m_ModelObject.transform.SetParent(l_ModelGameObject.transform);

            transform.localPosition = GetBasePosition() + GetPositionOffset();
            transform.localRotation = Quaternion.Euler(GetBaseRotation() + GetRotationOffset());
            Init();
            ActionLoop();
        }

        protected GameObject m_ModelObject;

        public abstract GameObject GetModel();

        public abstract List<Action<Vector3, bool>> GetActionLoopActions();

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        
        private async void ActionLoop()
        {
            await Task.Delay(GetActionLoopDelay());

            if (GetCanLoop() == false)
                goto NextLoop;

            //Plugin.Log.Info("Doing loop");

            Vector3 l_RandomPosition = new Vector3(UnityEngine.Random.Range(-1 * GetMaxRadius(), GetMaxRadius()), 
                UnityEngine.Random.Range(-1 * GetMaxRadius(), GetMaxRadius()), 
                UnityEngine.Random.Range(-1 * GetMaxRadius(), GetMaxRadius()));

            var l_Actions = GetActionLoopActions();
            int l_ActionsLength = l_Actions.Count();

            if (l_ActionsLength == 0)
                goto NextLoop;

            int l_Value = UnityEngine.Random.Range(0, l_ActionsLength);
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
                Vector3 l_NewPos = new Vector3(p_Position.x, GetPositionOffset().y, p_Position.z);
                m_MoveAnimation.Init(transform.localPosition, l_NewPos, 5 / (p_IsStealingObject ? GetStealSpeed() : GetSpeed()));
                m_MoveAnimation.Play();
                m_IsMoving = true;
            });

            return m_MoveAnimation;
        }

        public Vector3Animation Turn(Quaternion p_Value)
        {
            m_TurnAnimation.Init(transform.localRotation.eulerAngles, p_Value.eulerAngles + GetRotationOffset(), 1 / GetTurnSpeed());
            m_TurnAnimation.Play();

            return m_TurnAnimation;
        }

        public Vector3Animation Jump()
        {
            m_IsMoving = true;

            m_MoveAnimation.ForEachAnims(x => x.SetExponent(0.7f));
            m_MoveAnimation.Init(transform.localPosition, transform.localPosition + new Vector3(0, JUMP_HEIGHT, 0), 0.4f);
            m_MoveAnimation.OnFinished += JumpSecondPart;
            m_MoveAnimation.Play();
                
            return m_MoveAnimation;
        }

        private void JumpSecondPart(Vector3 p_End)
        {
            m_MoveAnimation.OnFinished -= JumpSecondPart;
            m_MoveAnimation.OnFinished += JumpFinished;

            m_MoveAnimation.ForEachAnims(x => x.SetExponent(3f));
            m_MoveAnimation.Init(transform.localPosition, transform.localPosition - new Vector3(0, JUMP_HEIGHT, 0), 0.5f);
            m_MoveAnimation.Play();
        }

        private void JumpFinished(Vector3 p_End)
        {
            m_MoveAnimation.OnFinished -= JumpFinished;
            m_MoveAnimation.ForEachAnims(x => x.SetExponent(1));
        }

        public Vector3Animation LookAtPosition(Vector3 p_Position)
        {
            var l_Quaternion = Quaternion.LookRotation(transform.localPosition - new Vector3(p_Position.x, GetPositionOffset().y, p_Position.z));
            var l_EulerAngles = l_Quaternion.eulerAngles;
            Turn(Quaternion.Euler(l_EulerAngles));
            return m_TurnAnimation;
        }

        public void StopMovements()
        {
            m_MoveAnimation.Stop();
            m_TurnAnimation.Stop();
        }

        private void OnMove(Vector3 p_Pos)
        {
            transform.localPosition = p_Pos;
            foreach (var l_Item in m_StolenObjects)
            {
                l_Item.Key.transform.localPosition = transform.localPosition + new Vector3(0, 0.3f, 0);
            }
        }

        private void OnTurn(Vector3 p_Rot)
        {
            transform.localRotation = Quaternion.Euler(p_Rot);
            foreach (var l_Item in m_StolenObjects)
            {
                l_Item.Key.transform.localRotation = transform.localRotation;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public Vector3Animation StealObject(GameObject p_Target) {
            ReleaseStolenObjects();

            Move(p_Target.transform.localPosition, true).SetFinishedCallback(() =>
            {
                m_StolenObjects.Add(p_Target, new StolenObjectTransform(p_Target.transform.position, p_Target.transform.rotation.eulerAngles, p_Target.transform.localScale, p_Target.transform.parent));
                //p_Target.transform.SetParent(transform, false);
                p_Target.transform.localPosition = transform.localPosition + new Vector3(0, 0.3f, 0);
            });
            return m_MoveAnimation;
        }

        public void ReleaseStolenObjects()
        {
            foreach (var l_Item in m_StolenObjects)
            {
                //l_Item.Key.transform.SetParent(l_Item.Value.OriginalParent, false);
                l_Item.Key.transform.localPosition = l_Item.Value.Position;
                l_Item.Key.transform.localRotation = Quaternion.Euler(l_Item.Value.Rotation);
                l_Item.Key.transform.localScale = l_Item.Value.Scale;
            }

            m_StolenObjects.Clear();
        }
    }

}
