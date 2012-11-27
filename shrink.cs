using System;
using Bricksoft.PowerCode;

namespace Bricksoft.DosToys
{
	public class shrink
	{
		public static void Main( string[] args )
		{
			// Adjust the window size.
			Console.WindowWidth = 80;
			Console.WindowHeight = 25;

			Console.BufferWidth = 80;
			Console.BufferHeight = 3000;

			ConsoleUtils.CenterWindow();
		}
	}
}
