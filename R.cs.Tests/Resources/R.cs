// =============================================================================================
// This file generated by R.cs v0.3
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated
// Site: https://github.com/MAX-POLKOVNIK/R.cs 
// =============================================================================================


namespace R.cs.Tests
{
static class R
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
public static class ViewControllers
{
}
}
}
