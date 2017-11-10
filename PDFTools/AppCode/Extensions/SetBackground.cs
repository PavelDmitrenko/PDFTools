using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Rectangle = iTextSharp.text.Rectangle;

namespace PDFTools
{
	public static partial class PdfExtensions
	{

		public static void SetPageBackgroundColor(this PdfInfo pdfInfo, int pageNumber, BaseColor color = null)
		{
			if (color == null)
				color = BaseColor.WHITE;

			using (PdfReader reader = new PdfReader(pdfInfo.GetBytes()))
			{

				using (MemoryStream ms = new MemoryStream())
				{
					using (PdfStamper stamper = new PdfStamper(reader, ms))
					{
						PageInfo pageInfo = pdfInfo.GetPage(pageNumber);

						Rectangle rectangle = new Rectangle(0, 0, Utilities.MillimetersToPoints(pageInfo.PageWidth),
							Utilities.MillimetersToPoints(pageInfo.PageHeight), 0) {BackgroundColor = color};

						var cb = stamper.GetUnderContent(pageNumber);
						cb.Rectangle(rectangle);
					}

					pdfInfo.UpdateBytes(ms.ToArray());

				}

			}

		}

	}

}
