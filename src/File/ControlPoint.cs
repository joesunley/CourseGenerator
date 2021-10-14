using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sunley.Orienteering.PurplePen.File
{
    public class ControlPoint
    {
        #region -- Fields --

        private int f_id;
        private int f_code;
        private PointF f_loc;
        private ControlPointType f_type;

        #endregion

        #region -- Properties --

        public int ID => f_id;
        public int Code => f_code;
        public PointF Position => f_loc;
        public ControlPointType Type => f_type;

        #endregion

        #region -- Constructors --

        public ControlPoint(XmlNode node)
        {
            string id = node.Attributes[0].Value;
            string type = node.Attributes[1].Value;
            string code = "";
            string x = "";
            string y = "";

            switch (type)
            {
                case "normal": f_type = ControlPointType.Normal; break;
                case "start": f_type = ControlPointType.Start; break;
                case "finish": f_type = ControlPointType.Finish; break;
                default: break;
            }

            if (f_type == 0)
            {
                code = node.ChildNodes[0].InnerText;
                x = node.ChildNodes[1].Attributes[0].Value;
                y = node.ChildNodes[1].Attributes[1].Value;


                f_id = Convert.ToInt32(id);
                f_code = Convert.ToInt32(code);
                f_loc = new PointF((float)Convert.ToDouble(x), (float)Convert.ToDouble(y));
            }
            else
            {
                x = node.ChildNodes[0].Attributes[0].Value;
                y = node.ChildNodes[0].Attributes[1].Value;

                f_id = Convert.ToInt32(id);
                f_code = -1;
                f_loc = new PointF((float)Convert.ToDouble(x), (float)Convert.ToDouble(y));

            }
        }
        public ControlPoint() { }

        #endregion
    }
    public enum ControlPointType
    {
        Normal,
        Start,
        Finish
    }
}
