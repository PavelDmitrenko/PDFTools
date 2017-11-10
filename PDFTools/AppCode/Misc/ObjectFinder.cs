using System;
using iTextSharp.text.pdf;

namespace PDFTools
{
	public class ObjectFinder
	{

		private readonly PdfDictionary _page;

		#region ctor
		public ObjectFinder(PdfDictionary page)
		{
			_page = page;
		} 
		#endregion

		#region FindObjectByGuid
		public PdfObject FindObjectByGuid(Guid guid)
		{
			PdfDictionary resources = (PdfDictionary)PdfReader.GetPdfObject(_page.Get(PdfName.RESOURCES));
			PdfDictionary xobject = (PdfDictionary)PdfReader.GetPdfObject(resources.Get(PdfName.XOBJECT));

			if (xobject == null)
				return null;

			PdfName valueToFind = new PdfName(guid.ToString());

			foreach (PdfName name in xobject.Keys)
			{
				PdfObject obj = xobject.Get(name);
				if (!obj.IsIndirect())
					continue;

				PdfDictionary childObject = (PdfDictionary)PdfReader.GetPdfObject(obj);
				if (childObject == null)
					continue;

				PdfObject curValue = childObject.Get(new PdfName("ITXT_ObjectId"));

				if (curValue.Equals(valueToFind))
					return obj;
			}

			return null;
		} 
		#endregion

	}
}
