/*
 * $Id:
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */
using System;
using Cairo;
using SolidV.Cairo;

namespace SolidV.MVC
{
    public class ArrowShapeViewer : ShapeViewer
    {
        public ArrowShapeViewer()
        {
        }
        
        public override void DrawItem(IView<Context, Model> view, Context context, object item)
        {
            ArrowShape shape = (ArrowShape)item;
            context.MoveTo(shape.Form.Location);
            context.Color = shape.Style.BorderColor;
            context.ArrowLineTo(shape.To.Location);
            context.Stroke();
        }
        
    }
}