using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Data;
using VRageMath;

namespace IngameScript.Classes
{
    public enum TargetingType
    {
        CenterOfMass,
        TurretAverage,
        RandomTurret,
        MostTargetedBlock
    }

    public struct TurretReturnData
    {
        public Vector3D position;
        public Vector3D velocity;
    }
    public class Turrets
    {
        public List<IMyLargeTurretBase> turrets;

        public long LastTargetedEntity = 0;
        public Turrets(List<IMyLargeTurretBase> turrets)
        {
            this.turrets = turrets;
        }

        public TurretReturnData GetTarget(TargetingType type)
        {
            ValidateTurretIntegrity();
            switch (type)
            {
                case TargetingType.CenterOfMass:
                    return GetCenterOfMassTarget();
                case TargetingType.TurretAverage:
                    return GetTurretAverageTarget();
                case TargetingType.RandomTurret:
                    return GetRandomTurretTarget();
                //case TargetingType.MostTargetedBlock:  //////////   To be implemented
                //    return GetMostTargetedBlock();
                default:
                    TurretReturnData data3 = new TurretReturnData();
                    data3.position = Vector3D.Zero;
                    data3.velocity = Vector3D.Zero;
                    return data3;
            }
        }
        
        private void ValidateTurretIntegrity()
        {
            var newTurrets = new List<IMyLargeTurretBase>();
            foreach (var turret in turrets)
            {
                if (turret == null || turret.Closed) continue;
                newTurrets.Add(turret);
            }
            turrets = newTurrets;
        }
        private TurretReturnData GetCenterOfMassTarget()
        {
            foreach (var turret in turrets)
            {
                MyDetectedEntityInfo info = turret.GetTargetedEntity();
                if (turret.HasTarget && info.EntityId == LastTargetedEntity)
                {
                    TurretReturnData data = new TurretReturnData();
                    data.position = info.Position;
                    data.velocity = info.Velocity;
                    return data;
                }
            }
            //None of the turrets match the last targeted entity, so we find the first turret that has a target instead
            foreach (var turret in turrets)
            {
                if (turret.HasTarget)
                {
                    MyDetectedEntityInfo info = turret.GetTargetedEntity();
                    LastTargetedEntity = info.EntityId;

                    TurretReturnData data = new TurretReturnData();
                    data.position = info.Position;
                    data.velocity = info.Velocity;
                    return data;
                }
            }
            //None of the turrets have a target, so we return zero vec3's
            TurretReturnData data2 = new TurretReturnData();
            data2.position = Vector3D.Zero;
            data2.velocity = Vector3D.Zero;
            return data2;
        }

        private TurretReturnData GetTurretAverageTarget()
        {
            Vector3D averageTurretTarget = Vector3D.Zero;
            Vector3D velocity = Vector3D.Zero;
            int turretCount = 0;
            foreach (var turret in turrets)
            {
                if (turret.HasTarget)
                {
                    MyDetectedEntityInfo info = turret.GetTargetedEntity();
                    if (LastTargetedEntity == info.EntityId)
                    {
                        averageTurretTarget += (Vector3D)info.HitPosition;
                        velocity = info.Velocity;
                        turretCount++;
                    }
                }
            }
            if (turretCount != 0)
            {
                TurretReturnData data = new TurretReturnData();
                data.position = averageTurretTarget / turretCount;
                data.velocity = velocity;
                return data;
            }
            //Need to get a new target if we reach this point
            SetMostTargetedEntity();

            foreach (var turret in turrets)
            {
                if (turret.HasTarget)
                {
                    MyDetectedEntityInfo info = turret.GetTargetedEntity();
                    if (LastTargetedEntity == info.EntityId)
                    {
                        averageTurretTarget += (Vector3D)info.HitPosition;
                        velocity = info.Velocity;
                        turretCount++;
                    }
                }
            }
            if (turretCount != 0)
            {
                TurretReturnData data = new TurretReturnData();
                data.position = averageTurretTarget / turretCount;
                data.velocity = velocity;
                return data;
            }
            //None of the turrets have a target, so we return zero vec3's
            TurretReturnData data2 = new TurretReturnData();
            data2.position = Vector3D.Zero;
            data2.velocity = Vector3D.Zero;
            return data2;

        }


        private void SetMostTargetedEntity()
        {
            Dictionary<long, int> targetCount = new Dictionary<long, int>();
            foreach (var turret in turrets)
            {
                if (turret.HasTarget)
                {
                    MyDetectedEntityInfo info = turret.GetTargetedEntity();
                    if (!targetCount.ContainsKey(info.EntityId))
                    {
                        targetCount[info.EntityId] = 1;
                    }
                    else
                    {
                        targetCount[info.EntityId]++;
                    }
                }
            }
            long mostTargeted = 0;
            int mostTargetedCount = 0;
            foreach (var pair in  targetCount)
            {
                if (pair.Value > mostTargetedCount)
                {
                    mostTargeted = pair.Key;
                    mostTargetedCount = pair.Value;
                }
            }
            LastTargetedEntity = mostTargeted;
        }

        private TurretReturnData GetRandomTurretTarget()
        {
            foreach (var turret in turrets)
            {
                if (turret.HasTarget)
                {
                    MyDetectedEntityInfo info = turret.GetTargetedEntity();
                    if (LastTargetedEntity == info.EntityId)
                    {
                        TurretReturnData data = new TurretReturnData();
                        data.position = info.Position;
                        data.velocity = info.Velocity;
                        return data;
                    }
                }
            }
            foreach (var turret in turrets)
            {
                if (turret.HasTarget)
                {
                    MyDetectedEntityInfo info = turret.GetTargetedEntity();
                    LastTargetedEntity = info.EntityId;
                    TurretReturnData data = new TurretReturnData();
                    data.position = info.Position;
                    data.velocity = info.Velocity;
                    return data;
                }
            }
            TurretReturnData data2 = new TurretReturnData();
            data2.position = Vector3D.Zero;
            data2.velocity = Vector3D.Zero;
            return data2;
        }
    }
}
