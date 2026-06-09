using UIKit;

namespace PMCSsE_Frontend_AvaloniaUI.iOS
{
    /// <summary>
    /// iOS 平台应用程序入口类。
    /// </summary>
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}
