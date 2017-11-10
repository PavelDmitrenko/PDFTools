using System;
using iTextSharp.text;
using iTextSharp.text.pdf.parser;

namespace PDFTools
{
	public class TextInfo
	{
		public string Text { get; }

		#region ctor
		public TextInfo(TextRenderInfo renderInfo)
		{
			Text = renderInfo.GetText();

			var bottomLeftPoint = renderInfo.GetDescentLine().GetStartPoint();
			var topRightPoint = renderInfo.GetAscentLine().GetEndPoint();

			var rectangle = new Rectangle(
									bottomLeftPoint[Vector.I1],
									bottomLeftPoint[Vector.I2],
									topRightPoint[Vector.I1],
									topRightPoint[Vector.I2]
			);

			var fontSize = Convert.ToDouble(rectangle.Height);
		} 
		#endregion


	}
}
