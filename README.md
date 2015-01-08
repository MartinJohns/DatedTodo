DatedTodo
=========

NuGet package to give warnings and errors when todo items with a due date are due.

It is released on the NuGet gallery: https://www.nuget.org/packages/DatedTodo/


What is it for?
---------------

Ever got bothered that TODO-items in your code are just.. silently being ignored and lingering in your code? This NuGet package will parse your code for TODO-items that have a due date and emit warnings when it's soon due, or errors when the due date is reached.


Usage
-----

1. Add the NuGet package to your project.
2. Create TODO-items with following format: `// TODO due 2015-01-12 Very important task
3. Build your project!


How it is working
-----------------

The package contains an executable called **DatedTodo.exe**. Upon installing the package your project will be extended by a build target that is executed before each compilation. Within this build target the added executable will be executed, passing the path to your project. The executable will then using Roslyn parse the source code and located all TODO-items.

The warnings and the error will also emit the file, line and column of the TODO-item, so you can just double-click on the entry and Visual Studio will jump right away to it.

When the due date is reached, it will emit an error. Five days before the date it will emit a warning.


Quick Demo
----------

![alt tag](https://raw.github.com/MartinJohns/DatedTodo/master/DatedTodoDemo.png)


Future development
------------------

For a future release I want to make this tool configurable using a JSON file. I'm thinking here about regular expression that's used to parse the date, the timespan in which warnings will be writte and a flag to always create warnings instead of errors. __Comments, bug reports, ideas or just any form of contribution is highly welcomed!__
