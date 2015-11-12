using System;
using System.Runtime.CompilerServices;

namespace NextionEditor
{
	public class GuiObjControl
	{
		public unsafe delegate void LoadHandler(ref InfoObject obj, byte id);
		public unsafe delegate byte InitHandler(ref InfoObject obj, byte id);
		public unsafe delegate byte RefreshHandler(ref InfoObject obj, byte id);

		public int A;
		public InitHandler OnInit;
		public LoadHandler OnLoad;
		public RefreshHandler OnRefresh;

		public GuiObjControl(InitHandler initHandler, RefreshHandler refHandler, LoadHandler loadHandler)
		{
			OnInit = initHandler;
			OnRefresh = refHandler;
			OnLoad = loadHandler;
		}
	}
}
