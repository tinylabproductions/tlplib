import java.io._

object TupleData {
  def paramsRange(params: Int) = 1 to params
  def genericParameterNames(genericParamPrefix: String, paramsRange: Range) =
    paramsRange.map(n => s"$genericParamPrefix$n")
  def typeSigGenerics(genericParameterNames: Seq[String]) =
    genericParameterNames.mkString(", ")
  def fullType(typename: String, typeSigGenerics: String) = s"$typename<$typeSigGenerics>"
}

class TupleData(
  typename: String, genericParamPrefix: String, params: Int,
  nextTuple: => Option[TupleData]
) {
  val paramsRange = TupleData.paramsRange(params)
  val genericParameterNames =
    TupleData.genericParameterNames(genericParamPrefix, paramsRange)
  val typeSigGenerics = TupleData.typeSigGenerics(genericParameterNames)
  val fullType = TupleData.fullType(typename, typeSigGenerics)
  val paramArgNames = paramsRange.map(n => s"p$n")
  val paramArgNamesS = paramArgNames.mkString(", ")
  val paramArgs = genericParameterNames.zip(paramArgNames).map {
    case (type_, name) => s"$type_ $name"
  }
  val paramArgsS = paramArgs.mkString(", ")
  val propNames = paramsRange.map(n => s"_$n")
  val propArgsS = propNames.mkString(", ")
  val props = genericParameterNames.zip(propNames).map {
    case (type_, prop) => s"public readonly $type_ $prop;"
  }
  val propsS = props.mkString(" ")
  val constructorSetters = propNames.zip(paramArgNames).map {
    case (prop, arg) => s"$prop = $arg"
  }
  val constructorSettersS = constructorSetters.mkString("; ")
  val toStringFmt = propNames.map(n => s"{$n}")
  val toStringFmtS = toStringFmt.mkString(",")

  val equals = genericParameterNames.zip(propNames).map {
    case (type_, prop) =>
      s"Smooth.Collections.EqComparer<$type_>.Default.Equals($prop, t.$prop)"
  }
  val equalsS = equals.mkString(" &&\n")

  val hash = genericParameterNames.zip(propNames).map {
    case (type_, prop) =>
      s"hash = 29 * hash + Smooth.Collections.EqComparer<$type_>.Default.GetHashCode($prop);"
  }
  val hashS = hash.mkString("\n")

  val compareTo = genericParameterNames.zip(propNames).map {
    case (type_, prop) =>
      s"c = Smooth.Collections.Comparer<$type_>.Default.Compare($prop, other.$prop);"
  }
  val compareToS = compareTo.mkString(" if (c != 0) { return c; }\n")

  lazy val adder = nextTuple.map { nextT =>
    val lastP = nextT.genericParameterNames.last
    s"public ${nextT.fullType} add<$lastP>($lastP a) { " +
      s"return new ${nextT.fullType}($propArgsS, a);" +
    "}"
  }
  lazy val adderS = adder.getOrElse("")

  lazy val unshifter = nextTuple.map { nextT =>
    val firstP = nextT.genericParameterNames.last
    val nextTTypeSigGenerics = TupleData.typeSigGenerics({
      val types = nextT.genericParameterNames
      types.last +: types.dropRight(1)
    })
    val nextTFullType = TupleData.fullType(typename, nextTTypeSigGenerics)
    s"public $nextTFullType unshift<$firstP>($firstP a) { " +
      s"return new $nextTFullType(a, $propArgsS);" +
    "}"
  }
  lazy val unshifterS = unshifter.getOrElse("")

  def fCsStr =
    s"public static $fullType t<$typeSigGenerics>($paramArgsS) " +
    s"{ return new $fullType($paramArgNamesS); }"

  def tupleCsStr =
    s"""
[Serializable] public
#if UNITY_IOS
  class
#else
  struct
#endif
       $fullType :
IComparable<$fullType>, IEquatable<$fullType> {
  $propsS

  public $typename($paramArgsS)
    { $constructorSettersS; }

  // Unapply.
  public void ua(Act<$typeSigGenerics> f) { f($propArgsS); }

  // Unapply with function.
  public R ua<R>(Fn<$typeSigGenerics, R> f) { return f($propArgsS); }

  $adderS

  $unshifterS

  public override string ToString() {
    return $$"($toStringFmtS)";
  }

  public override bool Equals(object o) {
    return o is $fullType && this.Equals(($fullType) o);
  }

  public bool Equals($fullType t) {
    return $equalsS;
  }

  public override int GetHashCode() {
    unchecked {
      int hash = 17;
      $hashS
      return hash;
    }
  }

  public int CompareTo($fullType other) {
    int c;
    $compareToS
    return c;
  }

  public static bool operator == ($fullType lhs, $fullType rhs) {
    return lhs.Equals(rhs);
  }

  public static bool operator != ($fullType lhs, $fullType rhs) {
    return !lhs.Equals(rhs);
  }

  public static bool operator > ($fullType lhs, $fullType rhs) {
    return lhs.CompareTo(rhs) > 0;
  }

  public static bool operator < ($fullType lhs, $fullType rhs) {
    return lhs.CompareTo(rhs) < 0;
  }

  public static bool operator >= ($fullType lhs, $fullType rhs) {
    return lhs.CompareTo(rhs) >= 0;
  }

  public static bool operator <= ($fullType lhs, $fullType rhs) {
    return lhs.CompareTo(rhs) <= 0;
  }
}"""

  def eitherCsStr = {
    val tupledReturnType = s"Either<ImmutableList<A>, Tpl<$typeSigGenerics>>"

    def tupleRights =
"""
  foreach (var b in e1.rightValue)
    foreach (var c in e2.rightValue)
      return Either<ImmutableList<A>, Tpl<P1, P2>>.Right(F.t(b, c));
"""
    def addTupleRights =
s"""
  foreach (var tpl in e1.rightValue)
    foreach (var value in e2.rightValue)
      return $tupledReturnType.Right(tpl.add(value));
"""

    def addErrors =
s"""
  var arr = e1.leftValue.fold(ImmutableList<A>.Empty, _ => _);
  foreach (var a in e2.leftValue) arr = arr.Add(a);
  return $tupledReturnType.Left(arr);
"""

    def addMultiErrors =
s"""
  var arr = e1.leftValue.fold(ImmutableList<A>.Empty, _ => _);
  foreach (var arr2 in e2.leftValue)
    foreach (var a in arr2) arr = arr.Add(a);
  return $tupledReturnType.Left(arr);
"""

    params match {
      case 1 => s"""
public static Either<ImmutableList<A>, B> asValidation<A, B>(
  this Either<A, B> e1
) { return e1.mapLeft(ImmutableList.Create); }
      """
      case 2 => s"""
public static Either<ImmutableList<A>, Tpl<P1, P2>> validateAnd<A, P1, P2>(
  this Either<A, P1> e1, Either<A, P2> e2
) {
  $tupleRights
  var arr = ImmutableList<A>.Empty;
  foreach (var a in e1.leftValue) arr = arr.Add(a);
  foreach (var a in e2.leftValue) arr = arr.Add(a);
  return Either<ImmutableList<A>, Tpl<P1, P2>>.Left(arr);
}

public static Either<ImmutableList<A>, Tpl<P1, P2>> and<A, P1, P2>(
  this Either<ImmutableList<A>, P1> e1, Either<A, P2> e2
) {
  $tupleRights
  $addErrors
}

public static Either<ImmutableList<A>, Tpl<P1, P2>> and<A, P1, P2>(
  this Either<ImmutableList<A>, P1> e1, Either<ImmutableList<A>, P2> e2
) {
  $tupleRights
  $addMultiErrors
}
     """
      case _ =>
        val inputTplGenParams = TupleData.typeSigGenerics(genericParameterNames.dropRight(1))
        s"""
public static $tupledReturnType and<A, $typeSigGenerics>(
  this Either<ImmutableList<A>, Tpl<$inputTplGenParams>> e1,
  Either<A, ${genericParameterNames.last}> e2
) {
  $addTupleRights
  $addErrors
}

public static $tupledReturnType and<A, $typeSigGenerics>(
  this Either<ImmutableList<A>, Tpl<$inputTplGenParams>> e1,
  Either<ImmutableList<A>, ${genericParameterNames.last}> e2
) {
  $addTupleRights
  $addMultiErrors
}
       """
    }
  }
}

val range = 1 to 22
val tuples: IndexedSeq[TupleData] = range.map { i => new TupleData("Tpl", "P", i, {
  if (i == range.end) None else Some(tuples(i))
}) }

val tupleCs = new PrintWriter("Tuple.cs")
val fCs = new PrintWriter("F_TupleFunctions.cs")
val eitherValidationCs = new PrintWriter("Either_Validation.cs")

val AutogenHeader = "/* This code is autogenerated from tuples.cs */"
def TupleBasedFileHeader(using: String="") =
  s"""$AutogenHeader
     |using System;
     |$using
     |
     |namespace com.tinylabproductions.TLPLib.Functional {
  """.stripMargin
val TupleBasedFileFooter =
  """  }
    |}
  """.stripMargin

def file(w: PrintWriter, header: String, f: TupleData => String, footer: String) = {
  w.println(header)
  tuples.map(f).foreach(w.println)
  w.println(footer)
}

file(
  tupleCs,
  s"""$AutogenHeader
     |namespace System {""".stripMargin,
  _.tupleCsStr,
  "}"
)

file(
  fCs,
  s"""${TupleBasedFileHeader()}
     |  public static partial class F {
  """.stripMargin,
  _.fCsStr,
  TupleBasedFileFooter
)

file(
  eitherValidationCs,
  s"""${TupleBasedFileHeader("using System.Collections.Immutable;")}
     |  public static class EitherValidationExts {
  """.stripMargin,
  _.eitherCsStr,
  TupleBasedFileFooter
)

tupleCs.close()
fCs.close()
eitherValidationCs.close()