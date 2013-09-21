using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Masa.Lib
{
	public static class XMLUtility
	{
		public static int IntValue(this XElement element)
		{
			return int.Parse(element.Value);
		}

		public static bool BoolValue(this XElement element)
		{
			return bool.Parse(element.Value);
		}
	}
}
