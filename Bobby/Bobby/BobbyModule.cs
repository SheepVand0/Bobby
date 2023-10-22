using BeatSaberPlus.SDK.Game;
using Bobby.Core;
using CP_SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobby
{
    internal class BobbyModule : CP_SDK.ModuleBase<BobbyModule>
    {
        public override EIModuleBaseType Type => EIModuleBaseType.External;

        public override string Name => "Bobby";

        public override string Description => "A random pet";

        public override bool UseChatFeatures => true;

        public override bool IsEnabled { get => BConfig.Instance.IsEnabled; set => BConfig.Instance.IsEnabled = value; }

        public override EIModuleBaseActivationType ActivationType => EIModuleBaseActivationType.OnMenuSceneLoaded;

        protected override void OnDisable()
        {
            if (Core.Bobby.Instance != null)
            {
                Core.Bobby.Instance.gameObject.SetActive(false);
            }
        }

        protected override void OnEnable()
        {
            if (Core.Bobby.Instance == null)
                Pet.CreatePet<Core.Bobby>();
            else
                Core.Bobby.Instance.gameObject.SetActive(true);
        }
    }
}
