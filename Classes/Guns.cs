using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ObjectBuilders.Components;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript.Classes
{

    public class Guns
    {
        static readonly MyDefinitionId ElectricityId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");
        const float IdlePowerDraw = 0.002f;

        private List<IMyUserControllableGun> guns;
        private Dictionary<IMyUserControllableGun, bool> availableGuns;
        private MyGridProgram program;
        public Guns(List<IMyUserControllableGun> guns, MyGridProgram program)
        {
            availableGuns = new Dictionary<IMyUserControllableGun, bool>();
            this.guns = guns;
            foreach (var gun in guns)
            {
                availableGuns[gun] = false;
            }
            this.program = program;
        }


        public bool AreAvailable()
        {
            bool IsGunAvailable = false;
            List<IMyUserControllableGun> tempGuns = new List<IMyUserControllableGun>();
            foreach (var gun in guns)
            {
                if (gun != null)
                {
                    tempGuns.Add(gun);
                }
            }

            foreach (var gun in tempGuns)
            {
                bool IsFunctional = gun.IsFunctional;
                bool IsReadyToFire = gun.Components.Get<MyResourceSinkComponent>().MaxRequiredInputByType(ElectricityId) < (IdlePowerDraw + float.Epsilon);

                IsGunAvailable = IsFunctional && IsReadyToFire;
                availableGuns[gun] = IsGunAvailable;
            }
            return IsGunAvailable;
        }

        public Vector3D GetAimingReferencePos()
        {
            Vector3D averagePos = Vector3D.Zero;
            int activeGunCount = 0;
            foreach (var gun in guns)
            {
                if (availableGuns[gun])
                {
                    Vector3D GunPos = gun.GetPosition();
                    if (double.IsNaN(GunPos.X)) continue;
                    averagePos += gun.GetPosition();
                    activeGunCount++;
                }
            }

            if (activeGunCount == 0) return averagePos;

            return averagePos / activeGunCount;
        }

        public void Fire()
        {
            foreach (var gun in guns)
            {
                if (availableGuns[gun])
                {
                    gun.Enabled = true;
                    gun.Shoot = true;
                }
            }
        }
        public void Cancel()
        {
            foreach (var gun in guns)
            {
                if (availableGuns[gun])
                {
                    gun.Shoot = false;
                    gun.Enabled = false;
                }
                else
                {
                    gun.Shoot = false;
                    gun.Enabled = true; //ensure that uncharged guns will still accumulate power
                }
            }
        }
    }
}
