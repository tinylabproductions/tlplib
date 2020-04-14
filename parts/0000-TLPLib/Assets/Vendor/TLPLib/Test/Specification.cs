using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using NUnit.Framework;
using pzd.lib.exts;

namespace com.tinylabproductions.TLPLib.Test {
  public class Specification : TestBase {
    public static void describe(Action<SpecificationBuilder> buildTests) {
      var builder = new SpecificationBuilder();
      buildTests(builder);
      builder.execute();
    }
  }

  public class ImplicitSpecification : Specification {
    SpecificationBuilder _currentBuilder;
    SpecificationBuilder currentBuilder {
      get {
        if (_currentBuilder == null) throw new IllegalStateException(
          "Implicit specification builder is not set! " +
          "You should be in a describe() block before calling this method."
        );
        return _currentBuilder;
      }
      set => _currentBuilder = value;
    }

    protected SpecificationBuilder.Scope when => currentBuilder.when;
    protected SpecificationBuilder.Scope then => currentBuilder.then;
    protected SpecificationBuilder.Scope on => currentBuilder.on;
    protected SpecificationBuilder.It it => currentBuilder.it;

    protected event Action beforeEach {
      add => currentBuilder.beforeEach += value;
      remove => currentBuilder.beforeEach -= value;
    }

    protected event Action afterEach {
      add => currentBuilder.afterEach += value;
      remove => currentBuilder.afterEach -= value;
    }

    protected SimpleRef<A> let<A>(A initialValue) => currentBuilder.let(initialValue);
    protected SimpleRef<A> let<A>(Func<A> initialValue) => currentBuilder.let(initialValue);

    protected void describe(Action buildTests) {
      describe(builder => {
        currentBuilder = builder;
        buildTests();
        currentBuilder = null;
      });
    }
  }

  public class SpecTestFailedException : Exception {
    public SpecTestFailedException(
      ImmutableList<SpecificationBuilder.Test<Exception>> failures
    ) : base(
      "Following tests failed:\n\n" + failures.Select(f => {
        var err = F.opt(f.a as AssertionException).fold(f.a.ToString, e => e.Message);
        return $"### {f.name}\n{err.Trim()}\n@ {f.stackFrame.fileAndLine()}\n";
      }).mkString("\n")
    ) { }
  }

  public sealed class SpecificationBuilder {
    public struct Test<A> {
      public readonly string name;
      public readonly A a;
      public readonly StackFrame stackFrame;

      public Test(string name, A a, StackFrame stackFrame) {
        this.name = name;
        this.a = a;
        this.stackFrame = stackFrame;
      }

      public override string ToString() => $"{nameof(Test)}: {name} ({a})";
    }

    class Context {
      public static readonly Context root = new Context(
        "", ImmutableList<Action>.Empty, ImmutableList<Action>.Empty
      );

      readonly string name;
      readonly ImmutableList<Action> beforeEach, afterEach;

      Context(string name, ImmutableList<Action> beforeEach, ImmutableList<Action> afterEach) {
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

      public Test<Action> test(string testName, Action testAction, StackFrame stackFrame) =>
        new Test<Action>(
          concatName(name, testName),
          () => {
            foreach (var a in beforeEach) a();
            testAction();
            foreach (var a in afterEach) a();
          },
          stackFrame
        );

      static string concatName(string context, string name, string joiner = " ") =>
        context.nonEmptyOpt(true).fold(name, s => $"{s}{joiner}{name}");
    }

    public class Scope {
      readonly SpecificationBuilder self;
      readonly string word;

      public Scope(SpecificationBuilder self, string word) {
        this.self = self;
        this.word = word;
      }

      public Action this[string name] {
        set {
          var prevContext = self.currentContext;
          self.currentContext = self.currentContext.child(
            self.currentContext.isRoot ? $"{word} {name}" : name
          );
          value();
          self.currentContext = prevContext;
        }
      }
    }

    public class It {
      readonly SpecificationBuilder self;
      public It(SpecificationBuilder self) { this.self = self; }

      public Action this[string name] {
        set {
          var stack = new StackFrame(1, true);
          self.tests.Add(self.currentContext.test($"it {name}", value, stack));
        }
      }
    }

    readonly List<Test<Action>> tests = new List<Test<Action>>();

    public readonly Scope when, on, then;
    public readonly It it;

    public SpecificationBuilder() {
      when = new Scope(this, "when");
      on = new Scope(this, "on");
      then = new Scope(this, "then");
      it = new It(this);
    }

    Context currentContext = Context.root;

    public SimpleRef<A> let<A>(A initialValue) => let(() => initialValue);

    /// <summary>A reference which gets set to provided value before each test.</summary>
    public SimpleRef<A> let<A>(Func<A> createInitialValue) {
      var r = new SimpleRef<A>(createInitialValue());
      Action reinit = () => r.value = createInitialValue();
      currentContext = currentContext.addBeforeEach(reinit);
      return r;
    }

    /// <summary>Code that is ran before each test.</summary>
    public event Action beforeEach {
      add { currentContext = currentContext.addBeforeEach(value); }
      remove { throw new NotImplementedException(); }
    }

    /// <summary>Code that is ran after each test.</summary>
    public event Action afterEach {
      add { currentContext = currentContext.addAfterEach(value); }
      remove { throw new NotImplementedException(); }
    }

    public void execute() {
      var failures = tests.SelectMany(test => {
        try {
          test.a();
          return Enumerable.Empty<Test<Exception>>();
        }
        catch (Exception e) {
          return new Test<Exception>(test.name, e, test.stackFrame).yield();
        }
      }).ToImmutableList();
      if (failures.nonEmpty()) throw new SpecTestFailedException(failures);
    }
  }
}