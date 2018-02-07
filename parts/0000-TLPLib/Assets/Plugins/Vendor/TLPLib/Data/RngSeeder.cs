namespace com.tinylabproductions.TLPLib.Data {
  /// <summary>
  /// Generates different random number generators by maintaining a seed variable.
  ///
  /// Imagine you want to spawn 10 objects at the same time, that use <see cref="Rng.now"/>
  /// internally. Their seeds are most likely to be identical, but we would probably want them
  /// to produce different random value streams.
  ///
  /// To achieve that you can use <see cref="RngSeeder"/>:
  /// <code><![CDATA[
  ///   static readonly RngSeeder seeder = new RngSeeder();
  ///   Rng rng = seeder.nextRng();
  /// ]]></code>
  ///
  /// Because <see cref="RngSeeder"/> is static, each instance of the object would get a
  /// <see cref="Rng"/> instance with a different seed.
  /// </summary>
  public class RngSeeder {
    static RngSeeder _global;
    public static RngSeeder global => _global ?? (_global = new RngSeeder());

    Rng.Seed currentSeed;
    Rng seedRng;

    public RngSeeder() : this(Rng.nowSeed) {}

    public RngSeeder(Rng.Seed currentSeed) {
      this.currentSeed = currentSeed;
      seedRng = new Rng(currentSeed);
    }

    public Rng.Seed nextSeed() {
      var seed = currentSeed;
      var newSeed = seedRng.nextULong(out seedRng);
      // seed for Rng cannot be 0 due to its implementation.
      if (newSeed == 0) newSeed = 42 /* what else could it be? */;
      currentSeed = new Rng.Seed(newSeed);
      return seed;
    }

    public Rng nextRng() => new Rng(nextSeed());
  }
}