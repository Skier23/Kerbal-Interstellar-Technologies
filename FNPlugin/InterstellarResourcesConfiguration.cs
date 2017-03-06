﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FNPlugin
{
    public class InterstellarResourcesConfiguration
    {
        private static InterstellarResourcesConfiguration _instance = null;

        private const String _ALIMINIUM = "Aluminium";
        private const String _ANTIMATTER = "Antimatter";
        private const String _INTAKE_ATMOSPHERE = "IntakeAtm";
        private const String _THORIUM_TETRAFLOURIDE = "ThF4";
        private const String _URANIUM_NITRIDE = "UraniumNitride";
        private const String _ACTINIDES = "Actinides";
        private const String _DEPLETED_FUEL = "DepletedFuel";
        private const String _VACUUM_PLASMA = "VacuumPlasma";
        private const String _EXOTIC_MATTER = "ExoticMatter";
        private const String _INTAKE_AIR = "IntakeAir";
        private const String _LITHIUM7 = "Lithium";
        private const String _LITHIUM6 = "Lithium6";
        private const String _PLUTONIUM_238 = "Plutonium-238";
        private const String _ALUMINA = "Alumina";
        private const String _LIQUID_DEUTERIUM = "LqdDeuterium";
        private const String _DEUTERIUM_GAS = "Deuterium";
        private const String _INTAKE_LIQUID = "IntakeLqd";

        private String _uranium_TerraFloride = "UF4";
        private String _aluminium = "Aluminium";
        private String _ammonia = "LqdAmmonia";
        private String _argon = "LqdArgon";
        private String _carbonDioxide = "LqdCO2";
        private String _carbonMoxoxide = "LqdCO";
        private String _helium4_gas = "Helium4";
        private String _liquid_helium4 = "LqdHelium";
        private String _helium3_gas = "Helium3";
        private String _liquid_helium3 = "LqdHe3";
        private String _sodium = "Sodium";
        private String _hydrogen = "LqdHydrogen";
        private String _hydrogen_peroxide = "HTP";
        private String _hydrazine = "Hydrazine";
        private String _methane = "LqdMethane";
        private String _nitrogen = "LqdNitrogen";
        private String _nitrogen15 = "LqdNitrogen15";
        private String _oxygen = "LqdOxygen";
        private String _water = "Water";
        private String _heavyWater = "HeavyWater";
        private String _tritium = "LqdTritium";
        private String _solarWind = "SolarWind";
        private String _neon_gas = "NeonGas";
        private String _regolith = "Regolith";
        private String _xenongas = "XenonGas";
        private String _krytongas = "KryptonGas";

        public String Actinides { get { return _ACTINIDES; } }
        public String Alumina { get { return _ALUMINA; } }
        public String Aluminium { get { return _aluminium; } }
        public String Ammonia { get { return _ammonia; } }
        public String Antimatter { get { return _ANTIMATTER; } }
        public String Argon { get { return _argon; } }
        public String CarbonDioxide { get { return _carbonDioxide; } }
        public String CarbonMoxoxide { get { return _carbonMoxoxide; } }
        public String DepletedFuel { get { return _DEPLETED_FUEL; } }
        public String LqdDeuterium { get { return _LIQUID_DEUTERIUM; } }
        public String DeuteriumGas { get { return _DEUTERIUM_GAS; } }
        public String ExoticMatter { get { return _EXOTIC_MATTER; } }
        public String LqdHelium4 { get { return _liquid_helium4; } }
        public String Helium4Gas { get { return _helium4_gas; } }
        public String LqdHelium3 { get { return _liquid_helium3; } }
        public String Sodium { get { return _sodium; } }
        public String Helium3Gas { get { return _helium3_gas; } }
        public String Hydrogen { get { return _hydrogen; } }
        public String HydrogenPeroxide { get { return _hydrogen_peroxide; } }
        public String Hydrazine { get { return _hydrazine; } }
        public String IntakeAtmosphere { get { return _INTAKE_ATMOSPHERE; } }
        public String IntakeLiquid { get { return _INTAKE_LIQUID; } }
        public String Lithium6 { get { return _LITHIUM6; } }
        public String IntakeAir { get { return _INTAKE_AIR; } }
        public String Lithium7 { get { return _LITHIUM7; } }
        public String Methane { get { return _methane; } }
        public String NeonGas { get { return _neon_gas; } }
        public String Nitrogen { get { return _nitrogen; } }
        public String Nitrogen15 { get { return _nitrogen15; } }
        public String Oxygen { get { return _oxygen; } }
        public String Plutonium238 { get { return _PLUTONIUM_238; } }
        public String Regolith { get { return _regolith; } }
        public String SolarWind { get { return _solarWind; } }
        public String ThoriumTetraflouride { get { return _THORIUM_TETRAFLOURIDE; } }
        public String LqdTritium { get { return _tritium; } }
        public String UraniumTetraflouride { get { return _uranium_TerraFloride; } }
        public String UraniumNitride { get { return _URANIUM_NITRIDE; } }
        public String VacuumPlasma { get { return _VACUUM_PLASMA; } }
        public String Water { get { return _water; } }
        public String HeavyWater { get { return _heavyWater; } }
        public String XenonGas { get { return _xenongas; } }
        public String KryptonGas { get { return _krytongas; } }

        public InterstellarResourcesConfiguration(ConfigNode plugin_settings)
        {
            if (plugin_settings != null)
            {
                if (plugin_settings.HasValue("AluminiumResourceName"))
                {
                    _aluminium = plugin_settings.GetValue("AluminiumResourceName");
                    Debug.Log("[KSP Interstellar] Aluminium resource name set to " + Aluminium);
                }
                if (plugin_settings.HasValue("AmmoniaResourceName"))
                {
                    _ammonia = plugin_settings.GetValue("AmmoniaResourceName");
                    Debug.Log("[KSP Interstellar] Ammonia resource name set to " + Ammonia);
                }
                if (plugin_settings.HasValue("ArgonResourceName"))
                {
                    _argon = plugin_settings.GetValue("ArgonResourceName");
                    Debug.Log("[KSP Interstellar] Argon resource name set to " + Argon);
                }
                if (plugin_settings.HasValue("CarbonDioxideResourceName"))
                {
                    _carbonDioxide = plugin_settings.GetValue("CarbonDioxideResourceName");
                    Debug.Log("[KSP Interstellar] CarbonDioxide resource name set to " + CarbonDioxide);
                }
                if (plugin_settings.HasValue("CarbonMonoxideResourceName"))
                {
                    _carbonMoxoxide = plugin_settings.GetValue("CarbonMonoxideResourceName");
                    Debug.Log("[KSP Interstellar] CarbonMonoxide resource name set to " + CarbonMoxoxide);
                }
                if (plugin_settings.HasValue("Helium4GasResourceName"))
                {
                    _helium4_gas = plugin_settings.GetValue("Helium4GasResourceName");
                    Debug.Log("[KSP Interstellar] Helium4 Gas resource name set to " + LqdHelium4);
                }
                if (plugin_settings.HasValue("Helium3GasResourceName"))
                {
                    _helium3_gas = plugin_settings.GetValue("Helium3GasResourceName");
                    Debug.Log("[KSP Interstellar] Helium3 Gas resource name set to " + LqdHelium4);
                }
                if (plugin_settings.HasValue("HeliumResourceName"))
                {
                    _liquid_helium4 = plugin_settings.GetValue("HeliumResourceName");
                    Debug.Log("[KSP Interstellar] Helium4 Liquid resource name set to " + LqdHelium4);
                }
                if (plugin_settings.HasValue("Helium3ResourceName"))
                {
                    _liquid_helium3 = plugin_settings.GetValue("Helium3ResourceName");
                    Debug.Log("[KSP Interstellar] Helium3 resource name set to " + LqdHelium3);
                }
                if (plugin_settings.HasValue("HydrazineResourceName"))
                {
                    _hydrazine = plugin_settings.GetValue("HydrazineResourceName");
                    Debug.Log("[KSP Interstellar] Hydrazine resource name set to " + Hydrazine);
                }
                if (plugin_settings.HasValue("HydrogenResourceName"))
                {
                    _hydrogen = plugin_settings.GetValue("HydrogenResourceName");
                    Debug.Log("[KSP Interstellar] Hydrogen resource name set to " + Hydrogen);
                }
                if (plugin_settings.HasValue("HydrogenPeroxideResourceName"))
                {
                    _hydrogen_peroxide = plugin_settings.GetValue("HydrogenPeroxideResourceName");
                    Debug.Log("[KSP Interstellar] Hydrogen Peroxide resource name set to " + HydrogenPeroxide);
                }

                if (plugin_settings.HasValue("MethaneResourceName"))
                {
                    _methane = plugin_settings.GetValue("MethaneResourceName");
                    Debug.Log("[KSP Interstellar] Methane resource name set to " + Methane);
                }
                if (plugin_settings.HasValue("NeonResourceName"))
                {
                    _neon_gas = plugin_settings.GetValue("NeonResourceName");
                    Debug.Log("[KSP Interstellar] Neon resource name set to " + NeonGas);
                }
                if (plugin_settings.HasValue("NitrogenResourceName"))
                {
                    _nitrogen = plugin_settings.GetValue("NitrogenResourceName");
                    Debug.Log("[KSP Interstellar] Nitrogen resource name set to " + Nitrogen);
                }
                if (plugin_settings.HasValue("OxygenResourceName"))
                {
                    _oxygen = plugin_settings.GetValue("OxygenResourceName");
                    Debug.Log("[KSP Interstellar] Oxygen resource name set to " + Oxygen);
                }
                if (plugin_settings.HasValue("RegolithResourceName"))
                {
                    _regolith = plugin_settings.GetValue("RegolithResourceName");
                    Debug.Log("[KSP Interstellar] Regolith resource name set to " + Regolith);
                }
                if (plugin_settings.HasValue("XenonGasResourceName"))
                {
                    _xenongas = plugin_settings.GetValue("XenonGasResourceName");
                    Debug.Log("[KSP Interstellar] XenonGas resource name set to " + Regolith);
                }
                if (plugin_settings.HasValue("SolarWindResourceName"))
                {
                    _solarWind = plugin_settings.GetValue("SolarWindResourceName");
                    Debug.Log("[KSP Interstellar] SolarWind resource name set to " + SolarWind);
                }
                if (plugin_settings.HasValue("TritiumResourceName"))
                {
                    _tritium = plugin_settings.GetValue("TritiumResourceName");
                    Debug.Log("[KSP Interstellar] Tritium resource name set to " + LqdTritium);
                }
                if (plugin_settings.HasValue("UraniumTetraflourideName"))
                {
                    _uranium_TerraFloride = plugin_settings.GetValue("UraniumTetraflourideName");
                    Debug.Log("[KSP Interstellar] UraniumTetraflouride resource name set to " + _uranium_TerraFloride);
                }
                if (plugin_settings.HasValue("WaterResourceName"))
                {
                    _water = plugin_settings.GetValue("WaterResourceName");
                    Debug.Log("[KSP Interstellar] Water resource name set to " + Water);
                }
                if (plugin_settings.HasValue("HeavyWaterResourceName"))
                {
                    _heavyWater = plugin_settings.GetValue("HeavyWaterResourceName");
                    Debug.Log("[KSP Interstellar] Heavy Water resource name set to " + HeavyWater);
                }
            } 
            else
            {
                PluginHelper.showInstallationErrorMessage();
            }
        }

        public static InterstellarResourcesConfiguration Instance { get { return _instance ?? (_instance = new InterstellarResourcesConfiguration(PluginHelper.PluginSettingsConfig)); } }
    }
}
