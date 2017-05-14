using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Test {
  public class Specification : TestBase {
    public static void describe(Act<SpecificationBuilder> buildTests) {
      var builder = new SpecificationBuilder();
      buildTests(builder);
      builder.execute();
    }
  }

  public class SpecTestFailedException : Exception {
    public SpecTestFailedException(
      ImmutableList<SpecificationBuilder.Test<Exception>> failures
    ) : base(
      "Following tests failed:\n\n" + failures.Select(f => {
        var err =
          F.opt(f.a as AssertionException).map(e => e.Message)
          .getOrElse(f.a.ToString);

        return $"### {f.name} ###\n{err}";
      }).mkString("\n")
    ) {}
  }

  public sealed class SpecificationBuilder {
    public struct Test<A> {
      public readonly string name;
      public readonly A a;

      public Test(string name, A a) {
        this.name = name;
        this.a = a;
      }

      public override string ToString() => $"{nameof(Test)}: {name} ({a})";
    }

    class Context {
      public static readonly Context root = new Context(
        "", ImmutableList<Action>.Empty, ImmutableList<Action>.Empty
      );

      public readonly string name;
      public readonly ImmutableList<Action> beforeEach, afterEach;

      public Context(string name, ImmutableList<Action> beforeEach, ImmutableList<Action> afterEach) {
        this.name = name;
        this.beforeEach = beforeEach;
        this.afterEach = afterEach;
      }

      public bool isRoot => name == root.name;

      public Context addBeforeEach(Action action) =>
        new Context(name, beforeEach.Add(action), afterEach);

      public Context addAfterEach(Action action) =>
        new Context(name, beforeEach, afterEach.Add(action));

      public Context child(string childName) => 
        new Context(concatName(name, childName, " and "), beforeEach, afterEach);

      public Test<Action> test(string testName, Action testAction) =>
        new Test<Action>(
          concatName(name, testName),
          () => {
            foreach (var a in beforeEach) a();
            testAction();
            foreach (var a in afterEach) a();
          }
        );

      static string concatName(string context, string name, string joiner = " ") =>
        context.nonEmptyOpt(true).fold(name, s => $"{s}{joiner}{name}");
    }

    readonly List<Test<Action>> tests = new List<Test<Action>>();

    Context currentContext = Context.root;

    public void when(string name, Action buildContext) {
      var prevContext = currentContext;
      currentContext = currentContext.child(currentContext.isRoot ? $"when {name}" : name);
      buildContext();
      currentContext = prevContext;
    }

    public void it(string name, Action testAction) {
      tests.Add(currentContext.test($"it {name}", testAction));
    }

    public ref A beforeEach<A>(A initialValue) => ref beforeEach(() => initialValue);

    public ref A beforeEach<A>(Fn<A> createInitialValue) {
      var r = new SimpleRef<A>(default(A));
      Action reinit = () => r.value = createInitialValue();
      currentContext = currentContext.addBeforeEach(reinit);
      return ref r.value;
    }

    public void beforeEach(Action action) {
      currentContext = currentContext.addBeforeEach(action);
    }

    public void afterEach(Action action) {
      currentContext = currentContext.addAfterEach(action);
    }

    public void execute() {
      var failures = tests.SelectMany(test => {
        try {
          test.a();
          return Enumerable.Empty<Test<Exception>>();
        }
        catch (Exception e) {
          return new Test<Exception>(test.name, e).Yield();
        }
      }).ToImmutableList();
      if (failures.nonEmpty()) throw new SpecTestFailedException(failures);
    }
  }
}