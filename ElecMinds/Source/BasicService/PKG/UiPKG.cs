using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
/*
 *  Control Extension
 */

/*
* To prevent recursion of accessor:
* Define the direction of calling and forbit reverse direction calling.
* 
* 1. Let object accessor call flag accessor
*	e.g. GroupRoot = someControl call IsGroupRoot
* 

* 2. disable of parent don't modify child enable
*	=> refresh parent at child setter, encapsulate refresh 
*	(e.g. EnableFlex = EnableDrag || EnableResize)
*	let setter call getter, getter don't call setter
*	
* 3. State flag accessor e.g.  IsRootOpen = logic result of other flags
*	Let parent flag accessor getter refresh itself according to child flag,
*    And let getter call setter, setter don't call getter
*	(Letting child flag accessor setter refresh parent will repeat many same refresh action)
*	
*/

namespace PKG
{
	public static class UiPKG
	{
		// Replace oldControl with newControl in parentControl
		public static void ReplaceControl(Control oldControl, Control newControl, Control parentControl)
		{
			// Step 1: Find the index of oldControl in the Panel's Controls collection
			int index = parentControl.Controls.IndexOf(oldControl);
			// Step 3: Remove oldControl from the Controls collection
			parentControl.Controls.Remove(oldControl);
			// Step 2: Insert newControl at the same index as oldControl
			parentControl.Controls.Add(newControl);
			parentControl.Controls.SetChildIndex(newControl, index);

			
		}
	}



	public static class ControlAligner
	{
		public static int GetTotalWidth_OfControlsInContainer(Panel panel, int spacing = 0)
		{
			int totalWidth = -spacing;
			foreach (Control control in panel.Controls)
			{
				totalWidth += control.Width + spacing;
			}
			return totalWidth;
		}
		public static int GetTotalHeight_OfControlsInContainer(Panel panel, int spacing = 0)
		{
			int totalHeight = -spacing;
			foreach (Control control in panel.Controls)
			{
				totalHeight += control.Height + spacing;
			}
			return totalHeight;
		}
		public static int GetMaxWidth_OfControlsInContainer(Panel panel)
		{
			int maxWidth = 0;
			foreach (Control control in panel.Controls)
			{
				if(control.Width > maxWidth)
				{
					maxWidth = control.Width;
				}
			}
			return maxWidth;
		}
		public static int GetMaxHeight_OfControlsInContainer(Panel panel)
		{
			int maxHeight = 0;
			foreach (Control control in panel.Controls)
			{
				if (control.Height > maxHeight)
				{
					maxHeight = control.Height;
				}
			}
			return maxHeight;
		}
		public static void AlignControlsHorizontally(Panel panel, int padding = 0, int spacing = 0)
		{
			//panel.Height = GetMaxHeightOfControlsInContainer(panel);
			panel.Size = new Size(GetTotalWidth_OfControlsInContainer(panel, spacing) + 2 * padding, GetMaxHeight_OfControlsInContainer(panel) + 2 * padding);

			int centerY = panel.Height / 2;
			int currentX = padding;
			foreach (Control control in panel.Controls)
			{
				int controlX = currentX;
				int controlY = centerY - (control.Height / 2);

				control.Location = new Point(controlX, controlY);
				currentX += control.Width + spacing;
			}
		}

		public static void AlignControlsVertically(Panel panel, int padding = 0, int spacing = 0)
		{
			//panel.Width = GetMaxWidthOfControlsInContainer(panel);
			panel.Size = new Size(GetMaxWidth_OfControlsInContainer(panel) + 2 * padding, GetTotalHeight_OfControlsInContainer(panel, spacing) + 2 * padding);

			int centerX = panel.Width / 2;
			int currentY = padding;

			foreach (Control control in panel.Controls)
			{
				int controlX = centerX - (control.Width / 2);
				int controlY = currentY;

				control.Location = new Point(controlX, controlY);
				currentY += control.Height + spacing;
			}
		}
		public enum Alignment
		{
			Horizontal,
			Vertical,
			Center
		}

		public static void AlignControls(Control container, Alignment alignment)
		{
			int maxControlWidth = 0;
			int totalControlHeight = 0;

			foreach (Control control in container.Controls)
			{
				maxControlWidth = Math.Max(maxControlWidth, control.Width);
				totalControlHeight += control.Height;
			}

			int y = 0;

			foreach (Control control in container.Controls)
			{
				switch (alignment)
				{
					case Alignment.Horizontal:
						control.Location = new Point(0, y);
						control.Anchor = AnchorStyles.Left | AnchorStyles.Top;
						break;
					case Alignment.Vertical:
						control.Location = new Point((maxControlWidth - control.Width) / 2, y);
						control.Anchor = AnchorStyles.Left | AnchorStyles.Top;
						break;
					case Alignment.Center:
						control.Location = new Point((container.Width - control.Width) / 2, y);
						control.Anchor = AnchorStyles.Left | AnchorStyles.Top;
						break;
				}

				y += control.Height;
			}
		}

	}

	// AutoResize when text changed
	
/*
	 * Remember to set property "AutoSize" of the control to False!!!!!
	 * Do not put the control in "table layout panel", it will work badly
	 * It is better to put the control in a "panel"
	 */
	public class ExTextBox : ExControl
	{
		private TextBox textBox;
		private int basicWidth;
		private int basicHeight;
		public int space = 10; // reserve some space for correct resizing

		public ExTextBox(TextBox textBox, Form rootForm_, int basicWidth = 50, int basicHeight = 0) : base(textBox, rootForm_)
		{
			this.textBox = textBox;
			this.basicWidth = basicWidth;
			this.basicHeight = Math.Max(basicHeight, textBox.Font.Height);
			textBox.Multiline = true;
			// Subscribe to the TextChanged event of the TextBox
			textBox.TextChanged += TextBox_TextChanged;

			// Call the initial resize to adjust the size based on the initial text content
			AutoResizeTextBox();
		}

		private void TextBox_TextChanged(object sender, EventArgs e)
		{
			AutoResizeTextBox();
		}

		private void AutoResizeTextBox()
		{
			// Create a temporary Graphics object to measure the text size
			using (Graphics g = textBox.CreateGraphics())
			{
				SizeF textSize = g.MeasureString(textBox.Text, textBox.Font);

				int newWidth = Math.Max((int)textSize.Width + space, basicWidth);  // Add some padding

				// Calculate the new height based on the number of lines
				int lines = textBox.GetLineFromCharIndex(textBox.TextLength) + 1;
				int newHeight = Math.Max(lines * textBox.Font.Height + 5, basicHeight);

				// Update the TextBox size if it needs to be adjusted
				if (textBox.Width != newWidth || textBox.Height != newHeight)
				{
					textBox.Size = new Size(newWidth, newHeight);
				}
			}
		}
	}
	public class ExPictureBox:ExControl
	{
		PictureBox pictureBox;
		public ExPictureBox(PictureBox pictureBox_, Form rootForm_) : base(pictureBox_, rootForm_)
		{
			pictureBox = pictureBox_;
			SizeRatio = ((float)pictureBox.Image.Size.Height / (float)pictureBox.Image.Size.Width);
			///EnableRatioFixed = true;
			control.AutoSize = false;
			FitBoxToZoomedImage(true);
		}
		public void StretchWidth()
		{
			pictureBox.Size = new Size((int)(pictureBox.Size.Height / SizeRatio), (int)(pictureBox.Size.Height));
		}
		public void StretchHeight()
		{
			pictureBox.Size = new Size((int)(pictureBox.Size.Width), (int)(pictureBox.Size.Width * SizeRatio));
		}
		public void FitBoxToZoomedImage(bool byShrink = true)
		{
			if (byShrink)
			{
				// shrink pictureBox to match imageRatio(eliminate rundent space)
				if (pictureBox.Size.Height > pictureBox.Size.Width * SizeRatio) // current height > expected height
				{
					StretchHeight();
				}
				else
				{
					StretchWidth();
				}
			}
			else
			{
				// grow pictureBox to match imageRatio
				if (pictureBox.Size.Height < pictureBox.Size.Width * SizeRatio) // current height > expected height
				{
					StretchHeight();
				}
				else
				{
					StretchWidth();
				}
			}
		}
	}
	public class ExControl
	{
		#region Debug Log
		public void FlexDebugLog(string message) { Debug.WriteLine(message); }
		public void Watch_FunctionalityEnable()
		{
			Debug.WriteLine($"\t>>CheckFunctionalityEnable");
			Debug.WriteLine($"\tEnableGroup:{EnableGroup}");
			Debug.WriteLine($"\tIsGroupRoot:{IsGroupRoot}");
			Debug.WriteLine($"\tEnableFlex:{EnableFlex}");
			Debug.WriteLine($"\tEnableDrag:{EnableDrag}");
			Debug.WriteLine($"\tEnableResize:{EnableResize}");
			Debug.WriteLine($"\tEnableRatioFixed:{EnableRatioFixed}");
		}
		public void Watch_GroupState() 
		{ 
			if (IsGroupRoot)
			{
				Debug.WriteLine($"\t{control.Name} GroupState: {GroupState}");
			}
			Debug.WriteLine($"\t{control.Name} IsRootOpen: {IsRootOpen}");
		}
		public void Watch_RootGroupState()
		{
			Debug.WriteLine($"\t{GroupRoot.control.Name} GroupState: {GroupState}");
			Debug.WriteLine($"\t{control.Name} IsRootOpen: {IsRootOpen}");
		}
		public void Watch_ControlFocused() { Debug.WriteLine($"\t\t{control.Name} Focused: {control.Focused}"); }
		public void Watch_IsParentNull() { }/// Debug.WriteLine($"\tparent is null: {control.Name}"); }
		public void Watch_OriginalCursor() { }/// Debug.WriteLine($"{control.Name} originalCursor: {originalCursor}"); }
		public void Watch_GroupHandlerSubscription() { } /// Debug.WriteLine($"\tGroup Extension Subscription of {control.Name}"); }
		public void Watch_GroupHanderUnsubscription() { } /// Debug.WriteLine($"\tGroup Extension Unsubscription of {control.Name}"); }
		public void Watch_Capture() { }/// Debug.WriteLine($"{control.Name}.Capture: {control.Capture}"); }
		public void Watch_Click() { Debug.WriteLine($"\tclick on {control.Name}"); }
		public void Watch_ClickOnGroupRoot() { Debug.WriteLine($"\t\tself {control.Name}.Capture: {control.Capture}"); }
		public void Watch_ClickOnGroupMember() { Debug.WriteLine($"\t\troot {GroupRoot.control.Name}.Capture: {GroupRoot.control.Capture}"); }
		public void Watch_StartFlex() { Debug.WriteLine($"\t{control.Name} Start Flex at {SideLocation} side"); }
		public void Watch_FinishFlex() { Debug.WriteLine($"\t{control.Name} Finish Flex"); }
		public void Watch_Cursor() { }/// Debug.WriteLine($"{control.Name} Cursor: {control.Cursor}"); }
		public void Watch_Enter() { Debug.WriteLine($"\t>> Enter {control.Name}");  }
		public void Watch_ForceEnterRoot() { Debug.WriteLine($"\t>>> Force Enter {GroupRoot.control.Name}"); }
		public void Watch_ForceReEnterRoot() { Debug.WriteLine($"\t>>> Force Re-Enter {GroupRoot.control.Name}"); }
		///Debug.WriteLine($"\tEnableGroup:{EnableGroup}, GroupRoot == null:{GroupRoot == null}, IsGroupRoot:{IsGroupRoot}"); <summary>
		/// Debug.WriteLine($"\tEnableGroup:{EnableGroup}, GroupRoot == null:{GroupRoot == null}, IsGroupRoot:{IsGroupRoot}");			///Debug.WriteLine($"\tRefresh {control.Name} IsRootOpen to {_isRootOpen}");
		///Debug.WriteLine($"\t this is not groupRoot: {this}");
		///Debug.WriteLine($"\t({rect.Width} x {rect.Height}) at ({rect.X}, {rect.Y}) ");
		///
		#endregion
		#region Fields & Accessors
		public Control control;
		public Control parent; // parent control
		public Form rootForm; // ready to remove this property
		public ClickHandler clickHandler;
		#region Functionality Enable
		protected bool _enablePaintBorder;
		public bool EnablePaintBorder
		{
			set
			{
				_enablePaintBorder = value;
				if (_enablePaintBorder)
				{
					BorderColor = original_borderColor;
					Subscribe_PaintBorder_Handlers();
				}
				else
				{
					Unsubscribe_PaintBorder_Handlers();
				}
			}
			get
			{
				return _enablePaintBorder;
			}
		}
		protected bool _enableGroup;
		protected bool _enableFlex;
		public bool EnableGroup
		{
			set
			{
				_enableGroup = value;
				///Refresh_IsRootOpen();
				if (_enableGroup)
				{ 
					Subscribe_Group_Handlers();
					if (IsGroupRoot)
					{
						groupFSM = new FiniteStateMachine<GroupSt>(GroupSt.None);
						#region Default State Actions
						groupStateActions = new Dictionary<GroupSt, Action<object?>>();
						groupStateActions[GroupSt.None] = (obj) => { };
						groupStateActions[GroupSt.BeyondGroup] = (obj) => 
						{ 
							BorderColor = original_borderColor;
							control.Capture = false;
						};
						groupStateActions[GroupSt.AimingGroup] = (obj) => 
						{ 
							BorderColor = hover_borderColor;
							control.Capture = true;
						};
						groupStateActions[GroupSt.EditingGroup] = (obj) => 
						{ 
							BorderColor = rootOpen_borderColor; 
							control.Capture = false; 
						};
						foreach (GroupSt state in Enum.GetValues(typeof(GroupSt)).Cast<GroupSt>())
						{
							groupFSM.Subscribe_Actions(state, groupStateActions[state]);
						}
						#endregion
					}
				}
				else 
				{ 
					Unsubscribe_Group_Handlers();
					groupFSM = null;
				}
				/// no group hander to subscribe, only root need to subscribe GroupRootHanders
				/// 

			}
			get
			{
				return _enableGroup;
			}
		}
		public void Refresh_EnableGroup()
		{
			///EnableGroup = GroupRoot != null || GroupInnerBody != null; 
			EnableGroup = IsGroupRoot || GroupRoot != null;
		}
		public bool EnableFlex
		{
			set
			{
				_enableFlex = value;
				if (_enableFlex)
				{
					Subscribe_Flex_Handlers();
				}
				else
				{
					Unsubscribe_Flex_Handlers();
				}
			}
			get
			{
				return _enableFlex;
			}
		}
		public void Refresh_EnableFlex() { EnableFlex = EnableDrag || EnableResize; }
		public bool _enableDrag;
		public bool _enableResize;
		public bool _enableRatioFixed;
		public bool EnableDrag
		{
			set
			{
				_enableDrag = value;
				if (_enableDrag) Refresh_EnableFlex();
			}
			get { return _enableDrag; }
		}
		public bool EnableResize
		{
			set
			{
				_enableResize = value;
				if (_enableResize) Refresh_EnableFlex();
			}
			get 
			{
				return _enableResize; 
			}
		}
		public void Refresh_EnableResize() { EnableResize = EnableResize || EnableRatioFixed; }
		public bool EnableRatioFixed
		{
			set
			{
				Refresh_EnableResize();
				_enableRatioFixed = value;
			}
			get { return _enableRatioFixed; }
		}
		#endregion
		#region Z-Order Extension
		protected Control originalParent;
		protected int originalIndex;
		protected Form _parentForm;
		protected int originalZOrder;
		protected Point offset;
		///protected Control replacingControl = new Control();
		protected Label replacingControl = new Label();
		public Form ParentForm
		{
			set
			{
				_parentForm = value;
			}
			get
			{
				if (_parentForm == null)
				{
					_parentForm = control.FindForm();
				}
				return _parentForm;
			}
		}
		
		#endregion
		#region Appearance Extension
		public Color original_borderColor = Color.Black;
		public Color hover_borderColor = Color.Cyan;
		public Color rootOpen_borderColor = Color.Blue;
		protected Color _backColor = Color.Gray;
		protected Color _borderColor; // default border color
		protected int _borderThickness = 5;
		public Color BackColor
		{
			get { return _backColor; }
			set
			{
				_backColor = value;
				control.Invalidate(); // Trigger repaint
			}
		}
		public Color BorderColor
		{
			get { return _borderColor; }
			set
			{
				if (EnablePaintBorder)
				{
					_borderColor = value;
					control.Invalidate(); // Trigger repaint
				}
			}
		}

		public int BorderThickness
		{
			get { return _borderThickness; }
			set
			{
				_borderThickness = value;
				control.Invalidate(); // Trigger repaint
			}
		}

		#endregion
		#region Group Extension
		public enum GroupSt // Group States
		{
			BeyondGroup, AimingGroup, EditingGroup, None
		}
		public FiniteStateMachine<GroupSt> groupFSM;
		public Dictionary<GroupSt, Action<object?>> groupStateActions;
		
		
		protected GroupSt _groupState = GroupSt.None;
		public GroupSt GroupState
		{
			set
			{
				groupFSM.State = value;
			}
			get
			{
				return groupFSM.State;
			}
		}
		///public Action[] stateActions;
		public static bool[] captureWhen = new bool[Enum.GetValues(typeof(GroupSt)).Length];

		public ExControl? _groupRoot = null; // GroupRoot control of group container
		public ExControl? GroupRoot
		{
			set
			{
				_groupRoot = value;
				///Refresh_IsRootOpen();
				Refresh_EnableGroup();
				if (_groupRoot != null)
				{
					if (!_groupRoot.IsGroupRoot)
					{
						_groupRoot.IsGroupRoot = true;
					}
				}
				
			}
			get { return _groupRoot; }
		}
		protected bool _isGroupRoot;
		protected bool _isRootOpen;
		public bool IsGroupRoot
		{
			set
			{
				_isGroupRoot = value;
				Refresh_EnableGroup();
				if (_isGroupRoot && GroupState == GroupSt.None) // init group state
					{ GroupState = GroupSt.BeyondGroup; }
				///Refresh_IsRootOpen();
			}
			get { return _isGroupRoot; }
		}
		public void Refresh_IsRootOpen()
		{
			if (!EnableGroup || GroupRoot == null || IsGroupRoot)
			{
				
				IsRootOpen = true;
			}
			else
			{
				if (GroupRoot.GroupState == GroupSt.EditingGroup) { IsRootOpen = true; }
				else { IsRootOpen = false; }
			}

		}
		public bool IsRootOpen // the group is being selected
		{
			set
			{
				_isRootOpen = value;
			}
			get
			{
				Refresh_IsRootOpen(); 
				return _isRootOpen;
			}
		}
		protected bool _isControlFocused;
		#endregion
		#region Flex Extension
		public Action ActionAfterFlex = () => { }; // Alignment after Flex action
		public enum Side
		{
			N, S, W, E, None
		}
		protected Cursor originalCursor;
		protected int _borderSize; // border BorderSize
		public int BorderSize
		{
			set
			{
				_borderSize = value;
				DefineBasicSize();
			}
			get { return _borderSize; }
		}

		protected float _sizeRatio;
		public float SizeRatio
		{
			set
			{
				_sizeRatio = value;
				DefineBasicSize();
			}
			get { return _sizeRatio; }
		}
		protected int basicWidth;
		protected int basicHeight;
		protected Rectangle wholeRegion;
		protected Rectangle borderN;
		protected Rectangle borderS;
		protected Rectangle borderW;
		protected Rectangle borderE;
		// flags
		protected bool isResizing = false;
		protected bool isDragging = false;
		// calculation variables
		protected bool[] isAtSide = new bool[Enum.GetValues(typeof(Side)).Length];
		protected bool isAtCorner;
		public string AtSide
		{
			get
			{
				string side = "";
				Side[] sides = (Side[])Enum.GetValues(typeof(Side));
				foreach (Side s in sides)
				{
					if (isAtSide[(int)s])
					{
						side += s.ToString();
					}
				}

				return side;
			}
		}
		protected Point preCursorPoint; // previous cursor point
		protected Point dragingStartPoint;
		protected int verticalMove, horizontalMove; // mouse movement
		protected int preWidth, preHeight; // previous W/H
		protected int widthDiff, heightDiff; // difference

		#endregion

		#endregion

		#region Constructor
		static ExControl()
		{
			captureWhen[(int)GroupSt.BeyondGroup] = false;
			captureWhen[(int)GroupSt.AimingGroup] = true;
			captureWhen[(int)GroupSt.EditingGroup] = false;
		}
		public ExControl(Control control_, Form rootForm_)
		{
			
			control = control_;
			rootForm = rootForm_;
			init();
		}
		#region Constructors with: "SizeRatio = ...", ".AutoSize = false"
		public ExControl(Label control_, Form rootForm_)
		{
			control = control_;
			rootForm = rootForm_;
			control.AutoSize = false;
			init();
		}
		#endregion
		public void init()
		{
			/* Disable all
			EnablePaintBorder = false;
			EnableGroup = false;
			IsGroupRoot = false;
			EnableFlex = false;
			EnableDrag = false;
			EnableResize = false;
			EnableRatioFixed = false;
			*/
			parent = control.Parent;
			if (parent == null) {  }
			originalCursor = new Cursor(control.Cursor.Handle);
			Watch_OriginalCursor();
			BorderSize = 8;

			clickHandler = new ClickHandler(control, 2);
			control.MouseDown += clickHandler.MouseDown;
			control.MouseUp += clickHandler.MouseUp;
		}
		public void DefineBasicSize()
		{
			basicWidth = 3 * BorderSize;
			basicHeight = 3 * BorderSize;
			if (control.MinimumSize.Width > basicWidth)
			{
				basicWidth = control.MinimumSize.Width;
			}
			if (control.MinimumSize.Height > basicHeight)
			{
				basicHeight = control.MinimumSize.Height;
			}
			if (EnableRatioFixed) // maintain ratio policy: only growth
			{
				int expectedBasicHeight = (int)((float)basicWidth * SizeRatio);
				int expectedBasicWidth = (int)((float)basicHeight / SizeRatio);
				if (SizeRatio > 1)
				{
					if (basicHeight < expectedBasicHeight)
					{
						basicHeight = expectedBasicHeight;
					}
					else
					{
						basicWidth = expectedBasicWidth;
					}
				}
				else if (SizeRatio <= 1)
				{
					if (basicWidth < expectedBasicWidth)
					{
						basicWidth = expectedBasicWidth;
					}
					else
					{
						basicHeight = expectedBasicHeight;
					}
				}
			}
			control.MinimumSize = new Size(basicWidth, basicHeight);
			DefineBorder();
		}
		#endregion
		#region Z-Order Extension
		public void BringToFrontOfForm()
		{
			///return;
			// Save the original parent and index
			originalParent = parent;
			originalIndex = parent.Controls.GetChildIndex(control);
			offset = control.PointToScreen(Point.Empty) - (Size)ParentForm.PointToScreen(Point.Empty);
			// Replace the control from its current parent
			replacingControl.Location = control.Location;
			replacingControl.Size = control.Size;
			//* debug view
			replacingControl.Text = "Replacing Control";
			replacingControl.BorderStyle = BorderStyle.FixedSingle; 
			replacingControl.BackColor = Color.Gray;
			//*/
			UiPKG.ReplaceControl(control, replacingControl, parent);

			// Add the control to the form's controls collection to bring it to the front
			if (ParentForm != null)
			{
				control.Location = offset;
				ParentForm.Controls.Add(control);
				control.BringToFront();
			}

			// Optionally, adjust the control's location or perform any other adjustments
		}

		public void ReturnToOriginalParent()
		{
			///return;
			// Replace the control from the form's controls collection
			UiPKG.ReplaceControl(replacingControl, control, parent);

			// Add the control back to its original parent at its original index
			originalParent.Controls.Add(control);
			originalParent.Controls.SetChildIndex(control, originalIndex);

			// Optionally, adjust the control's location or perform any other adjustments
		}
		#endregion

		#region Appearance Extension
		public void Subscribe_PaintBorder_Handlers()
		{
			Unsubscribe_PaintBorder_Handlers();
			control.Paint += Appearance_Paint;
			control.SizeChanged += Appearance_SizeChanged;
		}
		public void Unsubscribe_PaintBorder_Handlers()
		{
			control.Paint -= Appearance_Paint;
			control.SizeChanged -= Appearance_SizeChanged;
		}
		public void DrawBorder(PaintEventArgs e)
		{
			_borderThickness = 1;
			// Draw the custom border using the specified color and thickness
			using (var pen = new Pen(BorderColor, BorderThickness))
			{
				int shift = 3; // error shift
				Rectangle borderRect = new Rectangle(0, 0, control.Width - shift, control.Height - shift);
				e.Graphics.DrawRectangle(pen, borderRect);
			}
		}

		protected void DrawBackground(PaintEventArgs e)
		{

			using (var backBrush = new SolidBrush(BackColor))
			{
				Rectangle rect = control.ClientRectangle;
				e.Graphics.FillRectangle(backBrush, rect);
			}
			///if (_backColor != Color.Gray) { _backColor = Color.Gray; }
			///else { _backColor = Color.Blue; }
		}
		private void Appearance_SizeChanged(object sender, EventArgs e)
		{
			control.Invalidate();
		}


		// Pass the Paint event to draw border and background
		public void Appearance_Paint(object sender, PaintEventArgs e)
		{
			DrawBackground(e);
			DrawBorder(e);
		}
		private void PaintBorder(object sender, PaintEventArgs e)
		{

			// Draw the custom border using the specified color
			using (var pen = new Pen(BorderColor, BorderThickness))
			{
				int shift = 3; // error shift
				Rectangle borderRect = new Rectangle(0, 0, control.Width - shift, control.Height - shift);
				e.Graphics.DrawRectangle(pen, borderRect);
			}
			if (_borderColor == Color.Red) { _borderColor = Color.Green; }
			else { _borderColor = Color.Red; }
			
		}
		#endregion
		#region Group Extension
		public void Subscribe_Group_Handlers()
		{
			Unsubscribe_Group_Handlers();// prevent duplicated handler subscription
			control.MouseEnter += Group_MouseEnter;
			control.MouseMove += Group_MouseMove;
			Group_Subscribe_ClickActions();
			Watch_GroupHandlerSubscription();
		}
		public void Unsubscribe_Group_Handlers()
		{
			control.MouseEnter -= Group_MouseEnter;
			control.MouseMove -= Group_MouseMove;
			Group_Unsubscribe_ClickActions();
			Watch_GroupHanderUnsubscription();
		}
		public void Group_Subscribe_ClickActions()
		{
			clickHandler.SubscribeAction(1, Group_SingleClick);
		}
		public void Group_Unsubscribe_ClickActions()
		{
			clickHandler.UnsubscribeAction(1, Group_SingleClick);
		}
		#region Event Handlers
		public void Group_SingleClick()
		{
			if (IsGroupRoot)
			{
				if (GroupState == GroupSt.AimingGroup)
				{
					GroupState = GroupSt.EditingGroup;
				}
				else if (GroupState == GroupSt.EditingGroup)
				{
					GroupState = GroupSt.AimingGroup;
				}
			}
			else
			{
				if (GroupRoot != null)
				{
					if (GroupRoot.GroupState == GroupSt.AimingGroup)
					{
						GroupRoot.GroupState = GroupSt.EditingGroup;
					}
					else if (GroupRoot.GroupState == GroupSt.EditingGroup)
					{
						GroupRoot.GroupState = GroupSt.AimingGroup;
					}
				}
			}
		}
		public void Group_MouseEnter(object sender, EventArgs e)
		{
			Watch_Enter();
			///control.Focus();
			if (IsGroupRoot)
			{
				if (GroupState == GroupSt.BeyondGroup)
				{
					GroupState = GroupSt.AimingGroup; 
				}
				else if (GroupState == GroupSt.AimingGroup)
				{
					control.Capture = true; // hold ".Capture" being true, since Flex action will modify ".Capture"
				}
			}
			else
			{
				if (GroupRoot != null)
				{
					// the "Enter" event of a control is not being detected when actions are performed too quickly
					if (GroupRoot.GroupState == GroupSt.BeyondGroup)
					{
						Watch_ForceEnterRoot();
						GroupRoot.Group_MouseEnter(sender, e);
					}
					else if (GroupRoot.GroupState == GroupSt.AimingGroup)
					{
						Watch_ForceReEnterRoot();
						GroupRoot.Group_MouseEnter(sender, e);
					}
				}
				
			}
			
		}
		public void GroupRoot_MouseLeave(object sender, MouseEventArgs e)
		{
			if (GroupState == GroupSt.AimingGroup)
			{
				GroupState = GroupSt.BeyondGroup;
			}
		}
		// MouseLeave will not be triggered when capture is still true
		// so use MouseMove
		public void Group_MouseMove(object sender, MouseEventArgs e) 
		{
			if (IsGroupRoot)
			{
				if (GroupState == GroupSt.AimingGroup)
				{
					control.Capture = true; 
					// hold ".Capture" being true, since Flex action will modify ".Capture"
				}
				if (!control.ClientRectangle.Contains(e.Location)) // leave control
				{
					GroupRoot_MouseLeave(sender, e);
				}
			}
			else
			{
				if (GroupRoot != null)
				{
					if (GroupRoot.GroupState == GroupSt.AimingGroup)
					{
						//GroupRoot.control.Capture = true;
						// hold ".Capture" being true, since Flex action will modify ".Capture"
					}
					else if(GroupRoot.GroupState == GroupSt.EditingGroup)
					{
						if (!EnableFlex)
						{
							RecoverCursor();
						}
					}
				}
			}
		}
		#endregion
		#region Functions for Event Handlers
		
		#endregion
		#endregion
		#region Flex Extension
		public void Subscribe_Flex_Handlers()
		{
			Unsubscribe_Flex_Handlers();// prevent duplicated handler subscription
			control.MouseMove += Flex_MouseMove;
			control.MouseDown += Flex_MouseDown;
			control.MouseUp += Flex_MouseUp;
			control.MouseLeave += Flex_MouseLeave;
		}
		public void Unsubscribe_Flex_Handlers()
		{
			control.MouseMove -= Flex_MouseMove;
			control.MouseDown -= Flex_MouseDown;
			control.MouseUp -= Flex_MouseUp;
			control.MouseLeave -= Flex_MouseLeave;
		}
		#region Event Handlers
		protected virtual void Flex_MouseMove(object sender, MouseEventArgs e)
		{
			if (!IsRootOpen) { return; } 
			if (isResizing)
			{
				if (EnableResize)
				{
					// Calculate the size difference based on the mouse movement
					preWidth = control.Width;
					preHeight = control.Height;
					horizontalMove = e.X - preCursorPoint.X;
					verticalMove = e.Y - preCursorPoint.Y;
					CalculateSizeDifference();
					ResizeControl(e);
				}
			}
			else if (isDragging)
			{
				if (EnableDrag)
				{
					horizontalMove = e.X - dragingStartPoint.X;
					verticalMove = e.Y - dragingStartPoint.Y;
					DragControl(horizontalMove, verticalMove);
					//preCursorPoint = e.Location;
				}
			}
			else // just hovering
			{
				CheckCursorLocatingRegion(e.Location);
				ChangeCursorByRegion();
			}

		}
		protected void Flex_MouseDown(object sender, MouseEventArgs e)
		{
			if (!IsRootOpen) { return; }
			// Check if the mouse is near the border of the container
			if (EnableFlex)
			{
				Watch_GroupState();
				Watch_StartFlex();
				if (IsCursorNearBoundary())
				{
					isResizing = EnableResize;
					preCursorPoint = new Point(e.X, e.Y);
					BringToFrontOfForm();
				}
				else
				{
					isDragging = EnableDrag;
					dragingStartPoint = new Point(e.X, e.Y);
				}
			}
			

		}
		protected virtual void Flex_MouseUp(object sender, MouseEventArgs e)
		{
			if (!IsRootOpen) { return; }
			if (EnableFlex)
			{
				if(isResizing)
				{
					ReturnToOriginalParent();
				}
				ActionAfterFlex();
				isResizing = false;
				isDragging = false;
				DefineBorder();
				ChangeCursorByRegion();
				Watch_FinishFlex();
				Watch_GroupState();
			}
				
		}
		protected void Flex_MouseLeave(object sender, EventArgs e)
		{
			if (!IsRootOpen) { return; }
			//RecoverCursor();
		}
		#endregion
		#region Functions for Handlers
		public string SideLocation
		{
			get
			{
				string side = "";
				foreach (Side s in Enum.GetValues(typeof(Side)).Cast<Side>())
				{
					if (isAtSide[(int)s]) side += s.ToString();
				}
				if (side == "") side = "center";
				return side;
			}
		}
		public void DefineBorder()
		{
			wholeRegion = control.ClientRectangle;
			// Define the regions for each side of the wholeRegion
			borderN = new Rectangle(wholeRegion.Left, wholeRegion.Top, wholeRegion.Width, BorderSize);
			borderS = new Rectangle(wholeRegion.Left, wholeRegion.Bottom - BorderSize, wholeRegion.Width, BorderSize);
			borderW = new Rectangle(wholeRegion.Left, wholeRegion.Top, BorderSize, wholeRegion.Height);
			borderE = new Rectangle(wholeRegion.Right - BorderSize, wholeRegion.Top, BorderSize, wholeRegion.Height);
			
		}
		public void RecoverCursor()
		{
			control.Cursor = new Cursor(originalCursor.Handle);
			Watch_Cursor();
		}
		protected virtual void ResizeControl(MouseEventArgs e)
		{
			//  clear the value according to side condition
			horizontalMove *= Convert.ToInt32(isAtSide[(int)Side.W]);
			verticalMove  *= Convert.ToInt32(isAtSide[(int)Side.N]);
			widthDiff *= Convert.ToInt32(isAtSide[(int)Side.W] || isAtSide[(int)Side.E] || EnableRatioFixed);
			heightDiff *= Convert.ToInt32(isAtSide[(int)Side.N] || isAtSide[(int)Side.S] || EnableRatioFixed);
			FlexDebugLog($"\t--> move({horizontalMove}, {verticalMove}), size({widthDiff}, {heightDiff})");

			// Adjust the size of the resizable block control			
			// extend size of parent to suit current control
			//...
			Stack<Control> parentStack = new Stack<Control>();
			Stack<int> controlXStack = new Stack<int>();
			Stack<int> controlYStack = new Stack<int>();
			int controlX;
			int controlY;
			Control parent;
			int parentX;
			int parentY;

			///Debug.WriteLine($"control: {control.Name}");
			parent = control.Parent;
			controlX = control.Location.X + horizontalMove;
			controlY = control.Location.Y + verticalMove;
			while (parent != null && (controlX < parent.Padding.Left || controlY < parent.Padding.Top))
			{
				///Debug.WriteLine($"parent: {parent.Name}");
				parentStack.Push(parent);
				controlXStack.Push(controlX);
				controlYStack.Push(controlY);
				parentX = parent.Location.X;
				parentY = parent.Location.Y;
				if (controlX < parent.Padding.Left) { parentX += controlX - parent.Padding.Left; }
				if (controlY < parent.Padding.Top) { parentY += controlY - parent.Padding.Top; }

				controlX = parentX;
				controlY = parentY;
				parent = parent.Parent;
				
			}
			while (parentStack.Count > 0)
			{
				parent = parentStack.Pop();
				controlX = controlXStack.Pop();
				controlY = controlYStack.Pop();
				parentX = parent.Location.X;
				parentY = parent.Location.Y;
				if (controlX < parent.Padding.Left) { parentX += controlX - parent.Padding.Left; }
				if (controlY < parent.Padding.Top) { parentY += controlY - parent.Padding.Top; }
				parent.Location = new Point(parentX, parentY);
				parent.Width += controlX;
				parent.Height += controlY;
			}
			
			// Only update preCursorPosition when mousemove relates to S or E direction,
			int preCursorX = isAtSide[(int)Side.E] ? e.X : preCursorPoint.X;
			int preCursorY = isAtSide[(int)Side.S] ? e.Y : preCursorPoint.Y;

			// update control
			
			control.Location = new Point(controlX, controlY);
			control.Width += widthDiff;
			control.Height += heightDiff;
			preCursorPoint = new Point(preCursorX, preCursorY);
			FlexDebugLog($"\t=> to ({control.Location.X}, {control.Location.Y}) with ({control.Width} x {control.Height})\n");

		}
		protected virtual void CalculateSizeDifference()
		{
			FlexDebugLog($"mouse move ({horizontalMove}, {verticalMove}) at side {AtSide}");
			/* border near N or W need get diff by opposite move
			 * positive move => N/W:shrink,	S/E:grow
			 * negative move => N/W:grow,	S/E:shrink
			 */
			heightDiff = isAtSide[(int)Side.N] ? -verticalMove : verticalMove;
			widthDiff = isAtSide[(int)Side.W] ? -horizontalMove : horizontalMove;

			// check ratio and do calibration
			if (EnableRatioFixed)
			{
				int calibrated_heighDiff = (int)((float)(preWidth + widthDiff) * SizeRatio - (float)preHeight);
				int calibrated_widthDiff = (int)((float)(preHeight + heightDiff) / SizeRatio - (float)preWidth);
				if (isAtCorner) 
				{
					// handle the situation of one side diff nearing 0, remember to use absolute value to compare
					
					if (Math.Abs(calibrated_widthDiff) >= Math.Abs(calibrated_heighDiff))
					{
						heightDiff = calibrated_heighDiff;
					}
					else
					{
						widthDiff = calibrated_widthDiff;
					}
					if (isAtSide[(int)Side.W])
					{
						horizontalMove = -widthDiff;
					}
					if (isAtSide[(int)Side.N])
					{
						verticalMove = -heightDiff;
					}
				}
				else 
				{
					if (isAtSide[(int)Side.N] | isAtSide[(int)Side.S])
					{
						widthDiff = calibrated_widthDiff;
					}
					else if (isAtSide[(int)Side.W] | isAtSide[(int)Side.E])
					{
						heightDiff = calibrated_heighDiff;
					}
				}
			}
			
			// check basic Size
			if (preHeight + heightDiff <= control.MinimumSize.Height || preWidth + widthDiff <= control.MinimumSize.Width)
			{
				widthDiff = 0;
				heightDiff = 0;
				horizontalMove = 0;
				verticalMove = 0;
			}
		}
		protected virtual void DragControl(int horizontalMove, int verticalMove)
		{
			control.Location = new Point(control.Location.X + horizontalMove, control.Location.Y + verticalMove);
		}
		protected void CheckCursorLocatingRegion(Point cursorPosition)
		{
			isAtSide[(int)Side.N] = borderN.Contains(cursorPosition);
			isAtSide[(int)Side.S] = borderS.Contains(cursorPosition);
			isAtSide[(int)Side.W] = borderW.Contains(cursorPosition);
			isAtSide[(int)Side.E] = borderE.Contains(cursorPosition);
			isAtCorner = (Convert.ToInt32(isAtSide[(int)Side.N])
					+ Convert.ToInt32(isAtSide[(int)Side.S])
					+ Convert.ToInt32(isAtSide[(int)Side.W])
					+ Convert.ToInt32(isAtSide[(int)Side.E])) == 2;
			
		}
		protected void ChangeCursorByRegion()
		{
			
			if (!(isAtSide[(int)Side.N] | isAtSide[(int)Side.S] | isAtSide[(int)Side.W] | isAtSide[(int)Side.E]))
			{
				if (EnableDrag)
				{
					control.Cursor = Cursors.Hand;
				}
				else { RecoverCursor(); }
			}
			else
			{
				if (EnableResize)
				{
					if (isAtSide[(int)Side.N] & isAtSide[(int)Side.W])
					{
						control.Cursor = Cursors.SizeNWSE;
					}
					else if (isAtSide[(int)Side.N] & isAtSide[(int)Side.E])
					{
						control.Cursor = Cursors.SizeNESW;
					}
					else if (isAtSide[(int)Side.S] & isAtSide[(int)Side.W])
					{
						control.Cursor = Cursors.SizeNESW;
					}
					else if (isAtSide[(int)Side.S] & isAtSide[(int)Side.E])
					{
						control.Cursor = Cursors.SizeNWSE;
					}
					else
					{
						if (isAtSide[(int)Side.N])
						{
							control.Cursor = Cursors.SizeNS;
						}
						else if (isAtSide[(int)Side.S])
						{
							control.Cursor = Cursors.SizeNS;
						}
						else if (isAtSide[(int)Side.W])
						{
							control.Cursor = Cursors.SizeWE;
						}
						else if (isAtSide[(int)Side.E])
						{
							control.Cursor = Cursors.SizeWE;
						}
					}
				}
				else
				{
					RecoverCursor();
				}
			}

			
		}
		protected bool IsCursorNearBoundary()
		{
			return isAtSide[(int)Side.N] | isAtSide[(int)Side.S] | isAtSide[(int)Side.W] | isAtSide[(int)Side.E];
		}
		#endregion
		#endregion
	}
	public class ExPanel
	{
		protected Point _dragStartPoint;
		protected Point _canvasStartPoint;
		protected bool _isDragging;
		protected bool _readyToDraw = false;
		protected float _sensitivity;

		public Panel panel;
		public Label canvasX = new Label();
		public Label canvasY = new Label();
		public Label canvasX0 = new Label();
		public Label canvasY0 = new Label();
		public Label mouseX = new Label();
		public Label mouseY = new Label();
		public Label mouseX0 = new Label();
		public Label mouseY0 = new Label();
		public ExPanel(Panel panel, float sensitivity = 1)
		{
			this.panel = panel;
			panel.Capture = true;
			_sensitivity = sensitivity;
			panel.MouseDown += MouseDown;
			panel.MouseUp += MouseUp;
			panel.MouseMove += MouseMove;
			// Subscribe the AdjustWorldSpaceSize method to the appropriate events
			///panel.ControlAdded += AdjustWorldSpaceSize;
			///panel.ControlRemoved += AdjustWorldSpaceSize;
		}
		public void AdjustWorldSpaceSize(object sender, EventArgs e)
		{
			// Calculate the minimum required size for the world space
			int minWidth = 0;
			int minHeight = 0;
			int offsetX = int.MaxValue;
			int offsetY = int.MaxValue;

			foreach (Control childControl in panel.Controls)
			{
				// Subscribe to the LocationChanged event for each child control
				childControl.LocationChanged -= AdjustWorldSpaceSize;
				childControl.LocationChanged += AdjustWorldSpaceSize;

				// Subscribe to the SizeChanged event for each child control
				childControl.SizeChanged -= AdjustWorldSpaceSize;
				childControl.SizeChanged += AdjustWorldSpaceSize;
				// Adjust the required width and height based on the child control's position and size
				minWidth = Math.Max(minWidth, childControl.Right);
				minHeight = Math.Max(minHeight, childControl.Bottom);

				// Track the minimum negative X and Y coordinates
				offsetX = Math.Min(offsetX, childControl.Left);
				offsetY = Math.Min(offsetY, childControl.Top);
			}

			// Adjust the minimum required size for the world space based on negative offsets
			minWidth -= offsetX;
			minHeight -= offsetY;

			// Set the minimum required size for the world space
			panel.AutoScrollMinSize = new Size(minWidth, minHeight);
			panel.AutoScrollPosition = new Point(-offsetX, -offsetY);
			//Debug.WriteLine($"autoscroll: {panel.AutoScroll}");
		}
		public void MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				_dragStartPoint = new Point(e.X, e.Y);
				_isDragging = true;


				Point currentPosition = panel.AutoScrollPosition;
				_canvasStartPoint = new Point(currentPosition.X, currentPosition.Y);
				canvasX0.Text = currentPosition.X.ToString();
				canvasY0.Text = currentPosition.Y.ToString();
				mouseX.Text = e.X.ToString();
				mouseY.Text = e.Y.ToString();
				mouseX0.Text = _dragStartPoint.X.ToString();
				mouseY0.Text = _dragStartPoint.Y.ToString();
			}
		}

		public void MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				_isDragging = false;
				/*
				Point currentPosition = panel.AutoScrollPosition;
				panel.AutoScrollPosition = new Point(
					-_canvasStartPoint.X - (int)((e.X - _dragStartPoint.X) * 1),
					-_canvasStartPoint.Y - (int)((e.Y - _dragStartPoint.Y) * 1));
				currentPosition = panel.AutoScrollPosition; // read the AutoScrollPosition again
				canvasX.Text = currentPosition.X.ToString();
				canvasY.Text = currentPosition.Y.ToString();
				*/
			}
		}

		public void MouseMove(object sender, MouseEventArgs e)
		{
			if (_isDragging)
			{

				panel.AutoScrollPosition = new Point(
					-_canvasStartPoint.X - (int)((e.X - _dragStartPoint.X) * _sensitivity),
					-_canvasStartPoint.Y - (int)((e.Y - _dragStartPoint.Y) * _sensitivity));
				Point currentPosition = panel.AutoScrollPosition;
				canvasX.Text = currentPosition.X.ToString();
				canvasY.Text = currentPosition.Y.ToString();
				mouseX.Text = e.X.ToString();
				mouseY.Text = e.Y.ToString();


			}
		}
	}
	/*
		can solve: the first click on a control is treated as "focus" instead of Click
	*/
	public class ClickHandler
	{
		public Control control;
		protected int clickCount = 0;
		protected int maxClickCount;
		protected bool isCounting;
		protected System.Threading.Timer clickTimer;
		public object? state = null; // additional info for Callback function
		public int dueTime = Timeout.Infinite; // initial delay after creation of timer
		public int period = Timeout.Infinite; // repeat period
		public int clickInterval = 200; // mili second , Ui will response after this latency, so don't assign a too big value
		public Action[] actions;


		public void Watch(string msg)
		{
			Debug.WriteLine(msg);
		}

		public ClickHandler(Control control_, int maxClickCount_ = 2)
		{
			control = control_;
			clickTimer = new System.Threading.Timer(ClickTimerCallback, state, dueTime, period);
			maxClickCount = maxClickCount_;
			actions = new Action[maxClickCount];
			// Initialize each element with an empty Action delegate
			for (int i = 0; i < actions.Length; i++)
			{
				actions[i] = () => { }; // Empty Action delegate
			}
		}
		public void SubscribeAction(int clickCount_, Action action)
		{
			if (clickCount_ <= maxClickCount)
			{
				actions[clickCount_ - 1] -= action;
				actions[clickCount_ - 1] += action;
			}
			else
			{
				new Exception("over index range of array actions");
			}
		}
		public void UnsubscribeAction(int clickCount_, Action action)
		{
			if (clickCount_ <= maxClickCount)
			{
				actions[clickCount_ - 1] -= action;
			}
			else
			{
				new Exception("over index range of array actions");
			}
		}
		public void MouseDown(object sender, EventArgs e)
		{
			if (!isCounting)
			{
				Watch("start counting click");
				isCounting = true;
				clickTimer.Change(clickInterval, period); // change
			}
		}
		public void MouseUp(object sender, EventArgs e)
		{
			if (isCounting)
			{
				Watch("AddOneClick");
				clickCount++;
				clickTimer.Change(clickInterval, period); // change
			}
		}
		protected void ClickTimerCallback(object state_)
		{
			clickCount = Math.Min(clickCount, maxClickCount);
			Watch($"CallBack at {clickCount} click");
			if (clickCount > 0)
			{
				if (actions[clickCount - 1] != null)
				{
					// Use BeginInvoke to marshal the click event handling code to the main UI thread
					control.BeginInvoke(actions[clickCount - 1]);
				}
			}
			// Reset the click count
			clickCount = 0;
			isCounting = false;
		}
	}

	#region Developing

	

	#endregion


}
