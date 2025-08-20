using McMikeEngine.Graphics;
using McMikeEngine.Input.Mouse;
using System.Xml.Serialization;

using McMikeEngine.WPF;
using McMikeEngine.Utils.Bindings;

namespace McMikeEngine.UI.Controls
{
    public class Button : Panel
    {
        private ICommand mCommand;
        [XmlAttribute]
        public ICommand Command
        {
            get => mCommand;
            set
            {
                mCommand = value;
                OnPropertyChanged(nameof(Command));
            }
        }

        private Color mHoverColor;
        [XmlAttribute]
        public Color HoverColor
        {
            get => mHoverColor;
            set
            {
                mHoverColor = value;
                OnPropertyChanged(nameof(HoverColor));
            }
        }

        private Color mDownColor;
        [XmlAttribute]
        public Color DownColor
        {
            get => mDownColor;
            set
            {
                mDownColor = value;
                OnPropertyChanged(nameof(DownColor));
            }
        }

        public Button()
        {
            NormalColor = new Color(0.5f);
            HoverColor = new Color(0.7f);
            DownColor = new Color(0.9f);

            Mouse.IsEnabled = true;
            Mouse.Down += OnMouseDown;
            Mouse.Up += OnMouseUp;
            Mouse.Enter += OnMouseEnter;
            Mouse.Exit += OnMouseExit;
            Mouse.Click += OnMouseClick;

            IsFocusable = true;
        }

        public Button(Button button) : base(button)
        {
            mHoverColor = button.mHoverColor.Copy();
            mDownColor = button.mDownColor.Copy();

            Mouse.IsEnabled = true;
            Mouse.Down += OnMouseDown;
            Mouse.Up += OnMouseUp;
            Mouse.Enter += OnMouseEnter;
            Mouse.Exit += OnMouseExit;
            Mouse.Click += OnMouseClick;
        }

        protected virtual void OnMouseEnter(object sender, UIMouseEventArgs args)
        {
            Color = IsActive ? HoverColor : DisabledColor;
            Refresh();
        }

        protected virtual void OnMouseExit(object sender, UIMouseEventArgs args)
        {
            Color = IsActive ? NormalColor : DisabledColor;
            Refresh();
        }

        protected virtual void OnMouseDown(object sender, UIMouseEventArgs args)
        {
            if (args.Button == MouseButton.LEFT)
            {
                Color = IsActive ? mDownColor : DisabledColor;
                Refresh();
            }
        }

        protected virtual void OnMouseUp(object sender, UIMouseEventArgs args)
        {
            Color = IsActive ? HoverColor : DisabledColor;
            Refresh();
        }

        protected virtual void OnMouseClick(object sender, UIMouseEventArgs args)
        {
            if (IsActive)
            {
                Command?.Execute();

                Clicked?.Invoke(this, true);
            }
        }

        public delegate void ButtonEventHandler(object sender, bool isClicked);

        public event ButtonEventHandler Clicked;

        public new Button Copy()
        {
            return new Button(this);
        }
    }
}
