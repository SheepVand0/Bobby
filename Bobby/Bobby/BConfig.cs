using CP_SDK.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobby
{
    internal class BConfig : JsonConfig<BConfig>
    {
        public override string GetRelativePath() => $"Bobby/Config";

        [JsonProperty] internal bool IsEnabled = true;

        internal class PetConfig
        {
            [JsonProperty] internal bool CanStealElements = true;
            [JsonProperty] internal float Speed = 1.0f;
            [JsonProperty] internal float StealSpeed = 2.0f;
            [JsonProperty] internal float TurnSpeed = 1.0f;
        }

        [JsonProperty] internal PetConfig CurrentConfig = new PetConfig();

        public static ref PetConfig GetPetConfig()
        {
            return ref Instance.CurrentConfig;
        }

    }
}
