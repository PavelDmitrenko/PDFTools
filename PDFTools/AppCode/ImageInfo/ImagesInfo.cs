using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PDFTools
{
	public class ImagesInfo
	{
		public ReadOnlyCollection<ImageInfo> Items => _items.AsReadOnly();

		private readonly List<ImageInfo> _items;

		public int Count => _items.Count;
		public bool AnyImage => _items.Count !=0;

		#region ctor
		public ImagesInfo()
		{
			_items = new List<ImageInfo>();
		}
		#endregion

		#region Add
		public void Add(ImageInfo item)
		{
			_items.Add(item);
		}
		#endregion

	}
}
