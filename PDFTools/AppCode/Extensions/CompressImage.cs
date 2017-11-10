using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PDFTools
{
	public static partial class PdfExtensions
	{

		#region CompressImage
		public static PdfInfo CompressImage(this PdfInfo pdfInfo, ImageInfo image, IImageConverter converter)
		{

			byte[] convertedBytes = converter.Convert(image.ImageBytes);
			//PdfImage newImage = PdfImage.GetInstance(convertedBytes);
			//ImageCodecInfo encoderInfo = ImageCodecInfo.GetImageEncoders().First(i => i.MimeType == "image/png");

			//ConvertToPng c = new ConvertToPng();
			//NetImage netImage = c.Convert(image.ImageBytes);
			//convertedBytes = System.IO.File.ReadAllBytes(@"x:\img.jpg");
			Image newImage = Image.GetInstance(convertedBytes);


			using (PdfReader reader = new PdfReader(pdfInfo.GetBytes()))
			{

				using (MemoryStream ms = new MemoryStream())
				{
					using (PdfStamper stamper = new PdfStamper(reader, ms))
					{
						PdfDictionary page = reader.GetPageN(image.PageNumber);
						ObjectFinder objectFinder = new ObjectFinder(page);

						PdfObject imgObject = objectFinder.FindObjectByGuid(image.ID);

						if (imgObject != null)
						{
							PdfReader.KillIndirect(imgObject);
							stamper.Writer.AddDirectImageSimple(newImage, (PRIndirectReference)imgObject);

						}
					}

					pdfInfo.UpdateBytes(ms.ToArray());
				}

			}

			return pdfInfo;

		}
		#endregion
	}
}
