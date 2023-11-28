using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRageMath;


namespace IngameScript.Classes
{
    public class Gyroscopes
    {
        private IMyTerminalBlock Reference;
        private double MaxAngular;
        private List<IMyGyro> gyroscopes;
        private ClampedIntegralPID Pitch;
        private ClampedIntegralPID Yaw;



        public Gyroscopes(IMyTerminalBlock Reference, List<IMyGyro> gyroscopes, ClampedIntegralPID Pitch, ClampedIntegralPID Yaw, double MaxAngular)
        {
            this.Reference = Reference;
            this.gyroscopes = gyroscopes;
            this.Pitch = Pitch;
            this.Yaw = Yaw;
            this.MaxAngular = MaxAngular;
        }

        public void FaceShipTowards(Vector3D desiredGlobalFwdNormalized, double roll)
        {
            double gp;
            double gy;
            double gr = roll;
            //FaceShipTowards Toward forward
            if (Reference.WorldMatrix.Forward.Dot(desiredGlobalFwdNormalized) < 1)
            {
                var waxis = Vector3D.Cross(Reference.WorldMatrix.Forward, desiredGlobalFwdNormalized);
                Vector3D axis = Vector3D.TransformNormal(waxis, MatrixD.Transpose(Reference.WorldMatrix));
                gp = (float)MathHelper.Clamp(Pitch.Control(-axis.X), -MaxAngular, MaxAngular);
                gy = (float)MathHelper.Clamp(Yaw.Control(-axis.Y), -MaxAngular, MaxAngular);
            }
            else
            {
                gp = 0.0;
                gy = 0.0;
            }
            if (Math.Abs(gy) + Math.Abs(gp) > MaxAngular)
            {
                double adjust = MaxAngular / (Math.Abs(gy) + Math.Abs(gp));
                gy *= adjust;
                gp *= adjust;
            }
            const double sigma = 0.0009;
            if (Math.Abs(gp) < sigma) gp = 0;
            if (Math.Abs(gy) < sigma) gy = 0;
            //if (Math.Abs(gr) < sigma * 1000) gr = 0;
            ApplyGyroOverride(gp, gy, gr, gyroscopes, Reference.WorldMatrix);
        }

        private void ApplyGyroOverride(double pitchSpeed, double yawSpeed, double rollSpeed, List<IMyGyro> gyroList, MatrixD worldMatrix)
        {
            var rotationVec = new Vector3D(pitchSpeed, yawSpeed, rollSpeed);
            var relativeRotationVec = Vector3D.TransformNormal(rotationVec, worldMatrix);

            foreach (var thisGyro in gyroList)
            {
                var transformedRotationVec = Vector3D.TransformNormal(relativeRotationVec, Matrix.Transpose(thisGyro.WorldMatrix));

                thisGyro.Pitch = (float)transformedRotationVec.X;
                thisGyro.Yaw = (float)transformedRotationVec.Y;
                thisGyro.Roll = (float)transformedRotationVec.Z;
                thisGyro.GyroOverride = true;
            }
        }

        public void ApplyGyroPitch(float speed)
        {
            var rotationVec = new Vector3D(speed, 0, 0);
            var relativeRotationVec = Vector3D.TransformNormal(rotationVec, Reference.WorldMatrix);
            foreach (var gyro in gyroscopes)
            {
                ApplyToIndividualGyro(relativeRotationVec, gyro);

            }
        }

        public void ApplyGyroYaw(float speed)
        {
            var rotationVec = new Vector3D(0, speed, 0);
            var relativeRotationVec = Vector3D.TransformNormal(rotationVec, Reference.WorldMatrix);
            foreach (var gyro in gyroscopes)
            {
                ApplyToIndividualGyro(relativeRotationVec, gyro);

            }
        }

        public void ApplyGyroRoll(float speed)
        {
            var rotationVec = new Vector3D(0, 0, speed);
            var relativeRotationVec = Vector3D.TransformNormal(rotationVec, Reference.WorldMatrix);
            foreach (var gyro in gyroscopes)
            {
                ApplyToIndividualGyro(relativeRotationVec, gyro);
            }
        }

        private void ApplyToIndividualGyro(Vector3D relativeRotationVec, IMyGyro gyro)
        {
            var transformedRotationVec = Vector3D.TransformNormal(relativeRotationVec, Matrix.Transpose(gyro.WorldMatrix));
            gyro.Pitch = (float)(transformedRotationVec.X < double.Epsilon ? transformedRotationVec.X : gyro.Pitch);
            gyro.Yaw = (float)(transformedRotationVec.Y < double.Epsilon ? transformedRotationVec.Y : gyro.Yaw);
            gyro.Roll = (float)(transformedRotationVec.Z < double.Epsilon ? transformedRotationVec.Z : gyro.Roll);
            gyro.GyroOverride = true;
        }

        public void DisableGyros()
        {
            foreach (var gyro in gyroscopes)
            {
                gyro.Pitch = 0;
                gyro.Yaw = 0;
                gyro.Roll = 0;
                gyro.GyroOverride = false;
            }
        }
    }
}
