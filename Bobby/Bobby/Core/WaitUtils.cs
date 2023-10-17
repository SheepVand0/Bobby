using System;
using System.Threading.Tasks;
//using GuildSaber.Logger;

namespace Bobby.Core
{

    internal class WaitUtils
    {
        public static async Task<Task> Wait(Func<bool> p_Func, int p_ToleranceMs, int p_DelayAfter = 0, int p_MaxTryCount = 0, int p_CodeLine = -1)
        {
            int l_TryCount = 0;

            bool l_ShouldTryCount = p_MaxTryCount > 0;

            //GSLogger.Instance.Log($"Doing wait at line : {p_CodeLine}", IPA.Logging.Logger.LogLevel.InfoUp);

            do
            {
                try
                {
                    if (p_Func.Invoke()) break;

                    l_TryCount += 1;
                    await Task.Delay(p_ToleranceMs);
                }
                catch (Exception l_E)
                {
                    /*GSLogger.Instance.Error(l_E, nameof(WaitUtils), nameof(Wait));
                    if (p_CodeLine != -1) GSLogger.Instance.Error(new Exception($"At line {p_CodeLine}"), nameof(WaitUtils), nameof(Wait));*/
                }
            } while (l_ShouldTryCount ? l_TryCount < p_MaxTryCount : l_TryCount < 10000);
            if (p_DelayAfter != 0) await Task.Delay(p_DelayAfter);
            return Task.CompletedTask;
        }
    }

}