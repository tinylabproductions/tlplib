This directory is needed so that Intellij Rider would enable C# 7 support for this project.

Rider supports https://bitbucket.org/alexzzzz/unity-c-5.0-and-6.0-integration/src, but we use
our own compiler, therefore we do not have that directory.

To make Rider not complain about C# 7.0 features, we just create this directory, it picks it up
and is happy.