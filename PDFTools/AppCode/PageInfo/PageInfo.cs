using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using PDFTools.AppCode.Extensions;
using PdfImage = iTextSharp.text.Image;

namespace PDFTools
{
	public class PageInfo
	{
		public ImagesInfo ImagesInfo { get; private set; }
		public TextsInfo TextsInfo { get; private set; }
		public bool IsCoverImage => _IsFullPageImage();
		public bool IsFullPageTransparentImage => _IsFullPageTransparentImage();
		public int PageNumber { get; }
		public float PageWidth { get; }
		public float PageHeight { get; }
		private float PageUnits { get; }

		private PdfDictionary _page;
		private readonly PdfReader _reader;

		#region ctor
		public PageInfo(PdfReader reader, int pageNumber)
		{
		
			PageNumber = pageNumber;
			ImagesInfo = new ImagesInfo();
			TextsInfo = new TextsInfo();
			_reader = reader;
			_page = reader.GetPageN(pageNumber);
		
			PageUnits = _page.Contains(PdfName.USERUNIT) ? _page.GetAsNumber(PdfName.USERUNIT).FloatValue : 72;
			
			Rectangle mediabox = reader.GetPageSize(_page);
			PageWidth = Utilities.PointsToMillimeters(mediabox.Width);
			PageHeight = Utilities.PointsToMillimeters(mediabox.Height);

			_RenderPage(PageNumber, PageUnits);

		}
		#endregion

		#region _IsFullPageTransparentImage
		private bool _IsFullPageTransparentImage()
		{
			bool isFullPage = _IsFullPageImage();
			if (!isFullPage)
				return false;

			ImageInfo singleImage = ImagesInfo.Items[0];

			if (singleImage.BitsPerPixel == 1)
				return true;

			return false;
		} 
		#endregion

		#region _IsFullPageImage
		private bool _IsFullPageImage()
		{
			if (ImagesInfo.Count != 1 || TextsInfo.AnyText)
				return false;

			ImageInfo singleImage = ImagesInfo.Items[0];

			if (!singleImage.IsMask
				&& singleImage.WidthMm.ApproximatelyEquals(PageWidth, 5)
				&& singleImage.HeightMm.ApproximatelyEquals(PageHeight, 5))
				return true;

			return false;
		} 
		#endregion

		#region _RenderPage
		private void _RenderPage(int pageNumber, float pageUnits)
		{
			PdfReaderContentParser parser = new PdfReaderContentParser(_reader);
			RenderListener renderListener = new RenderListener(_reader, pageNumber, pageUnits);
			parser.ProcessContent(pageNumber, renderListener);

			ImagesInfo = renderListener.ImagesInfo;
			TextsInfo = renderListener.TextsInfo;
		} 
		#endregion

	}
}
