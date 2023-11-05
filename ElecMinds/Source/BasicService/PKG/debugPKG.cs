using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PKG
{
	public static class Logger
	{
		public static void WriteLine<T>(T message)
		{
#if DEBUG
			Debug.WriteLine(message); // 在 Debug 模式下使用 Debug.WriteLine
#else
		Console.WriteLine(message); // 在 Release 模式下使用 Console.WriteLine
#endif
		}
	}

}
/* usage
public static PKG.debugPKG.Log log = PKG.debugPKG.debugLog;
 */