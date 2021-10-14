using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml;

namespace Sunley.Orienteering.PurplePen.File
{
    public class RawCourse
    {
        int f_id;
        string f_name;
        int f_firstControl;
        CourseType f_type;
        PrintArea f_printArea;
        int f_printScale;
        int f_order;
        string f_orientation;
        Size f_pageSize;

        public RawCourse(XmlNode node)
        {
            f_id = Convert.ToInt32(node.Attributes[0].Value);
            f_name = node.FirstChild.InnerText;
            f_firstControl = -1;

            f_order = Convert.ToInt32(node.Attributes[2].Value);

            string x = node.ChildNodes[2].Attributes[6].Value;
            string y = node.ChildNodes[2].Attributes[7].Value;
            f_pageSize = new Size(Convert.ToInt32(x), Convert.ToInt32(y));

            switch (node.Attributes[1].Value)
            {
                case "normal": f_type = CourseType.Normal; break;
                case "score": f_type = CourseType.Score; break;
                default: break;
            }

            f_printArea = new PrintArea(node.ChildNodes[2]);
            f_printScale = Convert.ToInt32(node.ChildNodes[3].Attributes[0].Value);
            f_orientation = node.ChildNodes[2].Attributes[9].Value;
        }
        public RawCourse(RawCourse b, int id, int firstControl)
        {
            f_id = id;
            f_name = id.ToString();
            f_firstControl = firstControl;
            f_type = b.f_type;
            f_printArea = b.f_printArea;
            f_printScale = b.f_printScale;
            f_order = id;
            f_orientation = b.f_orientation;
            f_pageSize = b.f_pageSize;
        }

        public override string ToString()
        {
            string type;
            if (f_type == CourseType.Normal)
                type = "normal";
            else type = "score";
            string str;
            if (f_firstControl == -1)
            {
                str =
                $"<course id =\"{f_id}\" kind=\"{type}\" order=\"{f_order}\">" +
                    $"<name>{f_name}</name>" +
                    $"<labels label-kind=\"sequence\" />" +
                    $"<print-area automatic=\"true\" restrict-to-page-size=\"true\" " +
                        $"left=\"{f_printArea.Left}\" top=\"{f_printArea.Top}\" right=\"{f_printArea.Right}\" bottom=\"{f_printArea.Bottom}\"" +
                        $"page-width=\"{f_pageSize.Width}\" page-height=\"{f_pageSize.Height}\" " +
                        $"page-margins=\"0\" page-landscape=\"{f_orientation}\" />" +
                    $"<options print-scale=\"{f_printScale}\" description-kind=\"symbols\" />" +
                $"</course>";
            }
            else
            {
                str =
                $"<course id =\"{f_id}\" kind=\"{type}\" order=\"{f_order}\">" +
                    $"<name>{f_name}</name>" +
                    $"<labels label-kind=\"sequence\" />" +
                    $"<first course-control=\"{f_firstControl}\" />" +
                    $"<print-area automatic=\"true\" restrict-to-page-size=\"true\" " +
                        $"left=\"{f_printArea.Left}\" top=\"{f_printArea.Top}\" right=\"{f_printArea.Right}\" bottom=\"{f_printArea.Bottom}\"" +
                        $"page-width=\"{f_pageSize.Width}\" page-height=\"{f_pageSize.Height}\" " +
                        $"page-margins=\"0\" page-landscape=\"{f_orientation}\" />" +
                    $"<options print-scale=\"{f_printScale}\" description-kind=\"symbols\" />" +
                $"</course>";
            }

            return str;
        }
    }

    public struct PrintArea
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public PrintArea(XmlNode node)
        {
            string left = node.Attributes[2].Value;
            string top = node.Attributes[3].Value;
            string right = node.Attributes[4].Value;
            string bottom = node.Attributes[5].Value;

            Left = (float)Convert.ToDouble(left);
            Top = (float)Convert.ToDouble(top);
            Right = (float)Convert.ToDouble(right);
            Bottom = (float)Convert.ToDouble(bottom);
        }
    }
    public enum CourseType
    {
        Normal,
        Score
    }
}
