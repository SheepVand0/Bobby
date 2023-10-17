using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bobby.Core
{
    internal class Bobby : Pet
    {
        public override List<Action<Vector3, bool>> GetActionLoopActions()
        {
            return new List<Action<Vector3, bool>>();
        }

        public override int GetActionLoopDelay() => 5000;


        public override Vector3 GetBasePosition() => new Vector3();

        public override Vector3 GetBaseRotation() => new Vector3();

        public override bool GetCanLoop() => true;

        public override float GetMaxRadius() => 10.5f;

        public override async Task<GameObject> GetModel()
        {
            m_BobbyNote = await GetNotePrefab(Color.red);
            m_NotePrefab = m_BobbyNote;

            return m_BobbyNote;
        }

        public override string GetName() => "Bobby";

        public override Vector3 GetPositionOffset()
         => new Vector3();

        public override float GetSpeed()
        => BConfig.GetPetConfig().Speed;

        public override float GetStealSpeed()
        => BConfig.GetPetConfig().StealSpeed;

        public override float GetTurnSpeed()
        => BConfig.GetPetConfig().TurnSpeed;

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        protected GameObject m_NotePrefab = null;
        protected GameObject m_BobbyNote = null;

        ////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        
        public async Task<GameObject> GetNotePrefab(Color p_CubeColor)
        {
            GameObject l_Note = null;
            bool l_Finished = false;
            SceneManager.LoadSceneAsync("StandardGameplay", LoadSceneMode.Additive).completed += (_) =>
            {
                var l_BeatmapObjectInstaller = Resources.FindObjectsOfTypeAll<BeatmapObjectsInstaller>().First();
                var l_NotePrefab = l_BeatmapObjectInstaller.GetField<GameNoteController, BeatmapObjectsInstaller>("_normalBasicNotePrefab");

                l_Note = Instantiate(l_NotePrefab.transform.GetChild(0).gameObject);

                SetColor(Color.red, l_Note);

                l_Note.GetComponentInChildren<NoteBigCuttableColliderSize>().gameObject.SetActive(false);
                l_Note.GetComponentInChildren<BoxCuttableBySaber>().gameObject.SetActive(false);

                SceneManager.UnloadSceneAsync("StandardGameplay");

                l_Finished = true;
            };

            await WaitUtils.Wait(() => l_Finished == true, 1);
            return l_Note;
        }

        public void SetColor(Color p_Color, GameObject p_NoteObject = null)
        {
            if (p_NoteObject == null)
                p_NoteObject = m_BobbyNote;

            foreach (var l_Item in p_NoteObject.GetComponents<MaterialPropertyBlockController>())
            {
                l_Item.materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), p_Color);
                l_Item.ApplyChanges();
            }
        }

    }
}
