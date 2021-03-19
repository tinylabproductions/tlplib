#if UNITY_IOS
using System.Runtime.InteropServices;
using GenerationAttributes;
using pzd.lib.collection;

namespace com.tinylabproductions.TLPLib.iOS {
  public static class TLPLibIOSBinding {
    /// <summary>
    /// Fetches arguments for the process from
    /// https://developer.apple.com/documentation/foundation/nsprocessinfo/1415596-arguments
    ///
    /// The first argument will be a path to the process executable, for example:
    /// /var/containers/Bundle/Application/GUID/your_bundle_id.app/your_bundle_id 
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
    /// 2. Launch the modified .ipa.
    ///
    ///    Method a)
    ///      Use the [ios-deploy](https://github.com/ios-control/ios-deploy) tool.
    ///
    ///      From our experience this gives you an attached debugger and fast transmission of application logs from
    ///      the device to the terminal. 
    ///
    ///      Example:
    ///      /// <code><![CDATA[
    ///        # unzip the package
    ///        unzip your.ipa
    ///        # deploy & launch the bundle
    ///        #
    ///        # The argument parser supports quotes, so --foo="bar baz" will be passed as "--foo=bar baz" to C#.
    ///        #
    ///        # It uses https://docs.python.org/3/library/shlex.html#shlex.split to parse the arguments and errors
    ///        # out if you use single quotes (') because internally it sticks your arguments into a Python script in
    ///        # the following way:
    ///        #
    ///        #    args-arr = args_arr + shlex.split('your args string goes here')
    ///        ios-deploy --bundle Payload/your_bundle_id.app --debug --args "your command line arguments go here"
    ///      ]]></code>
    ///
    ///    Method b)
    ///      Use the [ideviceinstaller](https://github.com/libimobiledevice/ideviceinstaller) shell tools.
    ///
    ///      For some reason the transmission of application logs from the device to the terminal is a lot slower than
    ///      using method a. 
    ///
    ///      Example:
    ///      /// <code><![CDATA[
    ///        # install the package
    ///        ideviceinstaller -i your.ipa
    ///        # launch the bundle
    ///        idevicedebug run your_bundle_id your command line arguments go here
    ///      ]]></code>
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
#endif