# R.cs

This tool generates R.cs file to provide resources.

## Examples:

#### Images:
R.cs will find all images in `Resource` directory, subdirectories, and Assets files.  
Note: group assets currently not supported.

By default: 
```
var image = UIImage.FromBundle("my-awesome-image")
```
With R.cs:
```
var image = R.Image.my_awesome_image()
```

#### Fonts:
By default:
```
var font = UIFont.FromName("Lato-Bold", 15);
```
With R.cs:
```
var font = R.Font.Lato_Bold(15);
```

#### ViewControllers:
By default:
```
var controller = (MapViewController)UIKit.UIStoryboard.FromName("Main", bundle).InstantiateViewController("MapViewController");
```
With R.cs:
```
var controller = R.ViewController.MapViewController();
```
Note: R.cs generates ViewController name using identifier, not a class name.

### Structure of R.cs:

* `R.Image` - Images in bundle and assets;
* `R.Color` - Colors in assets;
* `R.Font` - Fonts in bundle;
* `R.Storyboard` - storyboards in bundle;
* `R.Xib` - xibs in bundle;
* `R.ViewController` - controllers defined in storyboards.

### Sample R.cs:
```
// =============================================================================================
// This file generated by R.cs v0.2
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated
// Site: https://github.com/MAX-POLKOVNIK/R.cs 
// =============================================================================================


namespace R.cs.Tests
{
    public static class R
    {
        public static class Storyboard
        {
            public static UIKit.UIStoryboard LaunchScreen(Foundation.NSBundle bundle = null) => UIKit.UIStoryboard.FromName("LaunchScreen", bundle);
            public static UIKit.UIStoryboard Main(Foundation.NSBundle bundle = null) => UIKit.UIStoryboard.FromName("Main", bundle);
        }
        public static class Xib
        {
        }
        public static class Font
        {
            public static UIKit.UIFont Lato_Bold(System.nfloat size) => UIKit.UIFont.FromName("Lato-Bold", size);
            public static UIKit.UIFont Lato_Regular(System.nfloat size) => UIKit.UIFont.FromName("Lato-Regular", size);
            public static UIKit.UIFont SFUIDisplay_Regular(System.nfloat size) => UIKit.UIFont.FromName("SFUIDisplay-Regular", size);
        }
        public static class Color
        {
            public static UIKit.UIColor color_primary() => UIKit.UIColor.FromName("color_primary");
            public static UIKit.UIColor color_primary_dark() => UIKit.UIColor.FromName("color_primary_dark");
            public static UIKit.UIColor color_accent() => UIKit.UIColor.FromName("color_accent");
        }
        public static class Image
        {
            public static class ResourcesSubDirectory
            {
                public static UIKit.UIImage LeftArrow() => UIKit.UIImage.FromBundle(@"ResourcesSubDirectory/LeftArrow");
            }
        }
        
        public static class ViewController
        {
            public static R.cs.Tests.MapViewController MapViewController(Foundation.NSBundle bundle = null) => (R.cs.Tests.MapViewController) UIKit.UIStoryboard.FromName("Main", bundle).InstantiateViewController("MapViewController");
        }
    }
}

```
