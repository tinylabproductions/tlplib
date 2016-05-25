using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Formats.JSON {
  abstract class JsonParserTestBase {
    public const string header = "header", footer = "footer";

    public static Either<JsonParserFailure, JsonParserResult<A>> p<A>(string json, JsonParser<A> parser) {
      return parser.parse($"{header}{json}{footer}", new JCursor(header.Length));
    }

    public static JsonParserResult<A> res<A>(int cursor, A result) {
      return new JsonParserResult<A>(new JCursor(cursor), result);
    }

    public static JsonParserFailure fail(int cursor, string message) {
      return new JsonParserFailure(new JCursor(cursor), message);
    }
  }
}
