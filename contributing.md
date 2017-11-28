## Getting started

**Getting started with Git and GitHub**

 * [Setting up Git for Windows and connecting to GitHub](http://help.github.com/win-set-up-git/)
 * [Forking a GitHub repository](http://help.github.com/fork-a-repo/)
 * [The simple guide to GIT guide](http://rogerdudler.github.com/git-guide/)

Once you're familiar with Git and GitHub, clone the repository.
The requirements are [Visual Studio 2017](https://www.visualstudio.com/) and [.net Core 1.0.3](https://www.microsoft.com/net/download/core#/sdk).

## Discussing ideas

* [Gitter Chatroom](https://gitter.im/dotliquid/dotliquid)
* [GitHub Issues](https://github.com/dotliquid/dotliquid/issues/new)

**The functionality is based as much as possible on existing functionality in [Liquid](https://shopify.github.io/liquid/)**

We prefer if each new feature must have been discussed first before submitting it in a PR.

## Coding conventions

 - We prefer spaces over tabs for indentation.
 - We have an [editorconfig](http://EditorConfig.org) [file](./.editorconfig) if you use an editor or plugin respecting it.
 
### Getting PRs merged

 - Describe in your message, **what you're fixing**. Reference the original issue, if possible.
 - Describe **how if any API-breaking changes** your PR may introduce.
 - State whether it's ready-to-review/-merge or if it's a WIP PR.

## Testing

 - Tests are mandatory for new functionality, please add some in the tests suite.
 
You can see the result either:
- in Visual Studio or other IDE supporting NUnit 3
- on [AppVeyor](https://ci.appveyor.com/project/tgjones/dotliquid) after the PR is submitted

## Documentation

The [docs](http://dotliquidmarkup.org/docs) sources are [here](./src/DotLiquid.Website/Views/Docs).
Don't hesitates to add some docs on existing features, or to write a new for your PR.
