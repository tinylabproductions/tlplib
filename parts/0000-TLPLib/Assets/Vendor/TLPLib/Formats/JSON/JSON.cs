using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Formats.JSON {
  public struct JCursor {
    public readonly int index;

    public JCursor(int index) {
      this.index = index;
    }

    public override string ToString() {
      return $"{nameof(JCursor)}({index})";
    }
  }

  public struct JsonParserFailure {
    /** Where did the error occured? */
    public readonly JCursor cursor;
    public readonly string error;

    public JsonParserFailure(JCursor cursor, string error) {
      this.cursor = cursor;
      this.error = error;
    }

    public override string ToString() {
      return $"{nameof(JsonParserFailure)}({cursor}, {error})";
    }
  }

  public struct JsonParserResult<A> {
    public readonly JCursor cursor;
    public readonly A parseResult;

    public JsonParserResult(JCursor cursor, A parseResult) {
      this.cursor = cursor;
      this.parseResult = parseResult;
    }

    public override string ToString() {
      return $"{nameof(JsonParserResult<A>)}({parseResult}, {cursor})";
    }
  }

  public abstract class JsonParser {
    public static bool isHex(char c) {
      switch (c) {
        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
        case 'a':
        case 'A':
        case 'b':
        case 'B':
        case 'c':
        case 'C':
        case 'd':
        case 'D':
        case 'e':
        case 'E':
        case 'f':
        case 'F':
          return true;
        default:
          return false;
      }
    }
  }

  public abstract class JsonParser<A> : JsonParser {
    public Either<JsonParserFailure, JsonParserResult<A>> parse(string jsonString) {
      return parse(jsonString, new JCursor(0));
    }

    public abstract Either<JsonParserFailure, JsonParserResult<A>> parse(
      string jsonString, JCursor cursor
    );

    public static Either<JsonParserFailure, JsonParserResult<A>> fail(int cursor, string error) {
      return Either<JsonParserFailure, JsonParserResult<A>>.Left(
        new JsonParserFailure(new JCursor(cursor), error)
      );
    }

    public static Either<JsonParserFailure, JsonParserResult<A>> result(int cursor, A result) {
      return Either<JsonParserFailure, JsonParserResult<A>>.Right(
        new JsonParserResult<A>(new JCursor(cursor), result)
      );
    }
  }
}
