using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PDFTools
{

	[DebuggerDisplay("AnyText={AnyText}")]
	public class TextsInfo
	{
		public string Text => _GetText();
		public bool AnyText => _GetText().Length != 0;

		private readonly List<TextInfo> _items;

		#region ctor
		public TextsInfo()
		{
			_items = new List<TextInfo>();
		}
		#endregion

		#region Add
		public void Add(TextInfo item)
		{
			_items.Add(item);
		}
		#endregion

		#region _GetText
		private string _GetText()
		{
			StringBuilder result = new StringBuilder();
			_items.ForEach(x => { result.Append(x.Text); });
			return result.ToString();
		} 
		#endregion

	}
}
