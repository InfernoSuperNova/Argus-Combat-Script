using IngameScript.Classes;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public Vector3D TargetPosition = Vector3D.Zero;
        public double TargetDistance = 1800;

        public TacticalController tacticalController;
        IMyTerminalBlock reference;

        float kP = 40.0f;
        float kI = 0.1f;
        float kD = 15.0f;


        float thrustkP = 5.0f;
        float thrustkI = 0.1f;
        float thrustkD = 20.0f;


        const double FireAngleSigma = 0.9997;
        const double TimeStep = 1.0 / 60.0;

        float MaxAngular = 30.0f;

        public Program()
        {
            var allGyros = new List<IMyGyro>();
            var allThrusters = new List<IMyThrust>();
            var allGuns = new List<IMyUserControllableGun>();
            var allTurrets = new List<IMyLargeTurretBase>();
            reference = GridTerminalSystem.GetBlockWithName("Forward Reference");
            if (reference == null)
            {
                Echo("No forward reference found you bozo");
                return;
            }
            GridTerminalSystem.GetBlocksOfType(allGyros);
            GridTerminalSystem.GetBlocksOfType(allThrusters);
            GridTerminalSystem.GetBlocksOfType(allGuns);
            GridTerminalSystem.GetBlocksOfType(allTurrets);
            ClampedIntegralPID pitch = new ClampedIntegralPID(kP, kI, kD, TimeStep, -MaxAngular, MaxAngular);
            ClampedIntegralPID yaw = new ClampedIntegralPID(kP, kI, kD, TimeStep, -MaxAngular, MaxAngular);

            ClampedIntegralPID upDown = new ClampedIntegralPID(thrustkP, thrustkI, thrustkD, TimeStep, -MaxAngular, -MaxAngular);
            ClampedIntegralPID leftRight = new ClampedIntegralPID(thrustkP, thrustkI, thrustkD, TimeStep, -MaxAngular, -MaxAngular);
            ClampedIntegralPID forwardBackward = new ClampedIntegralPID(thrustkP, thrustkI, thrustkD, TimeStep, -MaxAngular, -MaxAngular);

            ShipControlInitializationData data = new ShipControlInitializationData();
            data.gyroscopes = allGyros;
            data.thrusters = allThrusters;
            data.guns = allGuns;
            data.reference = reference;
            data.pitch = pitch;
            data.yaw = yaw;
            data.forwardBackward = forwardBackward;
            data.upDown = upDown;
            data.leftRight = leftRight;
            data.maxAngular = MaxAngular;
            data.program = this;
            data.timeStep = TimeStep;
            data.fireAngleSigma = FireAngleSigma;


            tacticalController = new TacticalController(allTurrets, data, reference);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {


            tacticalController.Update();
        }
    }
}
