using System.IO;

namespace PDFTools
{
	public static partial class PdfExtensions
	{
		public static void ExportImages(this PdfInfo pdfInfo, string dir, string filePrefix)
		{
			int c = 1;
			foreach (ImageInfo image in pdfInfo.Images)
			{
				string imFn = $@"{dir}\{filePrefix}{c}.{image.ImageFileType.ToString().ToLower()}";
				File.WriteAllBytes(imFn, image.ImageBytes);
				c++;
			}
		}
	}
}
