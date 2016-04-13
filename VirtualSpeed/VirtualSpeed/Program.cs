using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VirtualSpeed
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var calc = new VirtualSpeedCalculator();

            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load("powerData.tcx");

            XmlNode trainingNode;
            trainingNode = myXmlDocument.DocumentElement;

            double cumulatedDistance = 0;
            DateTime prevTime = DateTime.MinValue;

            foreach (XmlNode activitiesNode in trainingNode.ChildNodes)
                foreach (XmlNode activityNode in activitiesNode.ChildNodes)
                    foreach (XmlNode lapNode in activityNode.ChildNodes)
                    {
                        if (lapNode.Name == "Lap")
                        {
                            XmlNode lapDistance = null;
                            XmlNode lapMaxSpeedNode = null;
                            XmlNode lapAvgSpeed = null;
                            XmlNode lapTime = null;

                            double lapCumulativeDistance = 0;
                            double lapMaxSpeed = 0;

                            foreach (XmlNode track in lapNode.ChildNodes)
                            {
                                if (track.Name == "DistanceMeters") {
                                    lapDistance = track;
                                }
                                else if (track.Name == "TotalTimeSeconds")
                                {
                                    lapTime = track;
                                }
                                else if (track.Name == "MaximumSpeed")
                                {
                                    lapMaxSpeedNode = track;
                                }
                                else if (track.Name == "Extensions")
                                {
                                    foreach (XmlNode ext in track.ChildNodes)
                                        foreach (XmlNode LX in ext.ChildNodes)
                                            if (LX.Name == "AvgSpeed")
                                                lapAvgSpeed = LX;
                                }
                                else if (track.Name == "Track")
                                    foreach (XmlNode point in track.ChildNodes)
                                    {
                                        XmlNode distance = null;
                                        XmlNode time = null;
                                        XmlNode watt = null;
                                        XmlNode speed = null;
                                        foreach (XmlNode value in point.ChildNodes)
                                        {
                                            if (value.Name == "Time")
                                                time = value;
                                            else if (value.Name == "DistanceMeters")
                                                distance = value;
                                            else if (value.Name == "Extensions")
                                            {
                                                foreach (XmlNode ext in value.ChildNodes)
                                                    foreach (XmlNode tcxValue in ext.ChildNodes)
                                                    {
                                                        if (tcxValue.Name == "ns3:Watts")
                                                            watt = tcxValue;
                                                        else if (tcxValue.Name == "ns3:Speed")
                                                            speed = tcxValue;
                                                    }
                                            }
                                        }

                                        var newSpeed = calc.CalculateVelocity(Double.Parse(watt.InnerText));
                                        var newSpeedMS = calc.ConvertKmhToMS(newSpeed);
                                        lapMaxSpeed = Math.Max(lapMaxSpeed, newSpeedMS);

                                        var pointDistance = calc.CalculateDistance(newSpeed, 1);
                                        cumulatedDistance = cumulatedDistance + pointDistance;
                                        lapCumulativeDistance = lapCumulativeDistance + pointDistance;

                                        distance.InnerText = cumulatedDistance.ToString(new CultureInfo("en-US"));
                                        speed.InnerText = newSpeedMS.ToString(new CultureInfo("en-US"));
                                    }
                            }

                            lapDistance.InnerText= lapCumulativeDistance.ToString(new CultureInfo("en-US"));
                            lapMaxSpeedNode.InnerText=lapMaxSpeed.ToString(new CultureInfo("en-US"));

                            var avg = lapCumulativeDistance / Double.Parse(lapTime.InnerText);

                            lapAvgSpeed.InnerText = avg.ToString(new CultureInfo("en-US"));

                        }
                    }
            myXmlDocument.Save("Fixed.tcx");

        }

        private void Test(string[] args)
        {
            var what = args[0];

            var calc = new VirtualSpeedCalculator();

            if (what.Equals("P"))
            {
                var speed = Double.Parse(args[1]);

                var pwr = calc.CalculatePower(speed);
                Console.WriteLine("Measuring Power needed to go to a given speed");
                Console.WriteLine("{0} Km/h -> {1}W", speed, pwr);
            }
            else if (what.Equals("V"))
            {
                var pwr = Double.Parse(args[1]);

                var speed = calc.CalculateVelocity(pwr);
                Console.WriteLine("Measuring Speed obtained for a given power");
                Console.WriteLine("{0} W -> {1} Km/h", pwr, speed);
            }

            else if (what.Equals("D"))
            {
                var pwr = Double.Parse(args[1]);
                var time = Double.Parse(args[2]);

                var speed = calc.CalculateVelocity(pwr);
                var distance = calc.CalculateDistance(speed, time);
                Console.WriteLine("Measuring Distance for given power");
                Console.WriteLine("{0} W for {1} sec = {2} meters", pwr, time, distance);
            }

            Console.ReadLine();
        }

    }


}
