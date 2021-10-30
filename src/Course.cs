using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sunley.Orienteering.PurplePen.File;

namespace Sunley.Orienteering.PurplePen
{
    public class Course : IEnumerable<ControlPoint>
    {
        #region -- Fields --

        private List<ControlPoint> controls;

        #endregion

        #region -- Properties --

        public int Count => controls.Count;

        #endregion

        #region -- Accessors --

        public ControlPoint this[int n] => controls[n];

        public IEnumerator<ControlPoint> GetEnumerator()
        {
            foreach (ControlPoint c in controls)
                yield return c;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region -- Constructors --

        public Course()
        {
            controls = new List<ControlPoint>();
        }

        public Course(List<ControlPoint> c)
        {
            controls = c;
        }

        #endregion

        #region -- Public Methods --

        public void AddControl(ControlPoint c)
        {
            controls.Add(c);
        }

        public List<ControlPoint> ToList()
        {
            return controls;
        }
        public void RemoveControl(ControlPoint c)
        {
            controls.Remove(c);
        }

        public float CourseLength(ControlStore store)
        {
            float len = 0;

            for (int i  = 1; i < controls.Count; i++)
            {
                float leg = store.DistanceBetweenControls(controls[i - 1], controls[i]);
                len += leg;
            }

            return len;

        }
        #endregion

        #region -- Override Methods --

        public override string ToString()
        {
            string o = "";

            foreach (ControlPoint c in controls)
                o += "," + c.ID.ToString();


            return o.Substring(1);

        }

        #endregion
    }
}
