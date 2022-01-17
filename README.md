# cloc.net

cloc.net is a .NET wrapper for [tokei](https://github.com/XAMPPRocky/tokei), a program that displays statistics about your code.

## Usage

cloc.net currently supports counting files in the specified directory and its subdirectories:

```csharp
using (var checker = new Checker())
{
    var files = checker.Count(@"C:\src", true);

    // Do something with the processed files
}
```