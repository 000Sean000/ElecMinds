using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKG
{
	/// <summary>
	/// an Object can return a string corresponding to current language setting
	/// </summary>
	public class language
	{
		/// <summary>
		/// language constant key string
		/// </summary>
		public const string TW = "Traditional Chinese";
		public const string CH = "Simplified Chinese";
		public const string US = "American English";
		public const string UN = "Unified";
		public static List<string> modes = new List<string>()
		{
			TW, CH, US, UN
		};
		public enum languageModes {
			TW, CH, US, UN
		}

		public static string mode = UN;
		protected Dictionary<string, string> _textDic;

		public language(string str) 
		{
			_textDic = new Dictionary<string, string>();
			_textDic[UN] = str;
		}
		public string Get()
		{
			if (_textDic.Keys.Contains(mode) && _textDic[mode] != null)
			{
				return _textDic[mode];
			}
			else
			{
				return "(UN) " + _textDic[UN];
			}
		}
		public void Set(string mode, string str) 
		{
			if (modes.Contains(mode)) { _textDic[mode] = str; }
			else { Logger.WriteLine("wrong language mode!"); }
		}
		public void SetTW(string str) 
		{ 
			_textDic[TW] = str; 
		}
		public void SetUS(string str)
		{
			_textDic[US] = str;
		}



	}
}
