Tiny Lab Productions Library
============================

This is library which we, [Tiny Lab Productions](www.tinylabproductions.com) use
in our Unity3D games. 

It contains various things, including but not limited to:

* Android utilities.
* BiMap.
* Promises & Futures, CoRoutine helpers.
* Various data structures.
* Iter library - allocation free enumerators.
* A bunch of extension methods.
* JSON parser/emmiter.
* Functional utilities: Option, Lazy, Tuple, Either, Try, Unit, co-variant functions and actions, rudimentary pattern matching.
* Reactive extensions: observables, reactive references, lists and list views.
* Tween utilities: mainly to make tweens type-safe.
* Various other misc utilities.

Requirements
------------

This library requires at least Unity 5.x with [Roslyn or new Mono compilers](https://bitbucket.org/alexzzzz/unity-c-5.0-and-6.0-integration/overview).

Because we need the new compiler this only works where Mono VM or il2cpp is used. It does not support Mono AOT.

Disclaimer
----------

You are free to use this for your own games. Patches and improvements are welcome. A mention in your game credits would be nice.

If you are considering using this it would be also nice if you contacted me (Artūras Šlajus, arturas@tinylabproductions.com) so we could create a community around this.

Known bugs
----------

Using in your project
---------------------

There are various ways this library can be used in your project, but I suggest 
using a git submodule and symlinking the required code into your assets folder.

Alternatively you can just copy the whole Assets folder to your project and be done with it.

### Defining compiler constants ###

There are bits of code that depend on third party libraries that you might not
want to use in your project. They are disabled by default via precompiler 
defines.

If you wish to use them, you need to define the constants in Unity3D (Menu Bar > Edit > Project Settings > Player > (your platform) > Other Settings > Scripting Define Symbols).

* GOTWEEN - if you are using [GoKit](https://github.com/prime31/GoKit).
* DFGUI - if you are using [Daikon Forge GUI](http://www.daikonforge.com/dfgui/). Beware that this also uses GOTWEEN as well.
* UNITY_TEST - to enable unit tests.

Questions and feedback
----------------------

Contact me at <arturas@tinylabproductions.com>.
