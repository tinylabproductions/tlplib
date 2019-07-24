using System.Collections.Immutable;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.exts;

namespace com.tinylabproductions.TLPLib.Concurrent.unity_web_request {
  /// <summary>
  /// https://en.wikipedia.org/wiki/List_of_HTTP_status_codes
  /// </summary>
  [Record(GenerateToString = false)]
  public partial class AcceptedResponseCodes {
    [PublicAPI] public readonly ImmutableArray<long> codes;

    [PublicAPI]
    public bool contains(long responseCode) =>
      codes.Contains(responseCode);

    public override string ToString() =>
      $"{nameof(AcceptedResponseCodes)}[{codes.mkStringEnum(", ", "", "")}]";
    
    [PublicAPI]
    public static readonly AcceptedResponseCodes
      _20X = new AcceptedResponseCodes(ImmutableArray.Create(
        200L, 201, 202, 203, 204, 205, 206, 207, 208, 226
      ));
  }
}