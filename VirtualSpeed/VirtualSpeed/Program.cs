using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace VirtualSpeed
{
    class Program
    {
        
        static void Main(string[] args)
        {
            // abort execution if filename is missing
            if (args.Length == 0)
            {
                Console.WriteLine("Error : tcx file name is missing !");
                return;
            }

            string filename = args[0];

            // abort if file to fix is not a tcx
            if (Path.GetExtension(filename) != ".tcx")
            {
                Console.WriteLine("Error : input file is not a tcx");
                return;
            }

            VirtualSpeedCalculator calc;

            // check if a second argument is given (parameters file name)
            if (args.Length > 1)
                calc = new VirtualSpeedCalculator(args[1]);
            else
                calc = new VirtualSpeedCalculator();

            calc.Parameters.PrintParameters();

            XmlDocument myXmlDocument = new XmlDocument();

            // try to open the xml file and abort if any exception is raised
            try
            {
                myXmlDocument.Load(filename);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error while loading xml file : " + e.Message);
                return;
            }

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

                                        double newSpeed = 0.0;

                                        // check if watt information is missing
                                        if (watt == null)
                                        {
                                            // if speed node is present then keep the raw speed without adjusting it
                                            if (speed != null)
                                            {
                                                // speed in tcx is in m/s
                                                newSpeed = Double.Parse(speed.InnerText, CultureInfo.InvariantCulture) * 3600 / 1000;
                                            }
                                        }
                                        else
                                        {
                                            newSpeed = calc.CalculateVelocity(Double.Parse(watt.InnerText));
                                        }

                                        var newSpeedMS = calc.ConvertKmhToMS(newSpeed);
                                        lapMaxSpeed = Math.Max(lapMaxSpeed, newSpeedMS);

                                        var pointDistance = calc.CalculateDistance(newSpeed, 1);
                                        cumulatedDistance = cumulatedDistance + pointDistance;
                                        lapCumulativeDistance = lapCumulativeDistance + pointDistance;

                                        // DistanceMeters node might not be present in a Trackpoint node, don't try to replace it if so
                                        if (distance != null)
                                            distance.InnerText = cumulatedDistance.ToString(new CultureInfo("en-US"));

                                        // speed node might not be present in an Extension node, don't try to replace it if so
                                        if (speed != null)
                                            speed.InnerText = newSpeedMS.ToString(new CultureInfo("en-US"));
                                    }
                            }

                            lapDistance.InnerText= lapCumulativeDistance.ToString(new CultureInfo("en-US"));
                            lapMaxSpeedNode.InnerText=lapMaxSpeed.ToString(new CultureInfo("en-US"));

                            var avg = lapCumulativeDistance / Double.Parse(lapTime.InnerText);

                            lapAvgSpeed.InnerText = avg.ToString(new CultureInfo("en-US"));

                        }
                    }
            myXmlDocument.Save(Path.GetFileNameWithoutExtension(filename) + "_fixed.tcx");

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
