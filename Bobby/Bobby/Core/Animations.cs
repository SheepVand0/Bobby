using System;
using UnityEngine;

namespace Bobby.Core
{
    public class FloatAnimation : MonoBehaviour
    {
        public event Action<float> OnChange;

        public event Action<float> OnFinished;

        protected Action m_FinishedCallback;

        private float m_Start;

        private float m_End;

        private bool m_Started;

        private float m_Duration = 0;

        private float m_ValueDuration = 0;

        private float m_StartTime = 0;

        public float GetStart() => m_Start;
        public float GetEnd() => m_End;
        public bool IsPlaying() => m_Started;
        public float Duration() => m_Duration;

        protected virtual void OnPlay()
        {

        }

        protected virtual void OnStop()
        {

        }

        protected virtual void OnInit()
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init anim
        /// </summary>
        /// <param name="p_Start"></param>
        /// <param name="p_Value"></param>
        /// <param name="p_Duration"></param>
        public void Init(float p_Start, float p_Value, float p_Duration)
        {
            m_Start = p_Start;
            m_End = p_Value;
            m_ValueDuration = m_End - m_Start;
            m_Duration = p_Duration;
            OnInit();
        }

        public void SetFinishedCallback(Action p_Callback)
        {
            m_FinishedCallback = p_Callback;
        }

        ////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get value from current time, start and end
        /// </summary>
        public void Update()
        {
            if (m_Started == false) return;

            float l_Prct = (UnityEngine.Time.realtimeSinceStartup - m_StartTime) / m_Duration;

            float l_Value = m_Start + (m_ValueDuration * l_Prct);

            OnChange?.Invoke(l_Value);

            if (l_Prct > 1) { 
                m_Started = false; 
                OnFinished?.Invoke(l_Value); 
                if (m_FinishedCallback != null) {
                    m_FinishedCallback.Invoke();
                }
                m_FinishedCallback = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start animation
        /// </summary>
        public void Play()
        {
            m_StartTime = UnityEngine.Time.realtimeSinceStartup;
            if (m_Duration == 0 || float.IsPositiveInfinity(m_Duration) || float.IsNegativeInfinity(m_Duration))
            {
                OnChange?.Invoke(m_End);
                OnFinished?.Invoke(m_End);
                GameObject.DestroyImmediate(gameObject);
                return;
            }

            m_Started = true;
            OnPlay();
        }

        /// <summary>
        /// Stop current animation
        /// </summary>
        public void Stop()
        {
            m_Started = false;
            OnStop();
        }

    }

    public class Vector3Animation : MonoBehaviour
    {

        public event Action<Vector3> OnVectorChange;

        public event Action<Vector3> OnFinished;

        public Vector3 m_Start;

        public Vector3 m_End;

        Vector3 m_Current;

        public float m_Duration;

        private int m_FinishedAnimCount = 0;

        private Action m_FinishedCallback;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        FloatAnimation m_XAnim;
        FloatAnimation m_YAnim;
        FloatAnimation m_ZAnim;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create an animation in GameObject if no existing, else return existing
        /// </summary>
        /// <param name="p_GameObject">Target GameObject</param>
        /// <param name="p_Animation">Returned animation</param>
        public static void AddAnim(GameObject p_GameObject, out Vector3Animation p_Animation)
        {
            Vector3Animation l_ExistingAnim = p_GameObject.GetComponent<Vector3Animation>();
            if (l_ExistingAnim != null)
                p_Animation = l_ExistingAnim;
            else
                p_Animation = p_GameObject.AddComponent<Vector3Animation>();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init animation
        /// </summary>
        /// <param name="p_Start">Start value</param>
        /// <param name="p_Value">End value</param>
        /// <param name="p_Duration">Animation duration</param>
        public void Init(Vector3 p_Start, Vector3 p_Value, float p_Duration)
        {
            m_FinishedAnimCount = 0;
            m_Start = p_Start;
            m_End = p_Value;

            m_Duration = p_Duration;
        }

        public void SetFinishedCallback(Action p_Callback)
        {
            m_FinishedCallback = p_Callback;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// If all float animations have finished, invoke OnFinished event
        /// </summary>
        private void CheckFinishedAnims()
        {
            if (m_FinishedAnimCount == 3)
            {
                OnFinished?.Invoke(m_Current);
                if (m_FinishedCallback != null)
                {
                    m_FinishedCallback.Invoke();
                    m_FinishedCallback = null;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Play animation
        /// </summary>
        public void Play()
        {
            if (m_XAnim == null)
            {
                m_XAnim = gameObject.AddComponent<FloatAnimation>();
                m_XAnim.OnChange += (p_Val) =>
                {
                    m_Current = new Vector3(p_Val, m_Current.y, m_Current.z);
                    OnVectorChange?.Invoke(m_Current);
                };
                m_XAnim.OnFinished += (p_Val) => { m_FinishedAnimCount += 1; CheckFinishedAnims(); };
            }

            if (m_YAnim == null)
            {
                m_YAnim = gameObject.AddComponent<FloatAnimation>();
                m_YAnim.OnChange += (p_Val) =>
                {
                    m_Current = new Vector3(m_Current.x, p_Val, m_Current.z);
                    OnVectorChange?.Invoke(m_Current);
                };
                m_YAnim.OnFinished += (p_Val) => { m_FinishedAnimCount += 1; CheckFinishedAnims(); };
            }

            if (m_ZAnim == null)
            {
                m_ZAnim = gameObject.AddComponent<FloatAnimation>();
                m_ZAnim.OnChange += (p_Val) =>
                {
                    m_Current = new Vector3(m_Current.x, m_Current.y, p_Val);
                    OnVectorChange?.Invoke(m_Current);
                };
                m_ZAnim.OnFinished += (p_Val) => { m_FinishedAnimCount += 1; CheckFinishedAnims(); };
            }

            m_XAnim.Init(m_Start.x, m_End.x, m_Duration);
            m_YAnim.Init(m_Start.y, m_End.y, m_Duration);
            m_ZAnim.Init(m_Start.z, m_End.z, m_Duration);

            m_XAnim.Play();
            m_YAnim.Play();
            m_ZAnim.Play();
        }

        /// <summary>
        /// Stop current animation
        /// </summary>
        public void Stop()
        {
            foreach (FloatAnimation l_Current in gameObject.GetComponents<FloatAnimation>())
                l_Current.Stop();
        }
    }
}

