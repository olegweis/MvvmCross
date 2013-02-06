// MvxTouchDialogViewController.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Interfaces;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Binding.Touch.Interfaces;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Touch.Interfaces;
using Cirrious.MvvmCross.Views;
using CrossUI.Touch.Dialog;
using CrossUI.Touch.Dialog.Elements;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Binding.Touch.Views;

namespace Cirrious.MvvmCross.Dialog.Touch
{
	public class EventSourceDialogViewController
		: DialogViewController
		, IViewControllerEventSource
	{
		protected EventSourceDialogViewController (UITableViewStyle style = UITableViewStyle.Grouped, 
		                                           RootElement root = null,
		                                           bool pushing = false)
			: base(style, root, pushing)
		{			
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			ViewWillDisappearCalled.Raise(this, animated);
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			ViewDidDisappearCalled.Raise(this, animated);
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			ViewWillAppearCalled.Raise(this, animated);
		}
		
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			ViewDidAppearCalled.Raise(this, animated);
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			ViewDidLoadCalled.Raise(this);
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				IsDisposingDisposeCalled.Raise(this);
			}
			base.Dispose (disposing);
		}
		
		public event EventHandler ViewDidLoadCalled;
		public event EventHandler<TypedEventArgs<bool>> ViewWillAppearCalled;
		public event EventHandler<TypedEventArgs<bool>> ViewDidAppearCalled;
		public event EventHandler<TypedEventArgs<bool>> ViewDidDisappearCalled;
		public event EventHandler<TypedEventArgs<bool>> ViewWillDisappearCalled;
		public event EventHandler IsDisposingDisposeCalled;
	}

	public class MvxDialogViewController
		: EventSourceDialogViewController
		, IMvxTouchView
	{
		protected MvxDialogViewController (UITableViewStyle style = UITableViewStyle.Grouped, 
		                                           RootElement root = null,
		                                           bool pushing = false)
			: base(style, root, pushing)
		{			
			var adapter = new MvxViewControllerAdapter(this);	
		}

		public virtual object DataContext { get;set; }
		
		public IMvxViewModel ViewModel
		{
			get { return (IMvxViewModel)DataContext; }
			set { DataContext = value; }
		}

		public bool IsVisible
		{
			get { return this.IsVisible(); }
		}
		
		public MvxShowViewModelRequest ShowRequest { get; set; }
	}

	public class MvxBindingDialogViewController
		: MvxDialogViewController
		, IMvxBindingTouchView
	{
		protected MvxBindingDialogViewController (UITableViewStyle style = UITableViewStyle.Grouped, 
		                                   RootElement root = null,
		                                   bool pushing = false)
			: base(style, root, pushing)
		{			
			var adapter = new MvxBindingViewControllerAdapter(this);	
		}
		
		private readonly List<IMvxUpdateableBinding> _bindings = new List<IMvxUpdateableBinding>();
		
		public List<IMvxUpdateableBinding> Bindings
		{
			get { return _bindings; }
		}

		#region Extra Binding helpers just for Elements
		
		protected T Bind<T>(T element, string bindingDescription)
		{
			return element.Bind(this, bindingDescription);
		}
		
		protected T Bind<T>(T element, IEnumerable<MvxBindingDescription> bindingDescription)
		{
			return element.Bind(this, bindingDescription);
		}
		
		#endregion
	}

    public class MvxTouchDialogViewController<TViewModel>
        : DialogViewController
          , IMvxTouchView<TViewModel>
          , IMvxBindingTouchView
        where TViewModel : class, IMvxViewModel
    {
        protected MvxTouchDialogViewController(MvxShowViewModelRequest request)
            : this(request, UITableViewStyle.Grouped, null, false)
        {
            ShowRequest = request;
        }

        protected MvxTouchDialogViewController(MvxShowViewModelRequest request, UITableViewStyle style, RootElement root,
                                               bool pushing)
            : base(style, root, pushing)
        {
            ShowRequest = request;
        }

        #region Shared code across all Touch ViewControllers
       
		private IMvxViewModel _viewModel;
		
		public Type ViewModelType
		{
			get { return typeof (TViewModel); }
		}

		public virtual object DataContext { get; set; }
		
		public TViewModel ViewModel
		{
			get { return (TViewModel)DataContext; }
			set { DataContext = value; }
		}
		
		IMvxViewModel IMvxView.ViewModel
		{
			get { return (IMvxViewModel)DataContext; }
			set { DataContext = value; }
		}

		public bool IsVisible
		{
			get { return this.IsVisible(); }
		}

        public MvxShowViewModelRequest ShowRequest { get; set; }

        protected virtual void OnViewModelChanged()
        {
        }

        public override void ViewDidLoad()
        {
            this.OnViewCreate();
            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
        }

        #endregion

        #region Shared area needed by all binding controllers

        private readonly List<IMvxUpdateableBinding> _bindings = new List<IMvxUpdateableBinding>();

        public List<IMvxUpdateableBinding> Bindings
        {
            get { return _bindings; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ClearBindings();
            }

            base.Dispose(disposing);
        }

#warning really need to think about how to handle ios6 once ViewDidUnload has been removed
        [Obsolete]
        public override void ViewDidUnload()
        {
            this.ClearBindings();
            this.OnViewDestroy();
            base.ViewDidUnload();
        }

        #endregion

        #region Extra Binding helpers just for Elements

        protected T Bind<T>(T element, string bindingDescription)
        {
            return element.Bind(this, bindingDescription);
        }

        protected T Bind<T>(T element, IEnumerable<MvxBindingDescription> bindingDescription)
        {
            return element.Bind(this, bindingDescription);
        }

        #endregion
    }
}