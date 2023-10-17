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
        public override EIModuleBaseType Type => EIModuleBaseType.Integrated;

        public override string Name => "Bobby";

        public override string Description => "A random pet";

        public override bool UseChatFeatures => true;

        public override bool IsEnabled { get => BConfig.Instance.IsEnabled; set => BConfig.Instance.IsEnabled = value; }

        public override EIModuleBaseActivationType ActivationType => EIModuleBaseActivationType.OnMenuSceneLoaded;

        protected override void OnDisable()
        {

        }

        protected override void OnEnable()
        {
            //Pet.CreatePet<Bobby.Core.Bobby>();
        }
    }
}
