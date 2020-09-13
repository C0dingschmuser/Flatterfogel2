#if UNITY_IOS
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using System.IO;

public class ExcemptFromEncryption : IPostprocessBuildWithReport // Will execute after XCode project is built
{
    public int callbackOrder { get { return 0; } }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.iOS) // Check if the build is for iOS 
        {
            string plistPath = report.summary.outputPath + "/Info.plist"; 

            PlistDocument plist = new PlistDocument(); // Read Info.plist file into memory
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            /*PlistElementDict NSAppTransportSecurity = rootDict.CreateDict("NSAppTransportSecurity");
            NSAppTransportSecurity.SetBoolean("NSAllowsArbitraryLoads", true);
            PlistElementDict NSExceptionDomains = NSAppTransportSecurity.CreateDict("NSExceptionDomains");
            PlistElementDict url1 = NSExceptionDomains.CreateDict("bruh.games");
            url1.SetBoolean("NSIncludesSubdomains", true);
            url1.SetBoolean("NSThirdPartyExceptionRequiresForwardSecrecy", false);
            url1.SetBoolean("NSExceptionAllowsInsecureHTTPLoads", true);
            url1.SetString("NSTemporaryExceptionMinimumTLSVersion", "TLSv1.0");*/

            File.WriteAllText(plistPath, plist.WriteToString()); // Override Info.plist
        }
    }
}
#endif