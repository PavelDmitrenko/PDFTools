using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Encoder = System.Drawing.Imaging.Encoder;
using NetImage = System.Drawing.Image;

namespace PDFTools
{

	public class ConvertToTiff: IImageConverter
	{

		#region Convert
		public byte[] Convert(byte[] bmpBytes)
		{
			byte[] result = null;

			ImageCodecInfo encoderInfo = ImageCodecInfo.GetImageEncoders().First(i => i.MimeType == "image/tiff");
			EncoderParameters encoderParams = new EncoderParameters(2);
			EncoderParameter parameter = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionCCITT3);
			encoderParams.Param[0] = parameter;

			parameter = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
			encoderParams.Param[1] = parameter;

			using (MemoryStream inStream = new MemoryStream(bmpBytes))
			using (MemoryStream outStream = new MemoryStream())
			{
				using (NetImage tiff = NetImage.FromStream(inStream))
				{
					tiff.Save(outStream, encoderInfo, encoderParams);
				}
				result = outStream.ToArray();
			}

			return result;

		} 
		#endregion

	}
}
