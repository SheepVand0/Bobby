using BeatSaberPlus.SDK.Game;
using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Bobby.Core
{
    internal class Bobby : Pet
    {
        internal static Bobby Instance;

        public static readonly List<string> STEALABLE_GAMEOBJECTS = new List<string>() { 
            "Feet", "PileOfNotes", "Logo"
        };

        protected AudioClip m_WoolSound;
        protected AudioSource m_AudioSource;

        IEnumerator GetClips()
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("https://github.com/SheepVand0/MySimplesCodes-NoUE/blob/main/Wool%20Placing%20(Nr.%204%20%20%20Minecraft%20Sound)%20-%20Sound%20Effect%20for%20editing.mp3?raw=true", AudioType.MPEG))
            {
                yield return www.SendWebRequest();
                if (www.isNetworkError == false)
                    m_WoolSound = DownloadHandlerAudioClip.GetContent(www);
            }

            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.clip = m_WoolSound;
        }

        public override List<Action<Vector3, bool>> GetActionLoopActions()
        {
            return new List<Action<Vector3, bool>>() {
                (p_RandomPos, p_IsMoving) =>
                {
                    if (p_IsMoving == true) return;
                    Move(p_RandomPos);
                },
                (p_RandomPos, p_IsMoving) =>
                {
                    if (p_IsMoving == true) return;
                    Jump();
                },
                (p_RandomPos, p_IsMoving) => {
                    if (p_IsMoving == true) return;

                    StealObject(GameObject.Find(STEALABLE_GAMEOBJECTS[UnityEngine.Random.Range(0, STEALABLE_GAMEOBJECTS.Count)]));
                }
            };
        }

        public override int GetActionLoopDelay() => 5000;

        public override Vector3 GetBasePosition() => new Vector3(-2.38f, 0, 1.5f);

        public override Vector3 GetBaseRotation() => new Vector3(0, 225-90, 0);

        public override bool GetCanLoop() => true;

        public override float GetMaxRadius() => 10.5f;

        protected override void Init()
        {
            /*Logic.OnSceneChange += (p_Scene) =>
            {
                if (p_Scene != Logic.ESceneType.Playing) return;

                //StopMovements();
            };*/
            Instance = this;
            StartCoroutine(GetClips());
        }

        public override GameObject GetModel()
        {
            InitNotePrefab(Color.red);
            m_BobbyNote = new GameObject("BobbyNote");

            return m_BobbyNote;
        }

        public override string GetName() => "Bobby";

        public override Vector3 GetPositionOffset()
         => new Vector3(0, 0.2f, 0);

        public override Vector3 GetRotationOffset()
        => new Vector3(90, 0, 0);

        public override float GetSpeed()
        => BConfig.GetPetConfig().Speed;

        public override float GetStealSpeed()
        => BConfig.GetPetConfig().StealSpeed;

        public override float GetTurnSpeed()
        => 2;

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        protected override void OnSteal(GameObject p_StolenObject)
        {
            m_AudioSource.volume = 0.5f;
            m_AudioSource.Play();
        }

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        protected GameObject m_NotePrefab = null;
        protected GameObject m_BobbyNote = null;

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        
        public void InitNotePrefab(Color p_CubeColor)
        {
            if (m_NotePrefab != null) return;

            GameObject l_Note = null;
            SceneManager.LoadSceneAsync("StandardGameplay", LoadSceneMode.Additive).completed += (_) =>
            {
                Plugin.Log.Info("Loaded");
                var l_BeatmapObjectInstaller = Resources.FindObjectsOfTypeAll<BeatmapObjectsInstaller>().First();
                var l_NotePrefab = l_BeatmapObjectInstaller.GetField<GameNoteController, BeatmapObjectsInstaller>("_normalBasicNotePrefab");

                l_Note = Instantiate(l_NotePrefab.transform.GetChild(0).gameObject);

                SetColor(Color.red, l_Note);

                l_Note.GetComponentInChildren<NoteBigCuttableColliderSize>().gameObject.SetActive(false);
                l_Note.GetComponentInChildren<BoxCuttableBySaber>().gameObject.SetActive(false);

                m_NotePrefab = l_Note;
                m_NotePrefab.transform.SetParent(m_BobbyNote.transform, false);
                SceneManager.UnloadSceneAsync("StandardGameplay");
            };
        }

        public void SetColor(Color p_Color, GameObject p_NoteObject = null)
        {
            GameObject l_NoteObject = p_NoteObject;
            if (p_NoteObject == null)
                l_NoteObject = m_BobbyNote;

            foreach (var l_Item in l_NoteObject.GetComponents<MaterialPropertyBlockController>())
            {
                l_Item.materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), p_Color);
                l_Item.ApplyChanges();
            }
        }

    }
}
