﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FNPlugin.Extensions;
using FNPlugin.Redist;
using FNPlugin.Constants;
using FNPlugin.Resources;

namespace FNPlugin.Beamedpower
{
    class BeamEffect
    {
        public GameObject solar_effect;
        public Renderer solar_effect_renderer;
        public Collider solar_effect_collider;
    }

    class ReceivedBeamedPower
    {
        public double receivedPower;
        public double pitchAngle;
        public double spotsize;
    }

    class StarLight
    {
        public CelestialBody star;
        public Vector3d position; 
        public float luminocity;
        public double solarFlux;
        public bool hasLineOfSight; 
    }

    class ModulePhotonSail : PartModule, IBeamedPowerReceiver
    {
        // Persistent Variables
        [KSPField(isPersistant = true)]
        public bool IsEnabled = false;
        [KSPField(isPersistant = true)]
        public double previousPeA;
        [KSPField(isPersistant = true)]
        public double previousAeA;
        [KSPField(isPersistant = true)]
        public float previousFixedDeltaTime;

        // Persistent False
        [KSPField]
        public double reflectedPhotonRatio = 0.975;
        [KSPField(guiActiveEditor = true, guiName = "Photovoltaic film Area", guiUnits = " m\xB2")]
        public double photovoltaicArea = 1;
        [KSPField(guiActiveEditor = true, guiName = "Sail Surface Area", guiUnits = " m\xB2", guiFormat = "F0")]
        public double surfaceArea = 144400;
        [KSPField(guiActiveEditor = true, guiActive = true, guiName = "Sail Diameter", guiUnits = " m", guiFormat = "F3")]
        public double diameter;
        [KSPField(guiActiveEditor = true, guiName = "Mass", guiUnits = " t", guiFormat = "F3")]
        public double partMass;
        [KSPField(guiActiveEditor = true, guiName = "Min wavelength", guiUnits = " m")]
        public double minimumWavelength = 0.000000620;
        [KSPField(guiActiveEditor = true, guiName = "Max wavelength", guiUnits = " m")]
        public double maximumWavelength = 0.01;
        [KSPField(guiActiveEditor = false, guiActive = false, guiName = "Max Laser Consumption", guiUnits = " W", guiFormat = "F0")]
        public double maxLaserPowerInWatt;
        [KSPField(guiActiveEditor = true, guiActive = true, guiName = "Max Laser Consumption", guiUnits = " GW", guiFormat = "F3")]
        public double maxLaserPowerInGigaWatt;
        [KSPField(guiActiveEditor = true, guiName = "Geeforce Tolerance", guiUnits = " G", guiFormat = "F0")]
        public double gTolerance;

        [KSPField]
        public double heatMultiplier = 10;
        [KSPField]
        public float effectSize1 = 1.25f;
        [KSPField]
        public string animName = "";
        [KSPField]
        public float initialAnimationSpeed = 1;
        [KSPField]
        public float initialAnimationNormalizedTime = 0.5f;
        [KSPField]
        public float initialAnimationTargetWeight = 0.01f;

        [KSPField]
        public string kscLaserTech1 = "specializedScienceTech";
        [KSPField]
        public string kscLaserTech2 = "longTermScienceTech";
        [KSPField]
        public string kscLaserTech3 = "scientificOutposts";
        [KSPField]
        public string kscLaserTech4 = "highEnergyScience";
        [KSPField]
        public string kscLaserTech5 = "appliedHighEnergyPhysics";
        [KSPField]
        public string kscLaserTech6 = "ultraHighEnergyPhysics";

        [KSPField(guiActiveEditor = true, guiActive = false, guiName = "Max Available KCS Power", guiUnits = " GW", guiFormat = "F3")]
        public double kscLaserPowerInGigaWatt = 1000;
        [KSPField(guiActiveEditor = false, guiActive = false, guiName = "Max Available KCS Power", guiUnits = " W", guiFormat = "F0")]
        public double kscLaserPowerInWatt = 2e12;
        [KSPField]
        public double kscLaserLatitude = -0.13133150339126601;
        [KSPField]
        public double kscLaserLongitude = -74.594841003417997;
        [KSPField]
        public double kscLaserAltitude = 75;
        [KSPField]
        public double kscLaserAperture = 50;
        [KSPField]
        public double kscLaserAbsorbtion = 0.0000342; // @ 11 micrometer with unprotected coating   https://www.thorlabs.com/newgrouppage9.cfm?objectgroup_id=744
        [KSPField]
        public double kscLaserWavelength = 0.000011; // 11 micrometer

        // GUI
        [KSPField(guiActive = true, guiName = "Skin Temperature", guiFormat = "F4", guiUnits = " K°")]
        public double skinTemperature;
        [KSPField(guiActive = true, guiName = "#autoLOC_6001421", guiFormat = "F4", guiUnits = " EC/s")]
        public double photovoltalicFlowRate;
        [KSPField(guiActive = false, guiName = "Skin Dissipation", guiFormat = "F4", guiUnits = " MJ")]
        public double dissipationInMegaJoule;
        [KSPField(guiActive = true, guiName = "Solar Flux", guiFormat = "F4", guiUnits = " W/m\xB2")]
        public double totalSolarFluxInWatt;
        [KSPField(guiActive = true, guiName = "Solar Force Max", guiFormat = "F4", guiUnits = " N")]
        public double totalForceInNewtonFromSolarEnergy = 0;
        [KSPField(guiActive = true, guiName = "Solar Force Sail", guiFormat = "F4", guiUnits = " N")]
        public double solar_force_d = 0;
        [KSPField(guiActive = true, guiName = "Solar Acceleration")]
        public string solarAcc;
        [KSPField(guiActive = true, guiName = "Solar Pitch Angle", guiFormat = "F3", guiUnits = "°")]
        public double solarSailAngle = 0;
        [KSPField(guiActive = false, guiName = "Network power", guiFormat = "F4", guiUnits = " MW")]
        public double maxNetworkPower;

        [KSPField(isPersistant = true, guiActive = true, guiName = "KCS Power Throttle", guiUnits = "%"), UI_FloatRange(stepIncrement = 1, maxValue = 100, minValue = 0, requireFullControl = false)]
        public float kcsBeamedPowerThrottle = 0;
        [KSPField(isPersistant = true, guiActive = true, guiName = "Beamed Power Throttle", guiUnits = "%"), UI_FloatRange(stepIncrement = 1, maxValue = 100, minValue = 0, requireFullControl = false)]
        public float beamedPowerThrottle = 0;
        [KSPField(isPersistant = true, guiActive = true, guiName = "Beamed Push Direction"), UI_Toggle(disabledText = "Backward", enabledText = "Forward", requireFullControl = false)]
        public bool beamedPowerForwardDirection = true;
        [KSPField(guiActive = true, guiName = "Beamed Energy", guiFormat = "F4", guiUnits = " MW")]
        public double totalReceivedBeamedPower;
        [KSPField(guiActive = true, guiName = "Beamed Connections")]
        public int connectedTransmittersCount;
        [KSPField(guiActive = true, guiName = "Beamed Potential Force", guiFormat = "F4", guiUnits = " N")]
        public double totalForceInNewtonFromBeamedPower = 0;
        [KSPField(guiActive = true, guiName = "Beamed Pitch Angle", guiFormat = "F3", guiUnits = "°")]
        public double weightedBeamPowerPitch;
        [KSPField(guiActive = true, guiName = "Beamed Spotsize", guiFormat = "F3", guiUnits = " m")]
        public double weightedBeamedPowerSpotsize;
        [KSPField(guiActive = true, guiName = "Beamed Sail Force", guiFormat = "F3", guiUnits = " N")]
        public double beamedSailForce = 0;
        [KSPField(guiActive = true, guiName = "Beamed Acceleration")]
        public string beamedAcc;
        [KSPField(guiActive = true, guiName = "Beamed Power Wasteheat", guiFormat = "F3", guiUnits = " MJ")]
        public double beamPowerWasteheatInMegaJoule;

        [KSPField(guiActive = false, guiName = "Atmospheric Density", guiUnits = " kg/m2")]
        public double atmosphericGasKgPerSquareMeter;
        [KSPField(guiActive = true, guiName = "Maximum Drag", guiUnits = " N/m2")]
        public float maximumDragPerSquareMeter;
        [KSPField(guiActive = true, guiName = "Drag Coefficient", guiFormat = "F3")]
        public double weightedDragCoefficient;
        [KSPField(guiActive = true, guiName = "Drag Wasteheat", guiFormat = "F3", guiUnits = " MJ")]
        public double dragHeatInMegajoule;
        [KSPField(guiActive = true, guiName = "Diffuse Drag", guiUnits = " N")]
        public float diffuseSailDragInNewton;
        [KSPField(guiActive = true, guiName = "Specular Drag", guiUnits = " N")]
        public float specularSailDragInNewton;
        [KSPField(guiActive = true, guiName = "Abs Periapsis Change", guiFormat = "F3", guiUnits = " m/s")]
        public double periapsisChange;
        [KSPField(guiActive = true, guiName = "Abs Apapsis Change", guiFormat = "F3", guiUnits = " m/s")]
        public double apapsisChange;
        [KSPField(guiActive = true, guiName = "Orbit Diameter Change", guiFormat = "F3", guiUnits = " m/s")]
        public double orbitSizeChange;
        [KSPField(guiActive = false, guiName = "Can See KCS")]
        public bool hasLineOfSightToKtc;
        [KSPField(guiActive = false, guiName = "Beamed Energy from TCS", guiFormat = "F0", guiUnits = " W")]
        public double availableBeamedPowerFromKtc;

        //[KSPField(isPersistant = true, guiActive = true, guiName = "Global Acceleration", guiUnits = "m/s"), UI_FloatRange(stepIncrement = 1, maxValue = 100, minValue = -100)]
        //public float globalAcceleration = 0;
        //[KSPField(isPersistant = true, guiActive = true, guiName = "Global Angle", guiUnits = "m/s"), UI_FloatRange(stepIncrement = 1, maxValue = 180, minValue = 0)]
        //public float globalAngle = 45;
        //[KSPField(isPersistant = true, guiActive = true, guiName = "Skin Heating", guiUnits = "m/s"), UI_FloatRange(stepIncrement = 1, maxValue = 100, minValue = -100)]
        //public float skinHeating = 0;

        double solar_acc_d = 0;
        double beamed_acc_d = 0;
        double sailSurfaceModifier;
        int updateCounter;

        Animation solarSailAnim = null;
        List<ReceivedBeamedPower> receivedBeamedPowerList = new List<ReceivedBeamedPower>();
        IDictionary<VesselMicrowavePersistence, KeyValuePair<MicrowaveRoute, IList<VesselRelayPersistence>>> _transmitDataCollection;
        Dictionary<Vessel, ReceivedPowerData> received_power = new Dictionary<Vessel, ReceivedPowerData>();
        BeamEffect[] beamEffectArray;

        Queue<double> periapsisChangeQueue = new Queue<double>(30);
        Queue<double> apapsisChangeQueue = new Queue<double>(30);

        static List<StarLight> starLights = new List<StarLight>();

        public int ReceiverType { get { return 7; } }                       // receiver from either top or bottom

        public double Diameter { get { return diameter; } }

        public double ApertureMultiplier { get { return 1; } }

        public double MinimumWavelength { get { return minimumWavelength; } }     // receive optimally from red visible light

        public double MaximumWavelength { get { return maximumWavelength; } }           // receive up to maximum infrared

        public double HighSpeedAtmosphereFactor { get { return 1; } }

        public double FacingThreshold { get { return 0; } }

        public double FacingSurfaceExponent { get { return 1; } }

        public double FacingEfficiencyExponent { get { return 0; } }    // can receive beamed power from any angle

        public double SpotsizeNormalizationExponent { get { return 1; } }

        public bool CanBeActiveInAtmosphere { get { return false; } }

        public Vessel Vessel { get { return vessel; } }

        public Part Part { get { return part; } }

        // GUI to deploy sail
        [KSPEvent(guiActiveEditor = true,  guiActive = true, guiName = "Deploy Sail", active = true, guiActiveUncommand = true, guiActiveUnfocused = true)]
        public void DeploySail()
        {
            //scaleBeamToAnimation = true;
            runAnimation(animName, solarSailAnim, 0.5f, 0);
            IsEnabled = true;
        }

        // GUI to retract sail
        [KSPEvent(guiActiveEditor = true, guiActive = true, guiName = "Retract Sail", active = false, guiActiveUncommand = true, guiActiveUnfocused = true)]
        public void RetractSail()
        {
            //scaleBeamToAnimation = true;
            runAnimation(animName, solarSailAnim, -0.5f, 1);
            IsEnabled = false;
        }

        // Initialization
        public override void OnStart(PartModule.StartState state)
        {
            gTolerance = part.gTolerance;

            diameter = Math.Sqrt(surfaceArea);

            maxLaserPowerInWatt = GetBlackBodyDissipation(surfaceArea, part.skinMaxTemp - 300) / kscLaserAbsorbtion;
            maxLaserPowerInGigaWatt = maxLaserPowerInWatt * 1e-9;

            if (animName != null)
                solarSailAnim = part.FindModelAnimators(animName).FirstOrDefault();

            // start with deployed sail  when enabled
            if (IsEnabled)
            {
                solarSailAnim[animName].speed = initialAnimationSpeed;
                solarSailAnim[animName].normalizedTime = initialAnimationNormalizedTime;
                solarSailAnim.Blend(animName, initialAnimationTargetWeight);
            }

            DetermineKscLaserPower();

            kscLaserPowerInGigaWatt = kscLaserPowerInWatt * 1e-9;

            if (state == StartState.None || state == StartState.Editor)
                return;

            this.part.force_activate();

            CreateBeamArray();

            CompileStarData();
        }

        private void DetermineKscLaserPower()
        {
            if (ResearchAndDevelopment.Instance == null)
                return;

            kscLaserPowerInWatt = GetTechCost(kscLaserTech1) * 1e8;
            kscLaserPowerInWatt += GetTechCost(kscLaserTech2) * 1e8;
            kscLaserPowerInWatt += GetTechCost(kscLaserTech3) * 1e8;
            kscLaserPowerInWatt += GetTechCost(kscLaserTech4) * 1e8;
            kscLaserPowerInWatt += (GetTechCost(kscLaserTech5) + 700) * 1e8;
            kscLaserPowerInWatt += GetTechCost(kscLaserTech6) * 1e8;
        }

        // Scan the Kopernicus config nodes and extract luminosity values
        protected void CompileStarData()
        {
            // Only need to do this once 
            if (starLights.Count > 0)
                return;

            var celestrialBodies = FlightGlobals.Bodies.ToDictionary(m => m.name);

            ConfigNode[] nodeLevel1 = GameDatabase.Instance.GetConfigNodes("Kopernicus");

            if (nodeLevel1.Length > 0)
                Debug.Log("[KSPI] - Loading Kopernicus Configuration Data");
            else
                Debug.LogWarning("[KSPI] - Failed to find Kopernicus Configuration Data");

            for (int i = 0; i < nodeLevel1.Length; i++)
            {
                ConfigNode[] celestrialBodyNode = nodeLevel1[i].GetNodes("Body");

                Debug.Log("[KSPI] - Found " + celestrialBodyNode.Length + " celestrial bodies");

                for (int j = 0; j < celestrialBodyNode.Length; j++)
                {
                    string bodyName = celestrialBodyNode[j].GetValue("name");

                    bool usesSunTemplate = false;

                    ConfigNode sunNode = celestrialBodyNode[j].GetNode("Template");

                    if (sunNode != null)
                    {
                        string templateName = sunNode.GetValue("name");
                        usesSunTemplate = templateName == "Sun";
                        if (usesSunTemplate)
                            Debug.Log("[KSPI] -  Will use default Sun template for " + bodyName);
                    }

                    ConfigNode propertiesNode = celestrialBodyNode[j].GetNode("Properties");

                    float luminocity = 0;

                    if (propertiesNode != null)
                    {
                        string starLuminosityText = propertiesNode.GetValue("starLuminosity");

                        if (string.IsNullOrEmpty(starLuminosityText))
                        {
                            if (usesSunTemplate)
                                Debug.LogWarning("[KSPI] - starLuminosity in Properties ConfigNode is missing, defaulting to template");
                        }
                        else
                        {
                            float.TryParse(starLuminosityText, out luminocity);
                            CelestialBody celestialBody;

                            if (luminocity > 0 && celestrialBodies.TryGetValue(bodyName, out celestialBody))
                            {
                                Debug.Log("[KSPI] - Added Star " + celestialBody.name + " with luminocity " + luminocity);
                                starLights.Add(new StarLight() { star = celestialBody, luminocity = luminocity });
                            }
                            else
                                Debug.LogWarning("[KSPI] - Failed to initialize star " + bodyName);
                        }
                    }

                    if (usesSunTemplate && luminocity == 0)
                    {
                        CelestialBody celestialBody;

                        if (celestrialBodies.TryGetValue(bodyName, out celestialBody))
                        {
                            Debug.Log("[KSPI] - Added Star " + celestialBody.name + " with default luminocity of 1");
                            starLights.Add(new StarLight() { star = celestialBody, luminocity = 1 });
                        }
                        else
                            Debug.LogWarning("[KSPI] - Failed to initialize star " + bodyName + " as with a default luminocity of 1");
                    }
                }
            }

            // add local sun if kopernicus configuration was not found or did not contain any star
            var homePlanetSun = Planetarium.fetch.Sun;
            if (!starLights.Any(m => m.star.name == homePlanetSun.name))
            {
                Debug.LogWarning("[KSPI] - homeplanet star was not found, adding homeplanet star as default sun");
                starLights.Add(new StarLight() { star = Planetarium.fetch.Sun, luminocity = 1 });
            }
        }

        private void CreateBeamArray()
        {
            beamEffectArray = new BeamEffect[10];

            for (var i = 0; i < beamEffectArray.Length; i++)
            {
                beamEffectArray[i] = CreateBeam(1001 + i);
            }
        }

        private BeamEffect CreateBeam(int renderQueue)
        {
            var beam = new BeamEffect();

            beam.solar_effect = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.solar_effect.transform.localScale = Vector3.zero;
            beam.solar_effect.transform.position = Vector3.zero;
            beam.solar_effect.transform.rotation = part.transform.rotation;
            
            beam.solar_effect_collider = beam.solar_effect.GetComponent<Collider>();
            beam.solar_effect_collider.enabled = false;

            beam.solar_effect_renderer = beam.solar_effect.GetComponent<Renderer>();
            beam.solar_effect_renderer.material.shader = Shader.Find("Unlit/Transparent");
            beam.solar_effect_renderer.material.color = new Color(Color.white.r, Color.white.g, Color.white.b, 0.1f);
            beam.solar_effect_renderer.material.mainTexture = GameDatabase.Instance.GetTexture("WarpPlugin/ParticleFX/infrared", false);
            beam.solar_effect_renderer.material.renderQueue = renderQueue;
            beam.solar_effect_renderer.receiveShadows = false;            

            return beam;
        }

        public void Update()
        {
            // Sail deployment GUI
            Events["DeploySail"].active = !IsEnabled;
            Events["RetractSail"].active = IsEnabled;

            if (HighLogic.LoadedSceneIsFlight)
                return;

            diameter = Math.Sqrt(surfaceArea);
            partMass = part.mass;

            maxLaserPowerInWatt = GetBlackBodyDissipation(surfaceArea, part.skinMaxTemp - 300) / kscLaserAbsorbtion;
            maxLaserPowerInGigaWatt = maxLaserPowerInWatt * 1e-9;
        }

        /// <summary>
        /// Is called by KSP while the part is active
        /// </summary>
        public override void OnUpdate()
        {
            updateCounter++;
            maxNetworkPower = 0;

            maxLaserPowerInWatt = GetBlackBodyDissipation(surfaceArea, part.skinMaxTemp - FlightGlobals.getExternalTemperature(part.transform.position)) / kscLaserAbsorbtion;
            maxLaserPowerInGigaWatt = maxLaserPowerInWatt * 1e-9;

            var animationNormalizedTime = solarSailAnim[animName].normalizedTime;
            sailSurfaceModifier = Math.Pow(animationNormalizedTime > 0 ? (animationNormalizedTime > 0.54 ? (animationNormalizedTime - 0.54) * (1 / 0.46) : 0) : 1, 2);

            // update available beamed power transmitters
            _transmitDataCollection = BeamedPowerHelper.GetConnectedTransmitters(this);

            foreach (var transmitData in _transmitDataCollection)
            {
                VesselMicrowavePersistence transmitter = transmitData.Key;
                KeyValuePair<MicrowaveRoute, IList<VesselRelayPersistence>> routeRelayData = transmitData.Value;

                ReceivedPowerData beamedPowerData;
                if (!received_power.TryGetValue(transmitter.Vessel, out beamedPowerData))
                {
                    beamedPowerData = new ReceivedPowerData
                    {
                        Receiver = this,
                        Transmitter = transmitter
                    };
                    received_power[beamedPowerData.Transmitter.Vessel] = beamedPowerData;
                }

                beamedPowerData.NetworkPower = 0;
                beamedPowerData.AvailablePower = 0;
                beamedPowerData.Route = routeRelayData.Key;
                beamedPowerData.Distance = beamedPowerData.Route.Distance;
                beamedPowerData.UpdateCounter = updateCounter;
 
                foreach(var wavelengthData in transmitter.SupportedTransmitWavelengths)
                {
                    var transmittedPower = (wavelengthData.nuclearPower + wavelengthData.solarPower) / 1000d;

                    maxNetworkPower += transmittedPower;

                    beamedPowerData.NetworkPower += transmittedPower;

                    var currentWavelengthBeamedPower = transmittedPower * beamedPowerData.Route.Efficiency;

                    beamedPowerData.AvailablePower += currentWavelengthBeamedPower; 
                }
            }

            // reset any non updated record
            foreach (var beamedPowerData in received_power.Values)
            {
                if (beamedPowerData.UpdateCounter != updateCounter)
                {
                    beamedPowerData.NetworkPower = 0;
                    beamedPowerData.AvailablePower = 0;
                }
            }

            var showBeamedPowerFields = IsEnabled && (connectedTransmittersCount > 0 || hasLineOfSightToKtc);
            Fields["totalReceivedBeamedPower"].guiActive = showBeamedPowerFields;
            Fields["connectedTransmittersCount"].guiActive = showBeamedPowerFields;
            Fields["totalForceInNewtonFromBeamedPower"].guiActive = showBeamedPowerFields;
            Fields["weightedBeamPowerPitch"].guiActive = showBeamedPowerFields;
            Fields["weightedBeamedPowerSpotsize"].guiActive = showBeamedPowerFields;
            Fields["beamedSailForce"].guiActive = showBeamedPowerFields;
            Fields["beamedAcc"].guiActive = showBeamedPowerFields;

            Fields["weightedDragCoefficient"].guiActive = maximumDragPerSquareMeter > 0;
            Fields["maximumDragPerSquareMeter"].guiActive = maximumDragPerSquareMeter > 0;
            Fields["dragHeatInMegajoule"].guiActive = maximumDragPerSquareMeter > 0;

            Fields["diffuseSailDragInNewton"].guiActive = diffuseSailDragInNewton > 0;
            Fields["specularSailDragInNewton"].guiActive = specularSailDragInNewton > 0;

            var relevantOrbitalData = 
                vessel.situation != global::Vessel.Situations.SPLASHED && 
                vessel.situation != global::Vessel.Situations.LANDED &&
                vessel.situation != global::Vessel.Situations.PRELAUNCH && 
                vessel.situation != global::Vessel.Situations.FLYING; 

            Fields["periapsisChange"].guiActive = relevantOrbitalData;
            Fields["apapsisChange"].guiActive = relevantOrbitalData;
            Fields["orbitSizeChange"].guiActive = relevantOrbitalData;

            // Text fields (acc & force)
            Fields["solarAcc"].guiActive = IsEnabled;

            solarAcc = solar_acc_d.ToString("E") + " m/s";
            beamedAcc = beamed_acc_d.ToString("E") + " m/s"; ;
        }

        public override void OnFixedUpdate()
        {
            availableBeamedPowerFromKtc = 0;
            totalReceivedBeamedPower = 0;
            photovoltalicFlowRate = 0;
            totalForceInNewtonFromSolarEnergy = 0;
            totalForceInNewtonFromBeamedPower = 0;
            solar_force_d = 0;
            solar_acc_d = 0;
            beamedSailForce = 0;
            beamed_acc_d = 0;

            if (FlightGlobals.fetch == null || part == null || vessel == null)
                return;

            receivedBeamedPowerList.Clear();

            skinTemperature = part.skinTemperature;

            UpdateChangeGui();

            ResetBeams();

            var vesselMassInKg = vessel.totalMass * 1000;
            var universalTime = Planetarium.GetUniversalTime();
            var positionVessel = vessel.orbit.getPositionAtUT(universalTime);
            var connectedTransmitters = received_power.Values.Where(m => m.AvailablePower > 0).OrderBy(m => m.AvailablePower).ToList();
            connectedTransmittersCount = connectedTransmitters.Count;

            var beamedPowerThrottleRatio = beamedPowerThrottle / 100;
            var rentedBeamedPowerThrottleRatio = kcsBeamedPowerThrottle / 100;

            double receivedBeamedPowerInWatt = 0;

            // proces ktc laser
            if (kscLaserPowerInWatt > 0)
            {
                Vector3d positionKscLaser = Planetarium.fetch.Home.GetWorldSurfacePosition(kscLaserLatitude, kscLaserLongitude, kscLaserAltitude);
                hasLineOfSightToKtc = LineOfSightToTransmitter(positionVessel, positionKscLaser);
                if (hasLineOfSightToKtc && rentedBeamedPowerThrottleRatio > 0)
                {
                    connectedTransmittersCount++;

                    // calculate spotsize and received power From Ktc
                    Vector3d powerSourceToVesselVector = positionVessel - positionKscLaser;
                    var cosConeAngle = Vector3d.Dot(powerSourceToVesselVector.normalized, this.part.transform.up);
                    if (cosConeAngle < 0)
                        cosConeAngle = Vector3d.Dot(powerSourceToVesselVector.normalized, -this.part.transform.up);
                    var spotSize = powerSourceToVesselVector.magnitude * kscLaserWavelength / kscLaserAperture;

                    var spotsizeRatio = Math.Min(1, cosConeAngle * diameter / spotSize);
                    availableBeamedPowerFromKtc = rentedBeamedPowerThrottleRatio * beamedPowerThrottleRatio * Math.Min(kscLaserPowerInWatt, Math.Max(0, maxLaserPowerInWatt - receivedBeamedPowerInWatt)) * Math.Pow(spotsizeRatio, 0.3 + (0.5 * (1 - spotsizeRatio)));

                    receivedBeamedPowerInWatt += GenerateForce(positionKscLaser, positionVessel, availableBeamedPowerFromKtc, universalTime, vesselMassInKg, 0, false, spotSize / 4, 9);
                }
            }

            // apply photon pressure from every potential laser source
            for (var beamcounter = 0; beamcounter < connectedTransmitters.Count; beamcounter++)
            {
                var receivedPowerData = connectedTransmitters[beamcounter];
                Vector3d beamedPowerSource = receivedPowerData.Transmitter.Vessel.GetWorldPos3D();
                var availableBeamedPowerFromTransmitter = beamedPowerThrottleRatio * Math.Min(receivedPowerData.AvailablePower * 1e+6, Math.Max(0, maxLaserPowerInWatt - receivedBeamedPowerInWatt));
                receivedBeamedPowerInWatt += GenerateForce(beamedPowerSource, positionVessel, availableBeamedPowerFromTransmitter, universalTime, vesselMassInKg, 0, false, receivedPowerData.Route.Spotsize / 4, beamcounter);
            }

            // process statistical data
            if (receivedBeamedPowerList.Count > 0)
            {
                totalReceivedBeamedPower = receivedBeamedPowerList.Sum(m => m.receivedPower);
                weightedBeamPowerPitch = receivedBeamedPowerList.Sum(m => m.pitchAngle * m.receivedPower / totalReceivedBeamedPower);
                weightedBeamedPowerSpotsize = receivedBeamedPowerList.Sum(m => m.spotsize * m.receivedPower / totalReceivedBeamedPower);
            }

            UpdateGeforceThreshold();

            // update solar flux
            UpdateSolarFlux(universalTime, positionVessel);

            // apply solarflux presure for every star
            foreach (var starLight in starLights)
            {
                GenerateForce(starLight.position, positionVessel, starLight.solarFlux * surfaceArea * sailSurfaceModifier, universalTime, vesselMassInKg, starLight.solarFlux);
            }

            // generate electric power
            part.RequestResource("ElectricCharge", -photovoltalicFlowRate * TimeWarp.fixedDeltaTime);

            // calculate drag
            ApplyDrag(universalTime, vesselMassInKg);

            // apply wasteheat
            ProcesWasteHeat();

            // apply solarsail effect to all vessels
            //foreach (var currentvessel in FlightGlobals.Vessels)
            //{
            //    var vesselNormal = currentvessel.GetOrbitDriver().orbit.GetWorldSpaceVel();
            //    var vectorToSun = vessel.GetWorldPos3D() -  _localStar.position;

            //    //var cosConeAngle = Vector3d.Dot(vectorToSun.normalized, vesselNormal);
            //    //var cosConeAngleIsNegative = cosConeAngle < 0;
            //    //if (cosConeAngleIsNegative)
            //    //    vesselNormal = -vesselNormal;

            //    float angleAwayFromSun = -(float)(globalAngle / radToDegreeMult);
            //    var desiredVesselHeading = Vector3d.RotateTowards(vesselNormal, vectorToSun, angleAwayFromSun, 1);
            //    var vesselDeceleration = desiredVesselHeading.normalized * globalAcceleration;
            //    ChangeVesselVelocity(currentvessel, universalTime, vesselDeceleration * TimeWarp.fixedDeltaTime);
            //}
        }

        private void UpdateGeforceThreshold()
        {
            if (connectedTransmittersCount > 0)
                TimeWarp.GThreshold = part.gTolerance;
            else
                TimeWarp.GThreshold = 2;
        }

        private void ProcesWasteHeat()
        {
            var effectiveDeltaTime = Math.Min(100, TimeWarp.fixedDeltaTime);

            // process heating
            beamPowerWasteheatInMegaJoule = kscLaserAbsorbtion * totalReceivedBeamedPower;
            var totalHeating = beamPowerWasteheatInMegaJoule + dragHeatInMegajoule;
            part.skinTemperature += totalHeating * effectiveDeltaTime / part.mass;

            // process dissipation
            var externalTemperature = FlightGlobals.getExternalTemperature(part.transform.position);
            var temperatureDelta = Math.Max(0, part.skinTemperature - externalTemperature);
            dissipationInMegaJoule = Math.Min(totalHeating, GetBlackBodyDissipation(surfaceArea, temperatureDelta) * 1e-6);
            part.skinTemperature = Math.Max(externalTemperature,  part.skinTemperature - dissipationInMegaJoule * effectiveDeltaTime / part.mass);
        }

        private static double GetBlackBodyDissipation(double surfaceArea, double temperatureDelta)
        {
            return surfaceArea * PhysicsGlobals.StefanBoltzmanConstant * Math.Pow(temperatureDelta, 4);
        }

        private void ResetBeams()
        {
            for (var i = 0; i < beamEffectArray.Length; i++)
            {
                UpdateVisibleBeam(beamEffectArray[i], Vector3d.zero, 0, 0);
            }
        }

        private void UpdateSolarFlux(double universalTime, Vector3d vesselPosition)
        {
            totalSolarFluxInWatt = 0;

            foreach(var starLight in starLights)
            {
                starLight.position = starLight.star.position;
                starLight.hasLineOfSight = LineOfSightToSun(vesselPosition, starLight.star);
                starLight.solarFlux = starLight.hasLineOfSight ? solarFluxAtDistance(part.vessel, starLight.star, starLight.luminocity) : 0;
                totalSolarFluxInWatt += starLight.solarFlux;
            }
        }

        private void ApplyDrag(double universalTime, double vesselMassInKg)
        {
            atmosphericGasKgPerSquareMeter = AtmosphericFloatCurves.GetAtmosphericGasDensityKgPerCubicMeter(vessel);

            var specularRatio = Math.Max(0, Math.Min(1, part.skinTemperature / part.skinMaxTemp));
            var diffuseRatio = 1 - specularRatio;
            var maximumDragCoefficient = 4 * specularRatio + 3.3 * diffuseRatio;
            var cosOrbitRaw = Vector3d.Dot(this.part.transform.up, part.vessel.obt_velocity.normalized);
            var cosObitalDrag = Math.Abs(cosOrbitRaw);
            var squaredCosOrbitalDrag = cosObitalDrag * cosObitalDrag;
            var siderealSpeed = 2 * vessel.mainBody.Radius * Math.PI / vessel.mainBody.rotationPeriod;
            var effectiveSurfaceArea = cosObitalDrag * sailSurfaceModifier * surfaceArea * (IsEnabled ? 1 : 0);

            var highAtmosphereModifier = Math.Pow(Math.Min(1, vessel.altitude / vessel.mainBody.atmosphereDepth), 3);
            var lowOrbitModifier = Math.Min(1, vessel.mainBody.atmosphereDepth / vessel.altitude);
            var highOrbitModifier = Math.Sqrt(1 - lowOrbitModifier);
            var effectiveSpeedForDrag = Math.Max(0, vessel.obt_speed - siderealSpeed * lowOrbitModifier);
            var dragForcePerSquareMeter = atmosphericGasKgPerSquareMeter * 0.5 * effectiveSpeedForDrag * effectiveSpeedForDrag;
            maximumDragPerSquareMeter = (float)(dragForcePerSquareMeter * maximumDragCoefficient);
            
            // apply specular Drag
            Vector3d partNormal = this.part.transform.up;
            if (cosOrbitRaw < 0)
                partNormal = -partNormal;

            var specularDragCoefficient = squaredCosOrbitalDrag + 3 * squaredCosOrbitalDrag * highOrbitModifier;
            var specularDragPerSquareMeter = specularDragCoefficient * dragForcePerSquareMeter * specularRatio;
            var specularDragInNewton = specularDragPerSquareMeter * effectiveSurfaceArea;
            specularSailDragInNewton = (float)specularDragInNewton;
            var specularDragForceFixed = specularDragInNewton * partNormal;
            var specularDragDeceleration = -specularDragForceFixed / vesselMassInKg;

            ChangeVesselVelocity(this.vessel, universalTime, highAtmosphereModifier * specularDragDeceleration * TimeWarp.fixedDeltaTime);

            // apply Diffuse Drag
            var diffuseDragCoefficient = 1 + highOrbitModifier + squaredCosOrbitalDrag * 1.3 * highOrbitModifier;
            var diffuseDragPerSquareMeter = diffuseDragCoefficient * dragForcePerSquareMeter * diffuseRatio;
            var diffuseDragInNewton = diffuseDragPerSquareMeter * effectiveSurfaceArea;
            diffuseSailDragInNewton = (float)diffuseDragInNewton;
            var diffuseDragForceFixed = diffuseDragInNewton * part.vessel.obt_velocity.normalized;
            var diffuseDragDeceleration = -diffuseDragForceFixed / vesselMassInKg;

            weightedDragCoefficient = specularDragCoefficient * specularRatio + diffuseDragCoefficient * diffuseRatio;

            ChangeVesselVelocity(this.vessel, universalTime, highAtmosphereModifier * diffuseDragDeceleration * TimeWarp.fixedDeltaTime);

            // increase temperature skin
            dragHeatInMegajoule = highAtmosphereModifier * dragForcePerSquareMeter * effectiveSurfaceArea * heatMultiplier * (0.1 + cosObitalDrag) / 1e3;
        }

        private static void ChangeVesselVelocity(Vessel vessel, double universalTime, Vector3d acceleration)
        {
            if (double.IsNaN(acceleration.x) || double.IsNaN(acceleration.y) || double.IsNaN(acceleration.z))
                return;

            if (double.IsInfinity(acceleration.x) || double.IsInfinity(acceleration.y) || double.IsInfinity(acceleration.z))
                return;

            if (vessel.packed)
                vessel.orbit.Perturb(acceleration, universalTime);
            else
                vessel.ChangeWorldVelocity(acceleration);
        }

        private double GenerateForce(Vector3d positionPowerSource, Vector3d positionVessel, double availableEnergyInWatt, double universalTime, double vesselMassInKg, double solarFlux, bool isSun = true, double beamspotsize = 1, int index = 0)
        {
            // calculate vector between vessel and star/transmitter
            Vector3d powerSourceToVesselVector = positionVessel - positionPowerSource;

            // take part vector 
            Vector3d partNormal = this.part.transform.up;

            // Magnitude of force proportional to cosine-squared of angle between sun-line and normal
            var cosConeAngle = Vector3d.Dot(powerSourceToVesselVector.normalized, partNormal);

            var cosConeAngleIsNegative = cosConeAngle < 0;

            // If normal points away from sun, negate so our force is always away from the sun
            // so that turning the backside towards the sun thrusts correctly
            if (cosConeAngleIsNegative)
            {
                // recalculate Magnitude of force proportional to cosine-squared of angle between sun-line and normal
                partNormal = -partNormal;
                cosConeAngle = Vector3d.Dot(powerSourceToVesselVector.normalized, partNormal);
            }

            // convert cosine angle  into angle in radian
            var pitchAngleInRad = Math.Acos(cosConeAngle);

            // convert radian into angle in degree
            var pitchAngleInDegree = pitchAngleInRad * Mathf.Rad2Deg;

            double maxPhotovotalicEnergy;

            if (isSun)
            {
                solarSailAngle = pitchAngleInDegree;
                maxPhotovotalicEnergy = photovoltaicArea * Math.Max(0, solarFlux - 1);
            }
            else
            {
                maxPhotovotalicEnergy = (photovoltaicArea / surfaceArea) * availableEnergyInWatt;
                // skip beamed power in undesireable direction
                if ((beamedPowerForwardDirection && cosConeAngleIsNegative) || (!beamedPowerForwardDirection && !cosConeAngleIsNegative))
                    return 0;
            }

            photovoltalicFlowRate += Math.Min(photovoltaicArea * cosConeAngle,  maxPhotovotalicEnergy * 0.2 * cosConeAngle * 0.001);

            // convert energy into momentum
            var radiationPresure = 2 * availableEnergyInWatt / GameConstants.speedOfLight;

            // calculate solar light force at current location
            var maximumPhotonForceInNewton = reflectedPhotonRatio * radiationPresure;

            // calculate effective radiation presure on solarsail
            var radiationPresureOnSail = isSun ? radiationPresure * cosConeAngle : radiationPresure;

            // register force 
            if (isSun)
                totalForceInNewtonFromSolarEnergy += maximumPhotonForceInNewton * sign(cosConeAngleIsNegative);
            else
                totalForceInNewtonFromBeamedPower += maximumPhotonForceInNewton * sign(cosConeAngleIsNegative);

            if (!IsEnabled)
                return 0;

            // draw beamed power rays
            if (!isSun && index < 10)
            {
                var scaleModifier = beamedPowerThrottle > 0 ? (beamspotsize * 4 < diameter ? 1 : 2) : 0;
                var beamSpotsize = beamedPowerThrottle > 0 ? (float)Math.Max((sailSurfaceModifier * cosConeAngle * diameter / 4), beamspotsize) : 0;
                UpdateVisibleBeam(beamEffectArray[index], powerSourceToVesselVector, scaleModifier, Math.Max(effectSize1, beamSpotsize));
            }

            // old : F = 2 PA cos α cos α n
            var effectiveForce = radiationPresureOnSail * reflectedPhotonRatio * cosConeAngle * partNormal;

            // calculate the vector at 90 degree angle in the direction of the vector
            //var tangantVector = (powerSourceToVesselVector - (Vector3.Dot(powerSourceToVesselVector, partNormal)) * partNormal).normalized;
            // new F = P A cos α [(1 + ρ ) cos α n − (1 − ρ ) sin α t] 
            // where P: solar radiation pressure, A: sail area, α: sail pitch angle, t: sail tangential vector, ρ: reflection coefficien
            //var effectiveForce = radiationPresureOnSail * ((1 + reflectedPhotonRatio) * cosConeAngle * partNormal - (1 - reflectedPhotonRatio) * Math.Sin(pitchAngleInRad) * tangantVector);

            // Calculate acceleration from sunlight
            Vector3d photonAccel = effectiveForce / vesselMassInKg;

            // Acceleration from sunlight per second
            Vector3d fixedPhotonAccel = photonAccel * TimeWarp.fixedDeltaTime;

            // Apply acceleration when valid
            ChangeVesselVelocity(this.vessel, universalTime, fixedPhotonAccel);

            // Update displayed force & acceleration
            if (isSun)
            {
                solar_force_d += effectiveForce.magnitude * sign(cosConeAngleIsNegative);
                solar_acc_d += photonAccel.magnitude * sign(cosConeAngleIsNegative);
            }
            else
            {
                var availableEnergyInMW = availableEnergyInWatt * 1e-6;

                receivedBeamedPowerList.Add(new ReceivedBeamedPower { pitchAngle = pitchAngleInDegree, receivedPower = availableEnergyInMW, spotsize = beamspotsize });

                beamedSailForce += effectiveForce.magnitude * sign(cosConeAngleIsNegative);
                beamed_acc_d += photonAccel.magnitude * sign(cosConeAngleIsNegative);
            }

            return availableEnergyInWatt;
        }

        private static int sign(bool cosConeAngleIsNegative)
        {
            return (cosConeAngleIsNegative ? -1 : 1);
        }

        private void UpdateVisibleBeam(BeamEffect beameffect, Vector3d powerSourceToVesselVector, double scaleModifer = 1, float beamSize = 1, double beamlength = 200000)
        {
            var normalizedPowerSourceToVesselVector = powerSourceToVesselVector.normalized;
            var endBeamPos = part.transform.position + normalizedPowerSourceToVesselVector * beamlength;
            var midPos = part.transform.position - endBeamPos;
            var timeCorrection = TimeWarp.CurrentRate > 1 ? -vessel.obt_velocity * TimeWarp.fixedDeltaTime : Vector3d.zero;

            var solarVectorX = normalizedPowerSourceToVesselVector.x * 90;
            var solarVectorY = normalizedPowerSourceToVesselVector.y * 90 - 90;
            var solarVectorZ = normalizedPowerSourceToVesselVector.z * 90;

            beameffect.solar_effect.transform.localRotation = new Quaternion((float)solarVectorX, (float)solarVectorY, (float)solarVectorZ, 0);
            beameffect.solar_effect.transform.localScale = new Vector3(beamSize, (float)(beamlength * scaleModifer), beamSize);
            beameffect.solar_effect.transform.position = new Vector3((float)(part.transform.position.x + midPos.x + timeCorrection.x), (float)(part.transform.position.y + midPos.y + timeCorrection.y), (float)(part.transform.position.z + midPos.z + timeCorrection.z));
        }

        private void UpdateChangeGui()
        {
            var averageFixedDeltaTime = (previousFixedDeltaTime + TimeWarp.fixedDeltaTime) / 2;

            periapsisChangeQueue.Enqueue((vessel.orbit.PeA - previousPeA) / averageFixedDeltaTime);
            if (periapsisChangeQueue.Count > 30)
                periapsisChangeQueue.Dequeue();
            periapsisChange = periapsisChangeQueue.Count > 20 
                ?  periapsisChangeQueue.OrderBy(m => m).Skip(5).Take(20).Average() 
                : periapsisChangeQueue.Average();

            apapsisChangeQueue.Enqueue((vessel.orbit.ApA - previousAeA) / averageFixedDeltaTime);
            if (apapsisChangeQueue.Count > 30)
                apapsisChangeQueue.Dequeue();
            apapsisChange = apapsisChangeQueue.Count > 20
                ? apapsisChangeQueue.OrderBy(m => m).Skip(5).Take(20).Average()
                : apapsisChangeQueue.Average();

            orbitSizeChange = periapsisChange + apapsisChange;

            previousPeA = vessel.orbit.PeA;
            previousAeA = vessel.orbit.ApA;
            previousFixedDeltaTime = TimeWarp.fixedDeltaTime;
        }

        private static double solarFluxAtDistance(Vessel vessel, CelestialBody star, double luminosity)
        {
            var toStar = vessel.CoMD - star.position;
            var distanceToSurfaceStar = toStar.magnitude - star.Radius;
            var nearStarDistance = star.Radius / 4 * Math.Pow(1 + Math.Min(1, distanceToSurfaceStar / star.Radius),2);
            var distanceForeffectiveDistance = Math.Max(distanceToSurfaceStar, nearStarDistance);
            var distAU = distanceForeffectiveDistance / Constants.GameConstants.kerbin_sun_distance;
            return luminosity * PhysicsGlobals.SolarLuminosityAtHome / (distAU * distAU);
        }

        private static void runAnimation(string animationName, Animation anim, float speed, float aTime)
        {
            if (anim == null || string.IsNullOrEmpty(animationName))
                return;

            anim[animationName].speed = speed;
            if (anim.IsPlaying(animationName))
                return;

            anim[animationName].wrapMode = WrapMode.Default;
            anim[animationName].normalizedTime = aTime;
            anim.Blend(animationName, 1);
        }

        public static bool LineOfSightToSun(Vector3d vesselPosition, CelestialBody star)
        {
            return LineOfSightToTransmitter(vesselPosition, star.position, star.name);
        }

        public static bool LineOfSightToTransmitter(Vector3d vesselPosition, Vector3d transmitterPosition, string ignoreBody = "")
        {
            Vector3d bminusa = transmitterPosition - vesselPosition;

            foreach (CelestialBody referenceBody in FlightGlobals.Bodies)
            {
                // the star should not block line of sight to the sun
                if (referenceBody.name == ignoreBody)
                    continue;

                Vector3d refminusa = referenceBody.position - vesselPosition;

                if (Vector3d.Dot(refminusa, bminusa) <= 0)
                    continue;

                var normalizedBminusa = bminusa.normalized;

                var cosReferenceSunNormB = Vector3d.Dot(refminusa, normalizedBminusa);

                if (cosReferenceSunNormB >= bminusa.magnitude)
                    continue;

                Vector3d tang = refminusa - cosReferenceSunNormB * normalizedBminusa;
                if (tang.magnitude < referenceBody.Radius)
                    return false;
            }
            return true;
        }

        public static int GetTechCost(string techid)
        {
            if (String.IsNullOrEmpty(techid) || techid == "none")
                return 0;

            var techstate = ResearchAndDevelopment.Instance.GetTechState(techid);
            if (techstate != null)
            {
                var available = techstate.state == RDTech.State.Available;

                if (available)
                    return techstate.scienceCost;
            }

            return 0;
        }


    }
}
