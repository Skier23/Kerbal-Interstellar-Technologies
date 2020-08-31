﻿using System;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

namespace FNPlugin.Wasteheat
{
    class FNPassiveThermalDissipation: PartModule
    {
        // configuration
        [KSPField(guiActive = true, guiName = "Surface Area", guiUnits = " m\xB2", guiFormat = "F3")]
        public double solarDissipationSurfaceArea = 0;
        [KSPField]
        public double solarDissipationEmissiveConstant = 0;

        [KSPField]
        public double thermalMassModifier;

        [KSPField] public double emissiveConstant = 0.02;

        // state
        [KSPField(isPersistant = true)]
        public double storedPartTemperature;
        [KSPField(isPersistant = true)]
        public double storedPartSkinTemperature;
        [KSPField(isPersistant = true)]
        public bool isInitialized;

        // GUI
        [KSPField(guiActive = true, guiName = "Heat Dissipation", guiUnits = " MW", guiFormat = "F3")]//Dissipation
        public double dissipationInMegaJoules;
        [KSPField(guiActive = true, guiName = "Heat Absorbtion", guiUnits = " MW", guiFormat = "F3")] //Absorbtion
        public double deltaEnergyIncreaseInMegajoules;

        [KSPField(guiActive = true, guiName = "Stock SolarFlux", guiFormat = "F1")]//Solar Flux
        public double stockSolarFlux;
        //[KSPField(guiActive = true, guiName = "Classic SolarFlux", guiFormat = "F4")]//Solar Flux
        //public double classicSolarFlux;
        [KSPField(guiActive = true, guiName = "Simulated SolarFlux", guiFormat = "F1")]//Solar Flux
        public double simulatedSolarFlux;
        //[KSPField(guiActive = true, guiName = "Delta SolarFlux ", guiFormat = "F1")]//Solar Flux
        //public double deltaSolarFlux;

        //[KSPField(guiActive = false, guiFormat = "F0", guiUnits = " m")]
        //public double calculatedDistanceToSun;
        [KSPField(guiActive = false, guiFormat = "F0", guiUnits = " m")]
        public double realDistanceToSun;
        [KSPField(guiActive = false, guiFormat = "F0", guiUnits = " m")]
        public double distanceFromStarCenterToVessel;

        //[KSPField(guiActive = false)]
        //public double fluxMultiplier;
        //[KSPField(guiActive = true)] 
        //public double distanceToVesselDelta;

        //[KSPField(guiActive = true)] 
        //public double omega;
        //[KSPField(guiActive = true)] 
        //public double theta;
        //[KSPField(guiActive = true, guiFormat = "F0")]
        //public double num;

        [KSPField(guiActive = true, guiName = "Cosine Factor", guiFormat = "F4")] 
        public double cosAngle;


        // session
        private int _countDown;
        private double _thermalMassPerKilogram;


        public override void OnStart(PartModule.StartState state)
        {
            if (state == StartState.Editor)
                return;

            _countDown = 50;
            part.thermalMassModifier = thermalMassModifier;
            

            if (isInitialized) return;

            storedPartTemperature = part.temperature;
            storedPartSkinTemperature = part.skinTemperature;

            isInitialized = true;
        }

        public void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsEditor) return;

            _thermalMassPerKilogram = part.mass * part.thermalMassModifier * PhysicsGlobals.StandardSpecificHeatCapacity * 1e-3;

            CalculateDistances();

            if (_countDown > 0)
            {
                part.temperature = storedPartTemperature;
                part.skinTemperature = storedPartSkinTemperature;
                _countDown--;
            }
            else
            {
                storedPartTemperature = part.temperature;
                storedPartSkinTemperature = part.skinTemperature;
            }

            if (!(solarDissipationSurfaceArea > 0) || !(solarDissipationEmissiveConstant > 0)) return;

            dissipationInMegaJoules = PluginHelper.GetBlackBodyDissipation(solarDissipationSurfaceArea * solarDissipationEmissiveConstant, System.Math.Max(0,  part.temperature - 4)) * 1e-6;
            var temperatureChange = TimeWarp.fixedDeltaTime * -(dissipationInMegaJoules / _thermalMassPerKilogram);
            part.temperature = Math.Max(4, part.temperature + temperatureChange);
        }

        private void CalculateDistances()
        {
            stockSolarFlux = vessel.solarFlux;

            //var toStar = vessel.CoMD - vessel.mainBody.position;
            //distanceFromStarCenterToVessel = toStar.magnitude;
            //if (distanceFromStarCenterToVessel <= 0)
            //    return;

            
            var astronomicalUnit = FlightGlobals.GetHomeBody().orbit.semiMajorAxis;

            if (FlightIntegrator.sunBody != null)
            {
                Vector3d scaledSpace = ScaledSpace.LocalToScaledSpace(vessel.transform.position);
                Vector3d position = FlightIntegrator.sunBody.scaledBody.transform.position;
                Vector3d vector3d = position - scaledSpace;
                var distance = vector3d.magnitude;
                distanceFromStarCenterToVessel = distance * ScaledSpace.ScaleFactor;
                cosAngle = Math.Min(1, Math.Max(0, Vector3d.Dot(vector3d.normalized, this.vessel.transform.up)));

                //// normalize vector using distance
                //var sunVector = (Vector3)(vector3d / distance);

                //// create a ray from vessel to sun
                //Ray ray = new Ray(scaledSpace, sunVector);

                //var sunLayerMask = 1 << LayerMask.NameToLayer("Scaled Scenery");
                //if (Physics.Raycast(ray, out var sunBodyFluxHit, float.MaxValue, sunLayerMask))
                //    calculatedDistanceToSun = ScaledSpace.ScaleFactor * sunBodyFluxHit.distance;
                //else
                //    calculatedDistanceToSun = distance * ScaledSpace.ScaleFactor - FlightIntegrator.sunBody.Radius;
            }

            if (FlightIntegrator.ActiveVesselFI != null)
            {
                realDistanceToSun = FlightIntegrator.ActiveVesselFI.realDistanceToSun;
                var starRadius = vessel.mainBody.Radius;

                //var theta = Math.Asin(starRadius / distanceFromStarCenterToVessel);
                //var omega = 2 * Math.PI * (1 - Math.Cos(theta));
                //var distanceInAu = calculatedDistanceToSun / astronomicalUnit;
                //simulatedSolarFlux = solarRadiance * omega;

                var surfaceAreaSun = 4 * Math.PI * starRadius * starRadius;
                var solarRadiance = 4 * astronomicalUnit * astronomicalUnit * PhysicsGlobals.SolarLuminosityAtHome / surfaceAreaSun;

                //classicSolarFlux = solarRadiance * Math.PI * Math.Pow(starRadius / realDistanceToSun, 2);
                simulatedSolarFlux = solarRadiance * Math.PI * Math.Pow(starRadius / distanceFromStarCenterToVessel, 2);

                //deltaSolarFlux = Math.Max(0, classicSolarFlux - simulatedSolarFlux);

                deltaEnergyIncreaseInMegajoules = cosAngle * simulatedSolarFlux * solarDissipationSurfaceArea * emissiveConstant * 1e-6;
                var deltaTemperatureChange = TimeWarp.fixedDeltaTime * (deltaEnergyIncreaseInMegajoules / _thermalMassPerKilogram);

                part.skinTemperature = Math.Max(4, part.skinTemperature + deltaTemperatureChange);
                //part.temperature = Math.Max(4, part.temperature + deltaTemperatureChange);
            }
        }
    }
}
