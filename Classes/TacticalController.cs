using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Data;
using VRageMath;
using IngameScript.Classes.Behaviors;
using Sandbox.Game.Entities.Cube;

namespace IngameScript.Classes
{

    //The brains of the ship, chooses what data to supply to the locomotion segment of the ship. 
    public class TacticalController
    {

        Behavior currentBehavior;
        ShipControl shipControl;
        Turrets turrets;
        MyGridProgram program;
        IMyTerminalBlock reference;
        public TacticalController(List<IMyLargeTurretBase> turrets, ShipControlInitializationData shipControlInitializationData, IMyTerminalBlock reference) {
            program = shipControlInitializationData.program;
            this.reference = reference;
            this.turrets = new Turrets(turrets);
            currentBehavior = new SpinToWin(program);
            shipControl = new ShipControl(shipControlInitializationData);
        }

        public void Update()
        {


            TurretReturnData target = turrets.GetTarget(TargetingType.CenterOfMass);

            if (target.position != Vector3D.Zero)
            {
                currentBehavior = new SpinToWin(program);
            }
            else
            {
                currentBehavior = new Behavior();
            }
            var updateData = currentBehavior.UpdateData;
            updateData.aimPositionTargetPos = target.position;
            updateData.selfVelocity = reference.CubeGrid.LinearVelocity;
            updateData.targetVelocity = Vector3D.Zero;
            updateData.projectileVelocity = 2000;

            shipControl.Update(currentBehavior.Flags, updateData);
        }
    }
}
