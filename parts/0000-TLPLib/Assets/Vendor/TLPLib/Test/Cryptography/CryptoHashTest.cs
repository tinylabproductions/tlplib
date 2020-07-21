using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.Cryptography {
  public class CryptoHashTest : ImplicitSpecification {
    [Test]
    public void sha256Test() => describe(() => {
      string sha256(string data) => CryptoHash.calculate(data, CryptoHash.Kind.SHA256).asString();
      when["simple email"] = () => {
        it["hashed string should be correct"] = () => {
          sha256("test@test.com").shouldEqual("f660ab912ec121d1b1e928a0bb4bc61b15f5ad44d5efdc4e1c92a25e99b8e44a");
        };
      };
      when["unicode symbol"] = () => {
        it["hashed string should be correct"] = () => {
          sha256("č").shouldEqual("391a70de1fdee94a0bbd51f7b11a7712c435b0d3c74ce5eadd51ba91ba006bf5");
        };
      };
      when["chinese sentence"] = () => {
        it["hashed string should be correct"] = () => {
          sha256("电脑死机了。").shouldEqual("fb8c48040cf7cf948e0d42dbf9ae9b65db89751ffbe282df7efbe275ab92c73a");
        };
      };
    });
  }
}
