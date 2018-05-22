# watch-fs

## What does it do?

Watches one or more folders for file changes and outputs which files were changed as it happens.It's not rocket-science, but it helps to debug why, for example, your `jest -w` command keeps re-running tests even though you're not changing specs or code files.

## Runs on...?
Written in .net, compiles against dotnetcore and dotnet 4.5.2. Run with dotnet core by issuing: `dotnet watch-fs.dll {path}...`

## Building
1. Ensure you have dotnet build tools (dotnet core or msbuild however you get it)
2. run `npm install`
3. run `npm run build`
