using System.IO;
using System.Windows.Forms;

/* usage
ControlTextWriter writer = new ControlTextWriter(target_widget);
Console.SetOut(writer); // 重定向标准输出
Console.SetError(writer); // 重定向标准错误输出 
 */


namespace PKG
{

	public class ControlTextWriter : TextWriter
	{
		private Control control;

		public ControlTextWriter(Control control)
		{
			this.control = control;
		}

		public override void Write(char value)
		{
			control.Invoke((MethodInvoker)(() => control.Text += value));
		}

		public override void Write(string value)
		{
			control.Invoke((MethodInvoker)(() => control.Text += value));
		}

		public override void WriteLine(string value)
		{
			control.Invoke((MethodInvoker)(() => control.Text += value + Environment.NewLine));
		}

		public override System.Text.Encoding Encoding => System.Text.Encoding.Default;


	}


}
