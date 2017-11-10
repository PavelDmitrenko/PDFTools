using System.IO;
using iTextSharp.text.pdf;

namespace PDFTools
{
	public static class PdfProperties
	{

		#region Compress
		public static void Compress(this PdfInfo pdfInfo)
		{
			using (var reader = new PdfReader(pdfInfo.GetBytes()))
			{
				using (MemoryStream ms = new MemoryStream())
				{
					using (PdfStamper stamper = new PdfStamper(reader, ms))
					{
						stamper.Writer.CompressionLevel = 9;
						int total = reader.NumberOfPages + 1;

						for (int i = 1; i < total; i++)
							reader.SetPageContent(i, reader.GetPageContent(i));

						stamper.SetFullCompression();
						stamper.FormFlattening = true;
						stamper.AnnotationFlattening = true;
						stamper.FreeTextFlattening = true;
						reader.RemoveUnusedObjects();
					}

					pdfInfo.UpdateBytes(ms.ToArray());
				}
			}
		} 
		#endregion

	}
}
