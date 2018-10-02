# R.cs

*Currently in development*

This tool generates R.cs file to provide ids for resources same as Android does.

### Structure of R.cs:

* Assets:

    * ImageSets names in `R.Image`;
    * ColorSets names in `R.Color`;
* Storyboards names in `R.Storyboard`;
* Xibs names in `R.Xibs`;
* BundleResources names in `R.Resources`.


### Sample R.cs:
```
// This file generated with R.cs v.0.1.0.0

namespace MyProjectNamespace.Resources
{
    public static class R
    {
        public static class Image
        {
            public const string dots = "dots";
            public const string label = "label";
        }
        public static class Color
        {
            public const string TestColor = "TestColor";
        }
        public static class Storyboard
        {
            public const string Map = "Map";
            public const string Main = "Main";
            public const string Login = "Login";
            public const string LaunchScreen = "LaunchScreen";
        }
        public static class Xibs
        {
            public const string ChildCell = "ChildCell";
            public const string ParentCell = "ParentCell";
            public const string FilterHeaderCell = "FilterHeaderCell";
            public const string FilterCell = "FilterCell";
        }
        public static class Resources
        {
            public const string search = @"search";
            public const string maps = @"maps";
            public const string phone = @"phone";
            
            public static class Images
            {
                public const string ImageHolder = @"Images/ImageHolder";
                public const string GreenPen = @"Images/GreenPen";
                public const string Сalendar = @"Images/Сalendar";
                
                public static class StatsIcons
                {
                    public const string Calendar = @"Images/StatsIcons/Calendar";
                    public const string Hours = @"Images/StatsIcons/Hours";
                    public const string Tasks = @"Images/StatsIcons/Tasks";
                }
                public static class Statuses
                {
                    public const string default_marker_0 = @"Images/Statuses/default_marker_0";
                    public const string default_marker_1 = @"Images/Statuses/default_marker_1";
                    public const string default_marker_2 = @"Images/Statuses/default_marker_2";
                    public const string default_marker_3 = @"Images/Statuses/default_marker_3";
                    public const string default_marker_4 = @"Images/Statuses/default_marker_4";
                }
                public static class Others
                {
                    public const string bullet_blue = @"Images/Others/bullet_blue";
                    public const string bullet_green = @"Images/Others/bullet_green";
                    public const string bullet_red = @"Images/Others/bullet_red";
                    public const string home_marker = @"Images/Others/home_marker";
                    public const string zones_color = @"Images/Others/zones_color";
                    public const string default_user_icon = @"Images/Others/default_user_icon";
                    public const string user_icon = @"Images/Others/user_icon";
                }
            }
            public static class Fonts
            {
                public const string SFUIDisplay_Regular = @"Fonts/SFUIDisplay-Regular";
            }
        }
    }
}

```
