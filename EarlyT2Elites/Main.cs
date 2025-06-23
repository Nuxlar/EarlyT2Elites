using BepInEx;
using System.Diagnostics;
using R2API;
using static RoR2.CombatDirector;
using RoR2;
using BepInEx.Configuration;

namespace EarlyT2Elites
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Main : BaseUnityPlugin
  {
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "Nuxlar";
    public const string PluginName = "EarlyT2Elites";
    public const string PluginVersion = "1.0.0";

    internal static Main Instance { get; private set; }
    public static string PluginDirectory { get; private set; }

    public static ConfigEntry<int> minStages;
    public static ConfigEntry<float> healthCoeff;
    public static ConfigEntry<float> damageCoeff;
    public static ConfigEntry<float> costCoeff;
    private static ConfigFile ETEConfig { get; set; }

    public void Awake()
    {
      Instance = this;

      Stopwatch stopwatch = Stopwatch.StartNew();

      Log.Init(Logger);

      ETEConfig = new ConfigFile(Paths.ConfigPath + "\\com.Nuxlar.EarlyT2Elites.cfg", true);
      minStages = ETEConfig.Bind<int>("General", "Minimum Stages", 4, "Which stage should Tier 2 elites start spawning? Vanilla 6.");
      healthCoeff = ETEConfig.Bind<float>("General", "Health Coefficient", 12f, "Health multiplier for Tier 2 elites. Vanilla 16.");
      damageCoeff = ETEConfig.Bind<float>("General", "Damage Coefficient", 4f, "Damage multiplier for Tier 2 elites. Vanilla 6.");
      costCoeff = ETEConfig.Bind<float>("General", "Cost Coefficient", 24f, "Cost for Tier 2 elites to spawn. Vanilla 36.");

      On.RoR2.CombatDirector.Init += TweakT2s;

      stopwatch.Stop();
      Log.Info_NoCallerPrefix($"EarlyT2Elites: Initialized in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
    }

    private static void TweakT2s(On.RoR2.CombatDirector.orig_Init orig)
    {
      orig();
      EliteTierDef t2Tier = EliteAPI.VanillaEliteTiers[5];
      if (minStages.Value != 6) t2Tier.isAvailable = (eliteRules) =>
      {
        return Run.instance && Run.instance.stageClearCount >= (minStages.Value - 1);
      };
      t2Tier.costMultiplier = costCoeff.Value;
      foreach (EliteDef ed in t2Tier.eliteTypes)
      {
        if (ed != null)
        {
          ed.healthBoostCoefficient = healthCoeff.Value;
          ed.damageBoostCoefficient = damageCoeff.Value;
        }
      }
    }
  }
}