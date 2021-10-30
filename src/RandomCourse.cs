using Sunley.Debugging;
using Sunley.Miscellaneous;
using Sunley.Orienteering.PurplePen.File;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sunley.Orienteering.PurplePen
{
    public class RandomCourse
    {
        #region -- Fields --

        private Course course;
        private ControlStore controlStore;
        private int courseLength;
        private Random rand;

        private int angleTolerance;
        private float randomControlCutoff;
        private int lastControlMaximumDistance;

        private LegLengths legLengths;
        private LegProbabilities legProbabilities;

        #endregion

        #region -- Properties --

        public Course Course => course;



        #endregion

        

        #region -- Constructors --

        public RandomCourse(ControlStore store, int cLength = 7000)
        {
            controlStore = store;
            courseLength = cLength;

            course = new Course();
            rand = new Random();

            legLengths = LegLengths.DefaultValue;
            legProbabilities = LegProbabilities.DefaultValue;

            angleTolerance = 75;
        }

        public RandomCourse(ControlStore store, int cLength, int angTolerance, float rndControlCutoff, int lastControlMaxDist, LegLengths lengths, LegProbabilities probabilities)
        {
            controlStore = store;
            courseLength = cLength;

            angleTolerance = angTolerance;
            randomControlCutoff = rndControlCutoff;
            lastControlMaximumDistance = lastControlMaxDist;

            legLengths = lengths;
            legProbabilities = probabilities;

            course = new Course();
            rand = new Random();
        }

        #endregion

        #region -- Public Methods --

        public Course CreateCourse()
        {
            CreateRndCourse();
            
            return Course;
        }

        

        #endregion

        #region -- Private Methods --

        void CreateRndCourse()
        {
            Log.Counters.Add("Nearest");
            // Choose Start
            Log.Message("Started");

            ControlPoint start = controlStore.ChooseStart();
            course.AddControl(start);
            // Choose First set of conttrols

            // Choose First Control

            
            LegLength l = new LegLength(0, legLengths.Short_Max);
            List<ControlPoint> validt = new List<ControlPoint>();

            foreach (ControlPoint c in controlStore)
            {
                float legDist = controlStore.DistanceBetweenControls(c, course.Last());

                if (legDist >= l.Minimum && legDist <= l.Maximum)
                {
                    if (c.Type == ControlPointType.Normal)
                    {
                        validt.Add(c);
                    }
                }
            }

            if (validt.Count != 0)
            {
                int rndt = rand.Next(validt.Count);
                course.AddControl(validt[rndt]);
            }
            else
            {
                // Choose Nearest - Might change this decision
            }

            Log.Message($"Chosen first control:  {course.Last().Code.ToString()}");

            // Choose Other Controls

            float courseLen = 0;

            while (courseLen < (courseLength * randomControlCutoff))
            {
                // Select valid Controls

                List<ControlPoint> valid = ChooseValidControls();

                if (valid.Count != 0)
                {
                    int rnd = rand.Next(valid.Count);
                    course.AddControl(valid[rnd]);
                }
                else
                {
                    // Choose Nearest - Might change this decision
                    Log.Counters.Increment("Nearest");
                    Log.Warning("Nearest control chosen. " + Log.Counters["Nearest"]);
                    

                    ControlPoint nearest = new ControlPoint();
                    float dist = float.MaxValue;

                    foreach (ControlPoint c in controlStore)
                    {
                        float leg = controlStore.DistanceBetweenControls(course.Last(), c);
                        if (leg < dist && !course.Contains(c) && c.Type == ControlPointType.Normal)
                        {
                            nearest = c;
                            dist = leg;
                        }
                    }
                    course.AddControl(nearest);
                }

                Log.Message($"Chosen Control {course.Count}: {course.Last().Code}");

                float legLen = controlStore.DistanceBetweenControls(course[course.Count - 2], course[course.Count - 1]);
                courseLen += legLen;
            }

            // Choose Finish
             
            ControlPoint finish = controlStore.ChooseFinish(course.Last());

            Log.Message("Chosen Finish");

            // Work towards finish

            bool cont = true;
            while (cont)
            {
                // Get valid controls

                List<ControlPoint> valid = ChooseValidControls();

                // Choose random 

                if (valid.Count != 0)
                {
                    int count;
                    if (valid.Count >= 10)
                        count = 10;
                    else
                        count = Convert.ToInt32(Math.Ceiling(valid.Count / 2.0));

                    List<ControlPoint> chosen = new List<ControlPoint>();


                    for (int i = 0; i < count; i++)
                    {
                        int rnd = rand.Next(valid.Count);
                        chosen.Add(valid[rnd]);
                        valid.RemoveAt(rnd);
                    }

                    // Choose most direct of them

                    ControlPoint direct = new ControlPoint();
                    double angle = 0;

                    foreach (ControlPoint ct in chosen)
                    {
                        double ang = Misc.AngleBetweenThreePoints(course.Last().Position, ct.Position, finish.Position);
                        if (ang > 180)
                            ang = 360 - ang;

                        if (ang > angle)
                        {
                            angle = ang;
                            direct = ct;
                        }
                    }

                    course.AddControl(direct);
                }
                else
                {
                    Log.Counters.Increment("Nearest");
                    Log.Warning("Nearest Control Chosen. " + Log.Counters["Nearest"]);
                    
                    ControlPoint nearest = new ControlPoint();
                    float distt = float.MaxValue;

                    foreach (ControlPoint c in controlStore)
                    {
                        float leg = controlStore.DistanceBetweenControls(course.Last(), c);
                        if (leg < distt && !course.Contains(c))
                        {
                            nearest = c;
                            distt = leg;
                        }
                    }
                    course.AddControl(nearest);
                }



                Log.Message($"Working towards finish, Control {course.Count}: {course.Last().Code.ToString()}");

                // Check distance to finish and decide whether done

                float dist = controlStore.DistanceBetweenControls(course.Last(), finish);
                if (dist <= lastControlMaximumDistance)
                    cont = false;

                if (course.CourseLength(controlStore) > courseLength * 1.25)
                    cont = false;
            }

            course.AddControl(finish);

            //if (course.Last() == course[course.Count - 2])
            //{
            //    course.RemoveControl(course.Last());
            //}

            Log.Info("FINISHED");
            Log.Counters.Remove("Nearest");
        }
        LegLength ChooseLegLength(LegLengths legLengths, LegProbabilities legProbabilties)
        {
            float rnd = (float)rand.NextDouble();

            if (rnd <= legProbabilties.VeryShort_Sum())
            {
                return new LegLength(0, legLengths.VeryShort_Max);
            }
            else if (rnd <= legProbabilties.Short_Sum())
            {
                return new LegLength(legLengths.VeryShort_Max, legLengths.Short_Max);
            }
            else if (rnd <= legProbabilties.Medium_Sum())
            {
                return new LegLength(legLengths.Short_Max, legLengths.Medium_Max);
            }
            else
            {
                return new LegLength(legLengths.Medium_Max, int.MaxValue);
            }
        }
        List<ControlPoint> ChooseValidControls()
        {
            LegLength legLength = ChooseLegLength(legLengths, legProbabilities);

            List<ControlPoint> valid = new List<ControlPoint>();

            foreach (ControlPoint c in controlStore)
            {
                float legDist = controlStore.DistanceBetweenControls(c, course.Last());
                //Log.Error(legDist.ToString("F0"));

                if (legDist >= legLength.Minimum && legDist <= legLength.Maximum)
                {
                    double angle = Misc.AngleBetweenThreePoints(course[course.Count - 2].Position, course[course.Count - 1].Position, c.Position);

                    if (angle >= (180 - angleTolerance) && angle <= (180 + angleTolerance))
                    {
                        if (c.Type == ControlPointType.Normal)
                        {
                            if (!course.Contains(c))
                            {
                                valid.Add(c);
                            }
                        }
                    }
                }
            }

            return valid;
        }



        #endregion

        #region -- Override Methods --

        public override string ToString()
        {
            string o = "";

            foreach (ControlPoint c in course)
                o += "," + c.ID.ToString();


            return o.Substring(1);
        }

        #endregion
    }

    public struct LegLengths
    {
        public int VeryShort_Max { get; set; }
        public int Short_Max { get; set; }
        public int Medium_Max { get; set; }

        public LegLengths(int vShort, int shortt, int medium)
        {
            VeryShort_Max = vShort;
            Short_Max = shortt;
            Medium_Max = medium;
        }

        public static LegLengths DefaultValue = new LegLengths(250, 600, 1200);
    }
    public struct LegProbabilities
    {
        public float VeryShort { get; set; }
        public float Short { get; set; }
        public float Medium { get; set; }
        public float Long { get; set; }

        public LegProbabilities(float vShort, float shortt, float medium, float longg)
        {
            if (vShort + shortt + medium + longg != 1)
                throw new Exception("Probabilities must add up to 1");

            VeryShort = vShort;
            Short = shortt;
            Medium = medium;
            Long = longg;
        }

        public float VeryShort_Sum()
        {
            return VeryShort;
        }
        public float Short_Sum()
        {
            return VeryShort + Short;
        }
        public float Medium_Sum()
        {
            return VeryShort + Short + Medium;
        }

        public static LegProbabilities DefaultValue = new LegProbabilities(0.2f, 0.4f, 0.3f, 0.1f);
    }
    public struct LegLength
    {
        public int Minimum { get; }
        public int Maximum { get; }

        public LegLength(int min, int max)
        {
            Minimum = min;
            Maximum = max;
        }
    }
}
