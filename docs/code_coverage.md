It was easy to add a code coverage badge
1. Added the needed packages
   ```
   dotnet add package coverlet.collector
   dotnet add package coverlet.msbuild
   ```
1. Add [code-coverage.yml](../.github/workflows/code-coverage.yml)
   - codecov.io folks have a handy [action](https://github.com/marketplace/actions/codecov)
   - I could have added it to ci.ym, but I figure one OS is enough, and I wanted to with a debug build
1. Setting up an account was easy
1. Info for the badge can be found in the settings page

The code in `ObjectRenders` should be easy to get coverage on, some of the code in `Displayer.cs` is a little harder since it is going to do thing like start up processes or attempt to print to images to the console.

To get local copy of the code coverage just run `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover` in `ConsoleMarkdownRenderer.Tests`