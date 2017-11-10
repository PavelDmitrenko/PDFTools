using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text.exceptions;
using iTextSharp.text.pdf;
using PdfImage = iTextSharp.text.Image;
using NetImage = System.Drawing.Image;

namespace PDFTools
{
	public class PdfInfo
	{
		public int PagesCount => _pages.Count;
		public int ImagesCount => _pages.Sum(x => x.ImagesInfo.Count);

		private readonly List<PageInfo> _pages;
		public readonly bool Invalid;
		public readonly bool IsPasswordProtected;

		public IEnumerable<PageInfo> Pages => _pages.AsReadOnly();
		public IEnumerable<ImageInfo> Images => _pages.SelectMany(x => x.ImagesInfo.Items).ToList().AsReadOnly();

		private byte[] _pdfBytes;
		private readonly PdfReader _reader;

		#region ctor
		public PdfInfo(byte[] pdfBytes)
		{
			_pages = new List<PageInfo>();
			_pdfBytes = pdfBytes;
			_reader = null;
			
			try
			{
				_reader = new PdfReader(pdfBytes);
			}
			catch (InvalidPdfException exInvalidPdf)
			{
				Invalid = true;
				return;
			}

			if (!_reader.IsOpenedWithFullPermissions)
			{
				IsPasswordProtected = true;
				return;
			}

			var info = _reader.Info;

			using (MemoryStream ms = new MemoryStream())
			{
				using (PdfStamper stamper = new PdfStamper(_reader, ms))
				{
					for (int p = 1; p <= _reader.NumberOfPages; p++)
					{
						PageInfo pageInfo = new PageInfo(_reader, p);
						_pages.Add(pageInfo);
					}
				}
				_pdfBytes = ms.ToArray();
				Invalid = false;
				IsPasswordProtected = false;
			}

		}
		#endregion

		#region GetPage
		public PageInfo GetPage(int pageNumber)
		{
			return _pages.FirstOrDefault(x => x.PageNumber == pageNumber);
		} 
		#endregion

		#region GetBytes
		public byte[] GetBytes()
		{
			return _pdfBytes;
		}
		#endregion

		#region UpdateBytes
		public void UpdateBytes(byte[] bytes)
		{
			_pdfBytes = bytes;
		}
		#endregion

	}
}
