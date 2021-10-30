using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;

namespace Sunley.Orienteering.PurplePen.File
{
    public class ControlStore : IEnumerable<ControlPoint>
    {
        #region -- Fields --

        private Dictionary<int, ControlPoint> controlDict = new Dictionary<int, ControlPoint>();

        private List<RawCourse> courses = new List<RawCourse>();

        public Random random = new Random();
        private int scale;
        private string filePath;

        #endregion

        #region -- Properties --

        public int Count
        {
            get
            {
                return controlDict.Count;
            }
        }

        #endregion

        #region -- Accessors --

        public ControlPoint this[int id]
        {
            get
            {
                return controlDict[id];
            }
        }

        public IEnumerator<ControlPoint> GetEnumerator()
        {
            foreach (KeyValuePair<int, ControlPoint> c in controlDict)
            {
                yield return c.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        #endregion

        #region -- Constructors --

        public ControlStore(string fileLoc)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileLoc);
            CreateControls(doc);

            random = new Random();
            filePath = fileLoc;
        }
        public ControlStore(int scale)
        {
            this.scale = scale;
        }

        #endregion

        #region -- Public Methods --

        public ControlPoint ChooseStart()
        {
            List<ControlPoint> valid = new List<ControlPoint>();

            foreach (ControlPoint c in controlDict.Values)
                if (c.Type == ControlPointType.Start)
                    valid.Add(c);

            return valid[random.Next(valid.Count)];
        }
        public ControlPoint ChooseFinish(ControlPoint last)
        {
            float length = float.MaxValue;
            ControlPoint nearest = new ControlPoint();

            foreach (ControlPoint c in controlDict.Values)
            {
                if (c.Type == ControlPointType.Finish)
                {
                    float dist = DistBtwnCtrls(last, c);

                    if (dist < length)
                    {
                        length = dist;
                        nearest = c;
                    }
                }
            }

            return nearest;
        }
        public float DistanceBetweenControls(ControlPoint a, ControlPoint b)
        {
            return DistBtwnCtrls(a, b) * (float)(scale / 1000f);
        }


        public bool SaveCourses(List<Course> courses) {
            return SaveCourses(courses, @"RandomCourse.ppen");
        }
        public bool SaveCourses(List<Course> courses, string filePath) {
            try {
                CreateXML(courses, filePath);
                return true;
            } catch { return false; }
        }

        public void AddControl(ControlPoint c)
        {
            if (c.ID == -1)
            {
                c.AddID(controlDict.Count + 1);
            }

            controlDict.Add(c.ID, c);
        }

        #endregion

        #region -- Private Methods --

        void CreateControls(XmlDocument doc)
        {
            XmlNode eventDetails = doc.FirstChild.FirstChild;

            scale = Convert.ToInt32(eventDetails.ChildNodes[3].Attributes[0].Value);


            for (int i = 1; i < doc.FirstChild.ChildNodes.Count; i++)
            {
                XmlNode node = doc.FirstChild.ChildNodes[i];
                if (node.Name == "control")
                {
                    ControlPoint c = new ControlPoint(node);

                    controlDict.Add(c.ID, c);
                }
                else if (node.Name == "course")
                {
                    courses.Add(new RawCourse(node));
                }
            }
        }

        bool CheckAngle(ControlPoint previous, ControlPoint last, ControlPoint current)
        {
            PointF a = previous.Position;
            PointF b = last.Position;
            PointF c = current.Position;

            float
                ab = (float)Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2)),
                bc = (float)Math.Sqrt(Math.Pow(c.X - b.X, 2) + Math.Pow(c.Y - b.Y, 2)),
                ac = (float)Math.Sqrt(Math.Pow(c.X - a.X, 2) + Math.Pow(c.Y - a.Y, 2));

            float
                top = (float)Math.Pow(ab, 2) + (float)Math.Pow(bc, 2) - (float)Math.Pow(ac, 2),
                bottom = 2 * ab * bc;

            float angle = (float)Math.Acos(top / bottom) * (float)(180 / Math.PI);

            return angle >= 45 && angle <= 315;
        }

        float DistBtwnCtrls(ControlPoint a, ControlPoint b)
        {
            return (float)Math.Sqrt(Math.Pow(b.Position.X - a.Position.X, 2) + Math.Pow(b.Position.Y - a.Position.Y, 2));
        }

        Tuple<int, int> ChooseLength()
        {
            float n = (float)random.NextDouble();
            if (n <= 0.1)
            {
                // Long
                return new Tuple<int, int>(1000, int.MaxValue);
            }
            else if (n <= 0.4)
            {
                // Medium
                return new Tuple<int, int>(500, 1000);
            }
            else if (n <= 0.8)
            {
                // Short
                return new Tuple<int, int>(200, 500);
            }
            else
            {
                // Very Short
                return new Tuple<int, int>(0, 200);
            }
        }


        void CreateXML(List<Course> courses, string outFilePath)
        {
            List<string> courseStr = new List<string>();
            string controlStr = "";
            int cControlID = 1;
            int courseID = 1;

            foreach (Course course in courses)
            {
                string title = "";
                courseStr.Add(CourseStr(courseID, cControlID, scale, new List<string>() { title }));
                courseID++;

                for (int i = 0; i < course.Count; i++)
                {
                    if (i == course.Count - 1)
                        controlStr += EndControlStr(cControlID, course[i].ID);
                    else
                        controlStr += ControlStr(cControlID, cControlID + 1, course[i].ID);

                    cControlID++;
                }
            }

            string text = System.IO.File.ReadAllText(filePath);

            int loc = text.IndexOf("<course id=");
            text = text.Substring(0, loc);

            foreach (string s in courseStr) { text += s; }
            text += controlStr;
            text += "</course-scribe-event>";

            System.IO.File.WriteAllText(outFilePath, text);

        }

        static string ControlStr(int courseC1, int courseC2, int controlID)
        {
            return "<course-control id=\"" + courseC1.ToString() + "\" control=\"" + controlID.ToString() + "\">"
                + Environment.NewLine
                + "<next course-control=\"" + courseC2.ToString() + "\" />"
                + Environment.NewLine
                + "</course-control>"
                + Environment.NewLine;
        }
        static string EndControlStr(int courseC1, int controlID)
        {
            return "<course-control id=\"" + courseC1.ToString() + "\" control=\"" + controlID.ToString() + "\" />"
                + Environment.NewLine;
        }
        static string CourseStr(int id, int firstC, int scale, List<string> data)
        {
            string s = "";
            foreach (string d in data) { s += d + "|"; }
            try { s = s.Substring(0, s.Length - 1); } catch { }


            return
                "<course id=\"" + id.ToString() + "\" kind=\"normal\" order=\"" + id.ToString() + "\">"
                + Environment.NewLine
                + "<name>Course " + id.ToString() + "</name>"
                + Environment.NewLine
                + "<secondary-title>" + s + "</secondary-title>"
                + Environment.NewLine
                + "<labels label-kind=\"sequence\" />"
                + Environment.NewLine
                + "<first course-control=\"" + firstC.ToString() + "\" />"
                + Environment.NewLine
                + "<print-area automatic=\"true\" restrict-to-page-size=\"true\" "
                + "left=\"16.7639771\" top=\"219.328979\" right=\"313.689972\" bottom=\"9.270966\" page-width=\"827\" page-height=\"1169\" page-margins=\"0\" page-landscape=\"true\" />"
                + Environment.NewLine
                + "<options print-scale=\"" + scale.ToString() + "\" description-kind=\"symbols\" />"
                + Environment.NewLine
                + "</course>"
                + Environment.NewLine;
        }

        #endregion
    }
}
