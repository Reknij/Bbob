# Bbob
One serverless blog framework.

# Install
Requires .Net 6+

Now only support install from nuget.
```
dotnet tool install -g bbob
```

# How to use?
About usage and plugin develop, please see the [Documents](https://reknij.github.io/Bbob.Doc/)

# Why make it?
Feel funny

# Last update 2022-4-11 v1.5.0
## Bbob
- `ArticleStatusDetect` add new status `notice`. If has `notice` in article. Will print the `notice` then ask user continue generate it or not.
- all message to console now has color.
- fix relation of plugins order recursion.
- `MarkdownParser` will provide `markdownPipelineBuilder` to modify markdown parser.
- `MarkdownParser` toc id now is based on text.
- `CopyToDist` refactoring.
- custom command support global.
- refactoring meta merge
- `enable` and `disable` support control all plugin one time.

## Bbob.Plugin
- `printConsole` now support color.
- add `HashPluginsLoaded`.
- optimize some functions.
- `PluginsLoadedOrder` change to `PluginsLoaded` and type `Dictionary<string, PluginJson>`
- `ConfigBbob` support assign. Will ignore null value.
- optimize `PluginCondition`.
- Plugin info add `version` field.
- PluginHelper add `readConsole` and `readConsoleKey` functions.
- `PluginHelper.registerMeta` support option to merge or replace.

## Bbob.Shared
- optimize `SharedLib`.

## BbobDefaultTheme
- background now unselectable.
- fix hash of location.
- support avatar.

# Recommended plugins
- [bbob-plugin-disqus](https://github.com/Reknij/bbob-plugin-disqus) Let your article have comment.
- [bbob-plugin-prerender](https://github.com/Reknij/bbob-plugin-prerender) Generate static page for seo.
- [bbob-plugin-mathjax](https://github.com/Reknij/bbob-plugin-mathjax) Let your blog articles support display mathematics syntax.
