using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.text.exceptions;
using NetImage = System.Drawing.Image;
using PdfImage = iTextSharp.text.Image;
using iTextSharp.text.pdf.parser;
using BitMiracle.LibTiff.Classic;

namespace PDFTools
{
	public class ImageInfo
	{
		public Guid ID { get; }
		public int WidthPx { get; }
		public int HeightPx { get; }
		public int PageNumber { get; }
		public float WidthMm { get; }
		public float HeightMm { get; }
		public float Dpix { get; }
		public float DpiY { get; }
		public bool IsImage { get; }
		public PixelFormat PixelFormat { get; }
		public int BitsPerPixel { get; }
		public ImageFilter ImageFilter { get; }
		public ImgFileType ImageFileType { get; }
		public bool IsMask { get; }

		public byte[] ImageBytes { get; }

		private readonly PdfObject _fltr;
	
		private readonly PdfDictionary _imgDictionary;
		private readonly Matrix _matrix;
		private readonly float _pageUnits;
		private readonly ImageRenderInfo _renderInfo;
		private readonly PRStream _stream;
		private readonly PRStream _maskStream;
		private readonly PdfDictionary _imgObject;

		#region ctor
		public ImageInfo(PdfObject pdfObject, Matrix matrix, float pageUnits, int pageNumber, ImageRenderInfo renderInfo)
		{
			IsImage = false;
			ID = Guid.NewGuid();

			PageNumber = pageNumber;

			_matrix = matrix;
			_pageUnits = pageUnits;
			_renderInfo = renderInfo;

			_imgObject = (PdfDictionary)PdfReader.GetPdfObject(pdfObject);

			PdfObject subType = _imgObject.Get(PdfName.SUBTYPE);
			if (subType == null)
				return;

			_stream = _imgObject as PRStream;
			_maskStream = (PRStream)_imgObject.GetAsStream(PdfName.MASK) ?? (PRStream)_imgObject.GetAsStream(PdfName.SMASK);

			IsMask = _maskStream != null;

			_fltr = _imgObject.Get(PdfName.FILTER);

			WidthPx = _imgObject.GetAsNumber(PdfName.WIDTH).IntValue;
			HeightPx = _imgObject.GetAsNumber(PdfName.HEIGHT).IntValue;
			BitsPerPixel = _imgObject.GetAsNumber(PdfName.BITSPERCOMPONENT).IntValue;

			PixelFormat = _DetectPixelFormat();
			ImageFilter = _DetectImageFilter();
			ImageFileType = _GetFileType();

			ImageBytes = _GetImageBytes();

			var ctmWidth = matrix[Matrix.I11];
			var ctmHeight = matrix[Matrix.I22];

			var ImgSize = new SizeF(WidthPx, HeightPx);
			var CtmSize = new SizeF(ctmWidth, ctmHeight);
			var ImgWidthScale = ImgSize.Width / CtmSize.Width;
			var ImgHeightScale = ImgSize.Height / CtmSize.Height;

			Dpix = ImgWidthScale * pageUnits;
			DpiY = ImgHeightScale * pageUnits;

			WidthMm = Utilities.PointsToMillimeters(ctmWidth);
			HeightMm = Utilities.PointsToMillimeters(ctmHeight);

			IsImage = true;
		}
		#endregion

		#region _GetImageBytes
		private byte[] _GetImageBytes()
		{
			return _GetBytesFromObject(_imgDictionary);
		}
		#endregion

		#region _PostProcessBytes
		private void _PostProcessBytes(ref byte[] objectBytes)
		{
			bool unsupportedBits = BitsPerPixel == 1 || BitsPerPixel == 4;

			if (unsupportedBits && (ImageFilter == ImageFilter.FlateDecode))
			{
				using (Bitmap bmp = new Bitmap(WidthPx, HeightPx, PixelFormat))
				{

					int length = (int)Math.Ceiling(WidthPx * BitsPerPixel / 8.0);
					var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, WidthPx, HeightPx), ImageLockMode.WriteOnly, PixelFormat);

					for (int i = 0; i < HeightPx; i++)
					{
						int offset = i * length;
						int scanOffset = i * bmpData.Stride;
						Marshal.Copy(objectBytes, offset, new IntPtr(bmpData.Scan0.ToInt32() + scanOffset), length);
					}

					bmp.UnlockBits(bmpData);

					objectBytes = _BitmapToBytes(bmp);

				}
			}
		} 
		#endregion

		#region _GetFileType
		private ImgFileType _GetFileType()
		{
			ImgFileType result = new ImgFileType();
			result = ImgFileType.Unknown;

			PdfImageObject imageObject = null;

			try
			{
				imageObject = IsMask ? new PdfImageObject(_maskStream) : new PdfImageObject(_stream);
			}
			catch (UnsupportedPdfException)
			{
				return ImgFileType.Bmp;
			}

			string fileType = imageObject.GetFileType().ToUpper();

			switch (fileType)
			{
				case "PNG":
					result = ImgFileType.PNG;
					break;

				case "JBIG2":
					result = ImgFileType.JBIG2;
					break;

				case "JP2":
					result = ImgFileType.JP2;
					break;

				case "JPG":
					result = ImgFileType.JPG;
					break;

				case "TIF":
					result = ImgFileType.TIF;
					break;
			}

			if (result == ImgFileType.Unknown
				&& (PixelFormat == PixelFormat.Format1bppIndexed || PixelFormat == PixelFormat.Format4bppIndexed))
			{
				result = ImgFileType.Bmp;
			}

			if (result == ImgFileType.Unknown)
				throw new Exception("Cant Get FileType");

			return result;
		} 
		#endregion

		#region _GetBytesFromObject
		private byte[] _GetBytesFromObject(PdfDictionary obj)
		{
			byte[] result = null;

			PRStream stream = IsMask ? _maskStream : _stream;

			switch (ImageFilter)
			{
				case ImageFilter.FlateDecode:
					try
					{
						var image = _renderInfo.GetImage();
						result = image.GetImageAsBytes();
					}
					catch (UnsupportedPdfException)
					{
						var bytes = PdfReader.GetStreamBytesRaw(stream);
						result = PdfReader.FlateDecode(bytes, true);
						_PostProcessBytes(ref result);
					}
					break;

				case ImageFilter.None:
				case ImageFilter.DCTDecode:
				case ImageFilter.JPXDECODE:
				case ImageFilter.JBIG2DECODE:
				case ImageFilter.ASCII85DECODE:
					if (BitsPerPixel == 1 || BitsPerPixel == 4)
					{
						result = PdfReader.GetStreamBytesRaw(stream);
					}
					else
					{
						PdfImageObject image = new PdfImageObject(stream);
						result = image.GetImageAsBytes();
						//System.IO.File.WriteAllBytes(@"c:\1.jpg", result);
					}
					break;

				case ImageFilter.CCITTFaxDecode:
					{
						try
						{
							var image = _renderInfo.GetImage();
							result = image.GetImageAsBytes();
						}
						catch (UnsupportedPdfException)
						{
							result = PdfReader.GetStreamBytesRaw(stream);
							result = _GetTiff(result);
						}
					}
					break;
			}

			if (result == null)
				throw new Exception("Cant Get Image Bytes");

			return result;
		} 
		#endregion

		#region _GetTiff
		private byte[] _GetTiff(byte[] inBytes)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				using (Tiff output = Tiff.ClientOpen("InMemory", "w", ms, new TiffStream()))
				{
					output.SetField(TiffTag.IMAGEWIDTH, WidthPx);
					output.SetField(TiffTag.IMAGELENGTH, HeightPx);
					output.SetField(TiffTag.COMPRESSION, Compression.CCITTFAX4);
					output.SetField(TiffTag.BITSPERSAMPLE, BitsPerPixel);
					output.SetField(TiffTag.SAMPLESPERPIXEL, 1);

					IntPtr pointer2 = Marshal.AllocHGlobal(inBytes.Length);
					Marshal.Copy(inBytes, 0, pointer2, inBytes.Length);
					output.WriteRawStrip(0, inBytes, inBytes.Length);
					output.Close();
				}
				return ms.GetBuffer();
			}
		} 
		#endregion

		#region _BitmapToBytes
		private byte[] _BitmapToBytes(Bitmap bmp)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				bmp.Save(ms, ImageFormat.Bmp);
				return ms.ToArray();
			}
		} 
		#endregion

		#region _DetectPixelFormat
		private PixelFormat _DetectPixelFormat()
		{
			PixelFormat result = PixelFormat.Undefined;

			switch (BitsPerPixel)
			{
				case 1:
					result = PixelFormat.Format1bppIndexed;
					break;
				case 4:
					result = PixelFormat.Format4bppIndexed;
					break;
				case 8:
					result = PixelFormat.Format8bppIndexed;
					break;
				case 24:
					result = PixelFormat.Format24bppRgb;
					break;
				default:
					throw new Exception("Unknown Pixel Format");
			}

			return result;
		}
		#endregion

		#region _DetectImageFilter
		private ImageFilter _DetectImageFilter()
		{
			ImageFilter result = ImageFilter.Unknown;
			PdfObject value = _fltr;

			if (_fltr == null)
				return ImageFilter.None;

			if (_fltr is PdfArray)
			{
				value = ((PdfArray)_fltr).First();
			}

			if (value.Equals(PdfName.FLATEDECODE))
			{
				result = ImageFilter.FlateDecode;
			}
			else if (value.Equals(PdfName.DCTDECODE))
			{
				result = ImageFilter.DCTDecode;
			}
			else if (value.Equals(PdfName.CCITTFAXDECODE))
			{
				result = ImageFilter.CCITTFaxDecode;
			}
			else if (value.Equals(PdfName.JBIG2DECODE))
			{
				result = ImageFilter.JBIG2DECODE;
			}
			else if (value.Equals(PdfName.JPXDECODE))
			{
				result = ImageFilter.JPXDECODE;
			}
			else if (value.Equals(PdfName.ASCII85DECODE))
			{
				result = ImageFilter.ASCII85DECODE;
			}

			if (result == ImageFilter.Unknown)
				throw new Exception("Unknown Pixel Format");

			return result;
		}
		#endregion

	}

}