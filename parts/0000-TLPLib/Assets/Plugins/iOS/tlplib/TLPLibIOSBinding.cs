using System.Runtime.InteropServices;
using GenerationAttributes;
using pzd.lib.collection;

namespace com.tinylabproductions.TLPLib.iOS {
  public static class TLPLibIOSBinding {
    /// <summary>
    /// Fetches arguments for the process from
    /// https://developer.apple.com/documentation/foundation/nsprocessinfo/1415596-arguments
    ///
    /// Beware that you can only pass these arguments from a debug build of the application.
    ///
    /// So if you have a production .ipa file, the process of launching it with command line interface (CLI)
    /// arguments is this:
    ///
    /// 1. Resign the .ipa file using the DEBUG certificate that includes the device on which you are going to run
    ///    the application. fastlane tool is useful for this: https://docs.fastlane.tools/actions/resign/
    ///
    ///    Beware that at the time of writing this fastlane fails on Ruby 3.x, so you need to use 2.7.2. A tool like
    ///    [rbenv](https://github.com/rbenv/rbenv) is your friend here.
    ///
    ///    Example:
    ///    /// <code><![CDATA[
    ///      bundle exec fastlane run resign ipa:your.ipa signing_identity:"your dev identity" \
    ///        provisioning_profile:"your-dev.mobileprovision"
    ///    ]]></code>
    ///
    ///    You can find your signing identity name by running `security find-identity -v -p codesigning`.
    ///
    ///    Provisioning profiles that you already imported to XCode are stored in
    ///    "~/Library/MobileDevice/Provisioning Profiles".
    ///
    /// 2. Use the [ipa-deploy](https://github.com/floatinghotpot/ipa-deploy) tool to launch the modified .ipa.
    ///
    ///    You can pass it the arguments of [ios-deploy](https://github.com/ios-control/ios-deploy) and it will pass
    ///    those along for you.
    ///
    ///    Example:
    ///    /// <code><![CDATA[
    ///      ipa-deploy your.ipa --debug --args "your command line arguments go here"
    ///    ]]></code>
    ///
    ///    BEWARE: the arguments are separated by space by the ios-deploy.
    /// </summary>
    [LazyProperty] public static ImmutableArrayC<string> cliArguments {
      get {
        var count = tlplibGetCliArgumentCount();
        if (count == 0) return ImmutableArrayC<string>.empty;

        var args = new string[count];
        for (var idx = 0ul; idx < count; idx++) {
          args[idx] = tlplibGetCliArgument(idx);
        }

        return ImmutableArrayC.move(args);
      }
    }

    [DllImport("__Internal")] static extern ulong tlplibGetCliArgumentCount();
    [DllImport("__Internal")] static extern string tlplibGetCliArgument(ulong index);
  }
}