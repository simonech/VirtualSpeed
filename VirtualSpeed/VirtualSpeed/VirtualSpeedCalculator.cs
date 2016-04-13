using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualSpeed
{
    
    public class VirtualSpeedCalculator
    {
        private const double G = 9.8067;

        public VirtualSpeedCalculator(): this(new Parameters())
        {

        }

        public VirtualSpeedCalculator(Parameters parameters)
        {
            Parameters = parameters;
        }

        public Parameters Parameters { get; set; }


        public double CalculatePower(double velocityKmh)
        {
            Forces forces = CalculateForces(velocityKmh);

            double wheelPower = forces.Total * (velocityKmh * 1000.0 / 3600.0);

            double legPower = wheelPower / (1.0 - (Parameters.DriveTrainLoss / 100.0));

            return legPower;
        }

        private Forces CalculateForces(double velocityKmh)
        {
            var forces = new Forces();
            var velocityMS = ConvertKmhToMS(velocityKmh);

            forces.Gravity = G * Parameters.WeightTotal * Math.Sin(Math.Atan(Parameters.ClimbGrade / 100));

            forces.Rolling = G * Parameters.WeightTotal * Math.Cos(Math.Atan(Parameters.ClimbGrade / 100)) * Parameters.Crr;

            forces.Drag = 0.5 * Parameters.GetCdA() * Parameters.Rho * velocityMS * velocityMS;


            return forces;
        }

        public double ConvertKmhToMS(double velocityKmh)
        {
            return velocityKmh * 1000 / 3600;
        }

        public double CalculateDistance(double speedKmh, double timeSec)
        {
            return ConvertKmhToMS(speedKmh) * timeSec;
        }

        public double CalculateVelocity(double power)
        {
            var epsilon = 0.000001;
            var lowervel = -1000.0;
            var uppervel = 1000.0;
            var midvel = 0.0;

            var midpow = CalculatePower(midvel);

            var itcount = 0;
            do
            {
                if (Math.Abs(midpow - power) < epsilon)
                    break;

                if (midpow > power)
                    uppervel = midvel;
                else
                    lowervel = midvel;

                midvel = (uppervel + lowervel) / 2.0;
                midpow = CalculatePower(midvel);
            } while (itcount++ < 100);

            return midvel;
        }
    }

    public class Parameters
    {
        public Parameters()
        {
            DriveTrainLoss = 3.0;
            WeightRider = 83;
            WeightBike = 8;
            ClimbGrade = 0;
            Crr = 0.005;
            A = 0.509;
            Cd = 0.63;
            Rho = 1.226;
            CdA = 0;
        }

        public double DriveTrainLoss { get; set; }
        public double WeightRider { get; set; }
        public double WeightBike { get; set; }

        public double ClimbGrade { get; set; }

        public double Crr { get; set; }

        public double WeightTotal
        {
            get
            {
                return WeightRider + WeightBike;
            }
        }

        public double A { get; set; }
        public double Cd { get; set; }
        public double Rho { get; set; }
        public double CdA { get; set; }

        public double GetCdA()
        {
            if (CdA == 0) return Cd * A;
            return CdA;
        }
    }

    internal class Forces
    {
        public double Gravity { get; set; }
        public double Rolling { get; set; }
        public double Drag { get; set; }

        public double Total
        {
            get
            {
                return Gravity + Rolling + Drag;
            }
        }

    }

}
