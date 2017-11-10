using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using PDFTools;


public class RenderListener : IRenderListener
{

	private readonly PdfReader _reader;
	private readonly float _pageUnits;
	private readonly int _pageNumber;

	public ImagesInfo ImagesInfo { get; }
	public TextsInfo TextsInfo { get; }
	public int TextObjectsCount { get; private set; }
	public void BeginTextBlock() { }
	public void EndTextBlock() { }

	#region ctor
	public RenderListener(PdfReader reader, int pageNumber, float pageUnits)
	{
		_reader = reader;
		_pageNumber = pageNumber;
		_pageUnits = pageUnits;
		ImagesInfo = new ImagesInfo();
		TextsInfo = new TextsInfo();
	} 
	#endregion

	#region RenderText
	public void RenderText(TextRenderInfo renderInfo)
	{
		TextObjectsCount++;
		TextsInfo.Add(new TextInfo(renderInfo));
	}
	#endregion

	#region RenderImage
	public void RenderImage(ImageRenderInfo renderInfo)
	{
		float area = renderInfo.GetArea();
		Matrix matrix = renderInfo.GetImageCTM();

		int? mcid = renderInfo.GetMcid();

		PdfIndirectReference indirectRef = renderInfo.GetRef();
		if (indirectRef == null)
			return;
		
		PdfObject pdfObject = null;
		PdfDictionary imgDictionary = null;
		ImageInfo imageInfo = null;


		
		PdfObject directObj = _reader.GetPdfObject(indirectRef.Number);
		PdfDictionary imgObject = (PdfDictionary)PdfReader.GetPdfObject(directObj);

		imageInfo = new ImageInfo(imgObject, matrix, _pageUnits, _pageNumber, renderInfo);

		if (imageInfo.IsImage)
		{
			imgObject.Put(new PdfName("ITXT_ObjectId"), new PdfName(imageInfo.ID.ToString()));
			ImagesInfo.Add(imageInfo);
		}

		//PRStream maskStream = (PRStream)imgDictionary.GetAsStream(PdfName.MASK) ?? (PRStream)imgDictionary.GetAsStream(PdfName.SMASK);
		//if (maskStream != null)
		//{
		//	PdfImageObject maskImage = new PdfImageObject(maskStream);
		//	string filtype = maskImage.GetFileType();
		//	var bytes = maskImage.GetImageAsBytes();
		//	System.IO.File.WriteAllBytes(@"c:\1.png", bytes);
		//}
		//var props = renderInfo.GetImage().GetDrawingImage();

		//if (extension == ".jpeg")
		//	tempImage.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
		//else

		//props.Save(@"c:\1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
		//if (indirectRef != null)
		//{
		//	pdfObject = _reader.GetPdfObject(indirectRef.Number);
		//	imgDictionary = (PdfDictionary) PdfReader.GetPdfObject(pdfObject);
		//	imageInfo = new ImageInfo(imgDictionary, matrix, _pageUnits, _pageNumber, renderInfo);
		//}
		//else
		//{
		//	imgDictionary = renderInfo.GetImage().GetDictionary();
		//	imageInfo = new ImageInfo(imgDictionary, matrix, _pageUnits, _pageNumber, renderInfo);
		//}


	} 
	#endregion

}