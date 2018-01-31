using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;

namespace VirtualSpeed
{
    
    public class VirtualSpeedCalculator
    {
        private const double G = 9.8067;

        public VirtualSpeedCalculator()
        {
            Parameters = new Parameters();
        }

        public VirtualSpeedCalculator(String parametersFile)
        {
            Parameters = new Parameters();

            XmlDocument xmlParameters = new XmlDocument();

            try
            {
                xmlParameters.Load(parametersFile);
            }
            // use default parameters if the xml load fails
            catch (Exception e)
            {
                Console.WriteLine("Error while loading parameters, default values are used.");
                return;
            }

            XmlNode parametersNode = xmlParameters.DocumentElement;

            //go through each node of the file to override the corresponding parameter
            foreach (XmlNode parameter in parametersNode)
            {
                Double paramValue;
                try
                {
                    paramValue = Double.Parse(parameter.InnerText, CultureInfo.InvariantCulture);

                    switch (parameter.Name)
                    {
                        case "WeightRider":
                            Parameters.WeightRider = paramValue;
                            break;
                        case "DriveTrainLoss":
                            Parameters.DriveTrainLoss = paramValue;
                            break;
                        case "WeightBike":
                            Parameters.WeightBike = paramValue;
                            break;
                        case "ClimbGrade":
                            Parameters.ClimbGrade = paramValue;
                            break;
                        case "Crr":
                            Parameters.Crr = paramValue;
                            break;
                        case "A":
                            Parameters.A = paramValue;
                            break;
                        case "Cd":
                            Parameters.Cd = paramValue;
                            break;
                        case "Rho":
                            Parameters.Rho = paramValue;
                            break;
                        case "CdA":
                            Parameters.CdA = paramValue;
                            break;
                        default:
                            break;
                    }
                }
                // stay on the default value if the value couldn't be parsed
                catch (Exception e)
                {
                    Console.WriteLine("Error while reading " + parameter.Name +  " parameter, default value is used.");
                }
            }
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

        public void PrintParameters()
        {
            Console.WriteLine("");
            Console.WriteLine("Parameters used : ");
            Console.WriteLine("");
            Console.WriteLine("\t- DriveTrainLoss : {0:F3}", DriveTrainLoss);
            Console.WriteLine("\t- WeightRider : {0:F1}", WeightRider);
            Console.WriteLine("\t- WeightBike : {0:F1}", WeightBike);
            Console.WriteLine("\t- ClimbGrade : {0:F1}", ClimbGrade);
            Console.WriteLine("\t- Crr : {0:F3}", Crr);
            Console.WriteLine("\t- A : {0:F3}", A);
            Console.WriteLine("\t- Cd : {0:F3}", Cd);
            Console.WriteLine("\t- Rho : {0:F3}", Rho);
            Console.WriteLine("\t- CdA : {0:F3}", CdA);
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
