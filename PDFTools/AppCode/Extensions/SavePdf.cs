using System.IO;

namespace PDFTools
{
	public static partial class PdfExtensions
	{
		public static void Save(this PdfInfo pdfInfo, string file)
		{
			File.WriteAllBytes(file, pdfInfo.GetBytes());
		}
	}
}
