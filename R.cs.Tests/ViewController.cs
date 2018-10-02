using System;
using System.Linq;
using UIKit;

namespace R.cs.Tests
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            foreach (var colorMethod in typeof(R.Color).GetMethods().Where(x => x.IsPublic && x.IsStatic))
            {
                var color = colorMethod.Invoke(null, new object[0]);
                if (color == null)
                    throw new Exception($"Color {colorMethod.Name} not loaded");
            }

            Console.WriteLine("Colors loaded successfully");

            foreach (var imageMethod in new [] { typeof(R.Image) }.Concat(typeof(R.Image).GetNestedTypes()).SelectMany(x => x.GetMethods()).Where(x => x.IsPublic && x.IsStatic))
            {
                var image = imageMethod.Invoke(null, new object[0]);
                if (image == null)
                    throw new Exception($"Image {imageMethod.Name} not loaded");
            }

            Console.WriteLine("Images loaded successfully");

            foreach (var storyboardMethod in typeof(R.Storyboard).GetMethods().Where(x => x.IsPublic && x.IsStatic))
            {
                var storyboard = storyboardMethod.Invoke(null, new object[] { null });
                if (storyboard == null)
                    throw new Exception($"Storyboard {storyboardMethod.Name} not loaded");
            }

            Console.WriteLine("Storyboards loaded successfully");

            foreach (var fontMethod in typeof(R.Font).GetMethods().Where(x => x.IsPublic && x.IsStatic))
            {
                var font = fontMethod.Invoke(null, new object[] { (nfloat)15 });
                if (font == null)
                    throw new Exception($"Font {fontMethod.Name} not loaded");
            }

            Console.WriteLine("Fonts loaded successfully");
        }
    }
}
