# Tilang

Tilang is an experimental interpreted programming language implemented in C# on .NET 8.

The project includes a tokenizer, expression analyzer, runtime processor, stack model, type system, built-in functions, custom data structures, and a small execution pipeline for running Tilang source code.

> Status: active prototype / learning project. The interpreter builds successfully, but the developer runner currently uses a hardcoded script path in `Program.cs`, and automated tests are not yet included.

## Table of Contents

- [Features](#features)
- [Project Structure](#project-structure)
- [Requirements](#requirements)
- [Build](#build)
- [Run](#run)
- [Language Quick Start](#language-quick-start)
- [Syntax Guide](#syntax-guide)
- [Built-in APIs](#built-in-apis)
- [Runtime Architecture](#runtime-architecture)
- [Current Limitations](#current-limitations)
- [Roadmap Ideas](#roadmap-ideas)

## Features

- Interpreted language runtime written in C#.
- Primitive types: `int`, `float`, `bool`, `char`, `string`, `null`, and `func`.
- Typed variables, constants, and default values.
- Arrays with indexing and mutation helpers.
- User-defined `type` structures with properties and methods.
- Functions with typed parameters, return types, overload-style lookup by argument types, and default parameter values.
- Function references and lambda expressions.
- `if`, `else if`, `else`, `while`, and `for` control flow.
- `break` and `continue` support inside loops.
- `try`, `catch`, and `finally` blocks.
- Basic async function execution with `async` and `await`.
- Console input/output through `Sys.out` and `Sys.in`.
- Built-in helpers such as `len`, `add`, `remove`, `toInt`, `toFloat`, `toString`, `toCharArray`, `getCharCode`, and `throw`.

## Project Structure

```text
.
├── README.md
├── Tilang-project.sln
└── Tilang-project
    ├── Program.cs
    ├── Tilang-project.csproj
    ├── tests.ti
    ├── std-string-library.ti
    ├── Engine
    │   ├── Processors
    │   ├── Services
    │   ├── Stack
    │   ├── Structs
    │   ├── Syntax
    │   ├── Tilang_Keywords
    │   ├── Tilang_Pipeline
    │   └── Tilang_TypeSystem
    └── Utils
        ├── Background_Functions
        ├── String_Extentions
        └── Tilang_Console
```

### Important Directories

- `Tilang-project/Program.cs` - current console runner used during development.
- `Tilang-project/Engine/Syntax` - line splitting, tokenization, expression parsing, ternary parsing, and expression tokenization.
- `Tilang-project/Engine/Processors` - main interpreter execution logic for statements, functions, loops, conditionals, and error handling.
- `Tilang-project/Engine/Stack` - runtime variable/function stack and scope cleanup.
- `Tilang-project/Engine/Structs` - runtime representations for variables, arrays, functions, function pointers, and custom structures.
- `Tilang-project/Engine/Tilang_TypeSystem` - primitive type detection, parsing, casting, array typing, and custom type registration.
- `Tilang-project/Utils/Background_Functions` - built-in Tilang functions.
- `Tilang-project/Utils/Tilang_Console` - console input/output bridge.
- `Tilang-project/std-string-library.ti` - Tilang string helper library implemented in Tilang itself.

## Requirements

- [.NET SDK 8.0](https://dotnet.microsoft.com/download)

Check your installed SDK:

```bash
dotnet --version
```

## Build

From the repository root:

```bash
dotnet build Tilang-project.sln
```

The project currently builds as a .NET console application.

## Run

The current development runner reads a `.ti` file directly from `Program.cs`.

At the moment, `Program.cs` contains a hardcoded path:

```csharp
var codeFile = File.ReadAllText("/home/kanan/Desktop/Projects/Tilang-project/Tilang-project/tests.ti");
```

To run your own Tilang script today:

1. Open `Tilang-project/Program.cs`.
2. Change the `File.ReadAllText(...)` path to the `.ti` file you want to execute.
3. Run:

```bash
dotnet run --project Tilang-project/Tilang-project.csproj
```

Recommended future improvement:

```bash
dotnet run --project Tilang-project/Tilang-project.csproj -- path/to/script.ti
```

That command is the intended CLI shape, but command-line file arguments are not implemented yet.

## Language Quick Start

```ti
int count = 3;
string name = "Tilang";

function greet(value:string) <string> {
    return "Hello, " + value;
}

Sys.out.println(greet(name));

for(int i = 0; i < count; i += 1) {
    Sys.out.println("Loop: " + i);
}
```

## Syntax Guide

### Statements

Statements are generally separated with semicolons:

```ti
int x = 10;
x += 5;
Sys.out.println(x);
```

Block statements use `{ ... }` and do not require a trailing semicolon:

```ti
if(x > 10) {
    Sys.out.println("large");
}
```

### Comments

Comment syntax is not currently implemented in the tokenizer. Prefer keeping example scripts comment-free until comment support is added.

### Primitive Types

Supported primitive type names:

```text
int
float
bool
char
string
null
func
```

Examples:

```ti
int age = 20;
float price = 12.5;
bool enabled = true;
char letter = 'A';
string message = "hello";
```

### Variables

Tilang supports explicit `var` declarations:

```ti
var int score = 100;
var string label = "points";
```

It also supports typed shorthand declarations:

```ti
int score = 100;
string label = "points";
```

Variables without an initializer receive a default value based on their type:

```ti
int count;
bool active;
string text;
```

### Constants

Use `const` to create a constant:

```ti
const int maxRetries = 3;
```

Assigning to a constant later raises a runtime error.

### Operators

Arithmetic:

```ti
int a = 10;
int b = 3;

Sys.out.println(a + b);
Sys.out.println(a - b);
Sys.out.println(a * b);
Sys.out.println(a / b);
Sys.out.println(a % b);
```

Assignment:

```ti
int value = 10;
value += 5;
value -= 2;
value *= 3;
value /= 2;
```

Comparison and logic:

```ti
bool same = 10 == 10;
bool different = 10 != 5;
bool larger = 10 > 5;
bool valid = larger && same;
bool fallback = false || valid;
bool inverted = !valid;
```

### Strings and Characters

Strings use double quotes:

```ti
string greeting = "hello";
```

Characters use single quotes:

```ti
char first = 'h';
```

Indexing a string returns a `char`:

```ti
string word = "Tilang";
char first = word[0];
```

String concatenation uses `+`:

```ti
string name = "Tilang";
Sys.out.println("Hello " + name);
```

### Arrays

Array literals use square brackets:

```ti
int[] numbers = [1, 2, 3];
string[] names = ["ali", "sara"];
```

Access elements by index:

```ti
int first = numbers[0];
```

Use `len` to get the length:

```ti
Sys.out.println(len(numbers));
```

Use built-in helpers to mutate arrays:

```ti
add(numbers, 4);
remove(numbers, 0);
```

Nested indexing is supported by the runtime indexer:

```ti
int[][] matrix = [[1, 2], [3, 4]];
int value = matrix[1][0];
```

### Conditionals

```ti
int score = 85;

if(score >= 90) {
    Sys.out.println("A");
}
else if(score >= 80) {
    Sys.out.println("B");
}
else {
    Sys.out.println("C");
}
```

### Ternary Operator

```ti
int age = 20;
string status = age >= 18 ? "adult" : "minor";
```

### While Loops

```ti
int i = 0;

while(i < 3) {
    Sys.out.println(i);
    i += 1;
}
```

### For Loops

```ti
for(int i = 0; i < 3; i += 1) {
    Sys.out.println(i);
}
```

The current `for` implementation translates the loop into initialization, condition, body, and increment sections internally.

### Break and Continue

```ti
for(int i = 0; i < 10; i += 1) {
    if(i == 2) {
        continue;
    }

    if(i == 5) {
        break;
    }

    Sys.out.println(i);
}
```

### Functions

Functions use typed arguments and an explicit return type in angle brackets:

```ti
function add(a:int, b:int) <int> {
    return a + b;
}

int result = add(2, 3);
```

Functions can return any supported Tilang type:

```ti
function isEven(value:int) <bool> {
    return value % 2 == 0;
}
```

### Function Arguments with Defaults

Function parameters may include default values:

```ti
function greet(name:string = "world") <string> {
    return "Hello " + name;
}
```

### Function Overloading by Argument Types

Functions are stored internally using a signature-like definition made from the function name and argument types.

This allows multiple functions with the same name when the parameter types differ:

```ti
function show(value:int) <string> {
    return "int: " + value;
}

function show(value:string) <string> {
    return "string: " + value;
}
```

### Function References

The `func` type stores a function reference:

```ti
function add(a:int, b:int) <int> {
    return a + b;
}

func operation = add;
int result = operation(2, 3);
```

### Lambda Expressions

Lambda expressions use `=>` and can be assigned to `func` variables:

```ti
func double = (value:int) <int> => {
    return value * 2;
};

int result = double(4);
```

### Async Functions

Async functions can run on a background task:

```ti
async function work(value:int) <int> {
    return value * 2;
}

work(10);
```

Use `await` to wait for the result:

```ti
int result = await work(10);
```

### Custom Types

Use `type` to define a custom structure:

```ti
type Person {
    string name;
    int age;
}
```

Create an instance with property values:

```ti
Person user = Person { name = "Sara", age = 25 };
```

Access properties with `.`:

```ti
Sys.out.println(user.name);
```

Properties not provided during construction use their default type value.

### Methods on Custom Types

Custom types can contain functions:

```ti
type Counter {
    int value;

    function next() <int> {
        return value + 1;
    }
}

Counter counter = Counter { value = 10 };
Sys.out.println(counter.next());
```

### Try, Catch, and Finally

```ti
try {
    throw("Something went wrong");
}
catch(string error) {
    Sys.out.println(error);
}
finally {
    Sys.out.println("done");
}
```

`catch` can optionally declare a string variable to receive the error message.

## Built-in APIs

### Console APIs

Print without a newline:

```ti
Sys.out.print("Hello");
```

Print with a newline:

```ti
Sys.out.println("Hello");
```

Read one key:

```ti
char key = Sys.in.getKey();
```

Read one line:

```ti
string line = Sys.in.getLine();
```

### General Helpers

| Function | Description |
| --- | --- |
| `len(value)` | Returns the length of a string or array. |
| `add(array, item)` | Adds an item to an array. |
| `remove(array, item)` | Removes the first matching item from an array. |
| `remove(array, index)` | Removes the item at an index. |
| `toInt(value)` | Converts supported values to `int`. |
| `toFloat(value)` | Converts supported values to `float`. |
| `toString(value)` | Converts a value to `string`. |
| `toCharArray(value)` | Converts a string to `char[]`. |
| `getCharCode(char)` | Returns a character code as `int`. |
| `throw(message)` | Raises a runtime exception. |

### Standard String Library

`Tilang-project/std-string-library.ti` contains string helpers written in Tilang:

- `s_endsWith(value:string, target:string) <bool>`
- `s_startsWith(value:string, target:string) <bool>`
- `s_indexOf(value:string, target:char) <int>`
- `s_indexOf(value:string, target:string) <int>`

These functions are examples of implementing libraries in Tilang itself.

## Runtime Architecture

The interpreter is organized into several layers:

1. Source text is split into executable lines by the syntax analyzer.
2. Each line is converted into tokens.
3. The processor walks token lists and dispatches statements by keyword.
4. Expressions are tokenized and resolved against the processor stack.
5. Variables and functions are stored in `ProcessorStack`.
6. Function calls create child processors with scoped stacks.
7. Block execution can be routed through `Pipeline` for line-by-line processing.

### Key Components

- `SyntaxAnalyzer` - formats source text, splits lines, tokenizes declarations, assignments, blocks, functions, and type definitions.
- `ExprAnalyzer` - tokenizes and evaluates expressions, assignments, comparisons, and ternary operations.
- `Processor` - main runtime dispatcher for statements and blocks.
- `ProcessorStack` - stores variables and functions and performs scope cleanup.
- `TypeSystem` - parses primitive values, arrays, custom types, and default values.
- `TilangVariable` - common runtime value container.
- `TilangArray` - array representation and indexing logic.
- `TilangStructs` - custom structure representation and method dispatch.
- `TilangFunction` - user-defined function metadata and argument injection.
- `TilangFuncPtr` - function reference representation.
- `BackgroundFunctions` - built-in helper functions.
- `Tilang_System` - console input/output bridge.

## Current Limitations

- The runner in `Program.cs` uses a hardcoded file path instead of command-line arguments.
- There is no formal test project yet.
- `README` examples describe intended usage based on the current parser/runtime, but not every example is covered by automated tests yet.
- Build currently succeeds with nullable-reference warnings.
- Comment syntax is not implemented.
- Error messages are still low-level in several places.
- Operator precedence is limited compared with mature languages; use parentheses for clarity in complex expressions.
- The implementation is not sandboxed, so only run trusted Tilang scripts.
- Some names contain typos or inconsistent spelling from early development, such as `StringExtentions` and `OnStartProcesss`.

## Roadmap Ideas

- Add CLI support for `dotnet run -- path/to/file.ti`.
- Add a real test project for tokenizer, parser, expressions, functions, loops, structs, and built-ins.
- Add comment syntax.
- Improve diagnostics with line/column information.
- Reduce nullable warnings.
- Add package/library loading for `.ti` files.
- Add examples under an `examples/` directory.
- Add a REPL using the existing `Pipeline` class.
- Document and stabilize operator precedence.
- Add CI for build and tests.

## Contributing Notes

This is currently a compact experimental interpreter. When changing behavior, prefer small examples that demonstrate the syntax and add tests once a test project exists.

Suggested validation command:

```bash
dotnet build Tilang-project.sln
```

