using CommNet;
using KIT.Constants;
using KIT.Resources;
using KIT.ResourceScheduler;
using KSP.Localization;
using System;
using System.Linq;
using UnityEngine;


namespace KIT
{
    public class AIHome : PartModule
    {
        [KSPField(isPersistant = true)]
        public double AIMoveInTime;

        // Maximum loss of reputation from destroying the AI's home.
        [KSPField]
        public float destructionPenaltyMax = 150;

        private void OnJustAboutToBeDestroyed()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            // Has the AI moved in yet?
            if (AIMoveInTime == 0) return;

            // Transfer the AI away from the ship, just in time for it to watch
            // it's home to be destroyed.

            DestroyAIsHome();
        }

        public override void OnStart(PartModule.StartState state)
        {
            if (state == StartState.Editor) return;

            // ensure it's called only once.
            part.OnJustAboutToBeDestroyed -= OnJustAboutToBeDestroyed;
            part.OnJustAboutToBeDestroyed += OnJustAboutToBeDestroyed;
        }

        public void DestroyAIsHome()
        {
            double delta = Planetarium.GetUniversalTime() - AIMoveInTime;
            float penalty = (float)Math.Min(destructionPenaltyMax, (delta * 0.01));

            // Destroying peoples homes is never a good look.

            Reputation.Instance.AddReputation(-penalty, TransactionReasons.VesselLoss);
            Debug.Log(String.Format("DestroyAIsHOME: removing {0} points of reputation", penalty));
        }

        public void NewHome()
        {
            // Let's throw a welcoming party for the AI to his new home! Everyone is invited, and is at
            AIMoveInTime = Planetarium.GetUniversalTime();
        }
    }

    class ComputerCore : ModuleModableScienceGenerator, ITelescopeController, IUpgradeableModule, CommNet.ICommNetControlSource, IRelayEnabler
    {
        // Persistent
        [KSPField(isPersistant = true, guiActive = true, guiName = "#LOC_KSPIE_ComputerCore_Name")]//Name
        public string nameStr = "";
        [KSPField(guiActive = true, guiName = "#LOC_KSPIE_ComputerCore_DataCollectionRate")]//Data Collection Rate
        public string scienceRate;
        [KSPField(isPersistant = true, guiName = "#LOC_KSPIE_ComputerCore_AIOnline", guiActive = true, guiActiveEditor = true), UI_Toggle(disabledText = "#LOC_KSPIE_ComputerCore_AIOnline_Off", enabledText = "#LOC_KSPIE_ComputerCore_AIOnline_On", scene = UI_Scene.All)]//AI Online--Off--On
        public bool IsEnabled = false;
        [KSPField(isPersistant = true, guiName = "#LOC_KSPIE_ComputerCore_IsPowered", guiActive = true, guiActiveEditor = false)]//Powered
        public bool IsPowered;
        [KSPField(isPersistant = true, guiActiveEditor = true)]
        public bool isupgraded;
        [KSPField(isPersistant = true)]
        public double electrical_power_ratio;
        [KSPField(isPersistant = true, guiName = "#LOC_KSPIE_ComputerCore_Datastored", guiActive = true, guiActiveEditor = false)]//Data stored
        public double science_to_add;
        [KSPField(isPersistant = true)]
        public bool coreInit = false;
        [KSPField]
        public string upgradeTechReq = null;
        [KSPField]
        public string upgradedName = "";
        [KSPField]
        public string originalName = "";
        [KSPField]
        public float upgradeCost = 100;
        [KSPField]
        public float megajouleRate = 1;
        [KSPField]
        public float upgradedMegajouleRate = 10;
        [KSPField]
        public double powerReqMult = 1;
        [KSPField]
        public double activeAIControlDistance = 1.0e+13; // Distance from the Large Multi Bandwidth Dish Transciever
        [KSPField]
        public double inactiveAIControlDistance = 100000;

        //Gui
        [KSPField]
        const double baseScienceRate = 0.3;
        [KSPField(guiActive = true, guiName = "#LOC_KSPIE_ComputerCore_Type")]//Type
        public string computercoreType;
        [KSPField(guiActive = true, guiName = "#LOC_KSPIE_ComputerCore_Upgrade")]//Upgrade
        public string upgradeCostStr;

        // Privates
        double _scienceRateF;
        double _effectivePowerRequirement;

        ConfigNode _experimentNode;
        BaseField _nameStrField;
        BaseField _isEnabledField;
        BaseField _isPoweredField;
        BaseField _upgradeCostStrField;
        BaseField _scienceRateField;
        BaseEvent _retrofitCoreEvent;
        ModuleDataTransmitter _moduleDataTransmitter;
        ModuleCommand _moduleCommand;
        AIHome _moduleAIHome;

        //Properties
        public string UpgradeTechnology => upgradeTechReq;
        public bool CanProvideTelescopeControl => isupgraded && IsEnabled && IsPowered;

        // Events
        [KSPEvent(guiActive = true, guiName = "#LOC_KSPIE_ComputerCore_Retrofit", active = true)]//Retrofit
        public void RetrofitCore()
        {
            if (ResearchAndDevelopment.Instance == null) return;
            if (isupgraded || ResearchAndDevelopment.Instance.Science < upgradeCost) return;

            if (_moduleAIHome != null)
                _moduleAIHome.NewHome();

            upgradePartModule();
            ResearchAndDevelopment.Instance.AddScience(-upgradeCost, TransactionReasons.RnDPartPurchase);
        }

        // Public Overrides
        public override void OnStart(StartState state)
        {
            string[] resourcesToSupply = { ResourceSettings.Config.ThermalPowerInMegawatt, ResourceSettings.Config.ChargedParticleInMegawatt, ResourceSettings.Config.ElectricPowerInMegawatt, ResourceSettings.Config.WasteHeatInMegawatt };
            // this.resources_to_supply = resourcesToSupply;

            _isEnabledField = Fields[nameof(IsEnabled)];
            _isPoweredField = Fields[nameof(IsPowered)];
            _upgradeCostStrField = Fields[nameof(upgradeCostStr)];
            _retrofitCoreEvent = Events[nameof(RetrofitCore)];
            _nameStrField = Fields[nameof(nameStr)];
            _scienceRateField = Fields[nameof(scienceRate)];

            if (state == StartState.Editor)
            {
                if (!this.HasTechsRequiredToUpgrade()) return;

                isupgraded = true;
                upgradePartModule();
                return;
            }

            Debug.Log("[KSPI]: ComputerCore on " + part.name + " was Force Activated");
            part.force_activate();

            _moduleDataTransmitter = part.FindModuleImplementing<ModuleDataTransmitter>();
            _moduleCommand = part.FindModuleImplementing<ModuleCommand>();
            _moduleAIHome = part.FindModuleImplementing<AIHome>();

            if (isupgraded || !PluginHelper.TechnologyIsInUse)
                upgradePartModule();
            else
                computercoreType = originalName;

            _effectivePowerRequirement = (isupgraded ? upgradedMegajouleRate : megajouleRate) * powerReqMult;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (_moduleDataTransmitter != null)
                _moduleDataTransmitter.antennaPower = IsEnabled && IsPowered ? activeAIControlDistance : inactiveAIControlDistance;

            if (ResearchAndDevelopment.Instance != null)
                _retrofitCoreEvent.active = !isupgraded && ResearchAndDevelopment.Instance.Science >= upgradeCost;
            else
                _retrofitCoreEvent.active = false;

            var isUpgradedOrNoActiveScience = isupgraded || !PluginHelper.TechnologyIsInUse;

            _isEnabledField.guiActive = isUpgradedOrNoActiveScience;
            _upgradeCostStrField.guiActive = !isupgraded;
            _nameStrField.guiActive = isUpgradedOrNoActiveScience;
            _scienceRateField.guiActive = isUpgradedOrNoActiveScience;
            _isPoweredField.guiActive = isUpgradedOrNoActiveScience;

            var science = _scienceRateF * GameConstants.KEBRIN_DAY_SECONDS * PluginHelper.getScienceMultiplier(vessel);
            scienceRate = science.ToString("0.000") + "/ Day";

            if (ResearchAndDevelopment.Instance != null)
                upgradeCostStr = ResearchAndDevelopment.Instance.Science + "/" + upgradeCost.ToString("0") + " Science";//
        }

        protected override bool generateScienceData()
        {
            ScienceExperiment experiment = ResearchAndDevelopment.GetExperiment(experimentID);
            if (experiment == null)
                return false;

            if (science_to_add > 0)
            {
                ScienceSubject subject = ResearchAndDevelopment.GetExperimentSubject(experiment, ScienceUtil.GetExperimentSituation(vessel), vessel.mainBody, "", "");
                if (subject == null)
                    return false;
                subject.subjectValue = PluginHelper.getScienceMultiplier(vessel);
                subject.scienceCap = 167 * subject.subjectValue;
                subject.dataScale = 1.25f;

                science_to_add = Math.Min(science_to_add, (subject.scienceCap - subject.science) / subject.subjectValue);

                // transmission of zero data breaks the experiment result dialog box
                data_size = Math.Max(float.Epsilon, science_to_add * subject.dataScale);
                science_data = new ScienceData((float)data_size, 1, 0, subject.id, "Science Lab Data");

                result_title = experiment.experimentTitle;
                result_string = nameStr + " " + GetRandomExperimentResult();

                recovery_value = science_to_add;
                transmit_value = recovery_value;
                xmit_scalar = 1;
                ref_value = subject.scienceCap;

                return true;
            }
            return false;
        }

        protected override void cleanUpScienceData()
        {
            science_to_add = 0;
        }

        public override string GetInfo()
        {
            return Localizer.Format("#LOC_KSPIE_ComputerCore_getInfo1") + " " + PluginHelper.getFormattedPowerString(megajouleRate) + "\n" +
                Localizer.Format("#LOC_KSPIE_ComputerCore_getInfo2") + PluginHelper.getFormattedPowerString(upgradedMegajouleRate);//"Upgraded Power Requirements: "
        }

        // IUpgradeableModule
        public void upgradePartModule()
        {
            computercoreType = upgradedName;
            if (nameStr == "")
            {
                ConfigNode[] nameList = GameDatabase.Instance.GetConfigNodes("AI_CORE_NAME");
                ConfigNode myName = nameList[new System.Random().Next(0, nameList.Length)];
                nameStr = myName.GetValue("name");
            }

            isupgraded = true;
            canDeploy = true;

            _experimentNode = GameDatabase.Instance.GetConfigNodes("EXPERIMENT_DEFINITION").FirstOrDefault(nd => nd.GetValue("id") == experimentID);
        }

        // Privates
        private string GetRandomExperimentResult()
        {
            try
            {
                string[] resultStrings = _experimentNode.GetNode("RESULTS").GetValuesStartsWith("default");
                return resultStrings[new System.Random().Next(resultStrings.Length)];
            }
            catch (Exception ex)
            {
                Debug.Log("[KSPI]: Exception Generation Experiment Result: " + ex.Message + ": " + ex.StackTrace);
                return " has detected a glitch in the universe and recommends checking your installation of KSPInterstellar.";
            }
        }

        // This implements the ICommNetControlSource interface, and is used for determining control of the vessel
        // in the CommNetVessel code.
        string ICommNetControlSource.name => "AI Control";
        public void UpdateNetwork()
        {
        }

        public VesselControlState GetControlSourceState()
        {
            return (IsEnabled && IsPowered) ? VesselControlState.ProbeFull : VesselControlState.None;
        }

        public bool IsCommCapable() => vessel.connection.IsConnected;


        // IRelayEnabler documentation:
        // Any module that implements this interface can make all antennae, not just those of type RELAY, work as a relay.

        // This allows f.e. the XFELT antennas to be used as a RELAY while in AI mode. This will be useful for Galaxies
        // Unbound and setting up Interstellar relay networks (under the stock commnet system, and maybe commnet constellations)

        public bool CanRelay() => IsEnabled && IsPowered;

        public bool CanRelayUnloaded(ProtoPartModuleSnapshot mSnap)
        {
            bool Is_Enabled, Is_Powered;

            Is_Enabled = Is_Powered = false;

            if (!mSnap.moduleValues.TryGetValue(nameof(ComputerCore.IsEnabled), ref Is_Enabled)) return false;
            if (!mSnap.moduleValues.TryGetValue(nameof(ComputerCore.IsPowered), ref Is_Powered)) return false;

            return Is_Enabled && Is_Powered;
        }

        public new ResourcePriorityValue ResourceProcessPriority() => ResourcePriorityValue.First;

        public new void KITFixedUpdate(IResourceManager resMan)
        {
            base.KITFixedUpdate(resMan);

            if (!isupgraded || !IsEnabled)
            {
                IsPowered = false;
                _scienceRateF = 0;
                electrical_power_ratio = 0;
                science_to_add = 0;
            }

            var powerReturned = resMan.ConsumeResource(ResourceName.ElectricCharge, _effectivePowerRequirement * GameConstants.ecPerMJ) / GameConstants.ecPerMJ;

            electrical_power_ratio = powerReturned / _effectivePowerRequirement;
            IsPowered = electrical_power_ratio > 0.99;

            if (!IsPowered)
            {
                if (vessel != null && vessel.connection != null)
                {
                    vessel.connection.UnregisterCommandSource(this);

                    part.isControlSource = Vessel.ControlLevel.NONE;

                    if (vessel.connection.Comm != null)
                    {
                        vessel.connection.Comm.isHome = false;
                        vessel.connection.Comm.isControlSource = false;
                    }
                }

                part.RequestResource(ResourceSettings.Config.ElectricPowerInMegawatt, -powerReturned * TimeWarp.fixedDeltaTime);
                return;
            }
            
            part.isControlSource = Vessel.ControlLevel.FULL;

            if (vessel != null && vessel.connection != null)
            {
                vessel.connection.RegisterCommandSource(this);

                part.isControlSource = Vessel.ControlLevel.FULL;

                if (vessel.connection.Comm != null)
                {
                    vessel.connection.Comm.isHome = true;
                    vessel.connection.Comm.isControlSource = true;
                }

                return;
            }

            var altitudeMultiplier = Math.Max(vessel.altitude / vessel.mainBody.Radius, 1);

            var scienceMultiplier = PluginHelper.getScienceMultiplier(vessel);

            _scienceRateF = baseScienceRate * scienceMultiplier / GameConstants.KEBRIN_DAY_SECONDS * powerReturned / _effectivePowerRequirement / Math.Sqrt(altitudeMultiplier);

            if (ResearchAndDevelopment.Instance != null && !double.IsInfinity(_scienceRateF) && !double.IsNaN(_scienceRateF))
                science_to_add += _scienceRateF * TimeWarp.fixedDeltaTime;
        }
    }
}

