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

# Last update 2022-3-29 v1.4.2
## Bbob
- `BuildWebArticleJson` fix same name articles.
- Some build-in plugin add custom command to modify config. Example, `bbob run categoryprocess config mode folder` to change `CategoryProcess` plugin config.mode to `folder`.
- `SkipIfDraft` refactoring to `ArticleStatusDetect`, support `important` field in article. If article have `important` field in front matter, will stop generation.
- Fixed preview listening port still using the next port even if the port is not already in use
- optimize command message show.
- `add` and `remove` command will auto set value to uppper.
- `enable` and `disable` command will auto set value to uppper. If start with 'plugin-' or 'theme-' will auto add 'bbob-' to front of value.
- new command `run`. will enter mode of target plugin to run custom command.
- optimize help message.
- Load plugin will check interface version.
- optimize preview command, support `url` option.

## Bbob.Plugin
- register and get object support option.
- PluginHelper.savePluginJsonConfig add option `JsonSerializerOptions`. Default write indented.
- refactoring unsafe fields.
- add PluginsLoadedOrder field.
- add `registerCustomCommand` function.
- add `isTargetPluginLoaded` function.

## BbobDefaultTheme
- add dark mode.
- refactoring ui.
- support meta to configurate background image or video, welcome text and description.
- article page front have information now.
- fix title position. 
- fix footer position.

# Recommended plugins
- [bbob-plugin-disqus](https://github.com/Reknij/bbob-plugin-disqus) Let your article have comment.
- [bbob-plugin-prerender](https://github.com/Reknij/bbob-plugin-prerender) Generate static page for seo.
